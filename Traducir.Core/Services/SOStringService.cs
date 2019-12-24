using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CsvHelper;
using Dapper;
using Microsoft.Extensions.Configuration;
using StackExchange.Profiling;
using Traducir.Core.Helpers;
using Traducir.Core.Models;
using Traducir.Core.Models.Enums;
using Traducir.Core.Models.Services;

namespace Traducir.Core.Services
{
    public interface ISOStringService
    {
        Task StoreNewStringsAsync(ImmutableArray<TransifexString> strings);

        Task<SOString> GetStringByIdAsync(int stringId);

        Task<ImmutableArray<SOString>> GetStringsAsync(Func<SOString, bool> predicate = null, bool includeEverything = false);

        Task<int> CountStringsAsync(Func<SOString, bool> predicate = null);

        Task<bool> CreateSuggestionAsync(int stringId, string suggestion, int userId, UserType userType, bool approve);

        Task<bool> ReviewSuggestionAsync(int suggestionId, bool approve, int userId, UserType userType, string host);

        Task UpdateStringsPushed();

        Task UpdateTranslationsFromSODB(bool overrideExisting);

        Task<bool> ManageUrgencyAsync(int stringId, bool isUrgent, int userId);

        Task<bool> ManageIgnoreAsync(int stringId, bool isIgnored, int userId, UserType userType);

        Task<bool> DeleteSuggestionAsync(int suggestionId, int userId);

        Task<ImmutableArray<SOStringSuggestion>> GetSuggestionsByUser(int userId, StringSuggestionState? state);

        Task<bool> ReplaceSuggestionAsync(int suggestionId, string suggestion, int userId);

        Task<int> GetStringIdBySuggestionId(int suggestionId);
    }

    public class SOStringService : ISOStringService
    {
        private readonly IDbService _dbService;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public SOStringService(IDbService dbService, IUserService userService, IConfiguration configuration)
        {
            _dbService = dbService;
            _userService = userService;
            _configuration = configuration;
        }

        private ImmutableArray<SOString> Strings { get; set; }

        private Dictionary<int, SOString> StringsById { get; set; }

        public async Task StoreNewStringsAsync(ImmutableArray<TransifexString> strings)
        {
            using (var db = _dbService.GetConnection())
            {
                await CreateTemporaryTable(db);
                using (MiniProfiler.Current.Step("Populate temp table"))
                {
                    var table = new DataTable();
                    table.Columns.Add("NormalizedKey", typeof(string));
                    table.Columns.Add("Key", typeof(string));
                    table.Columns.Add("Variant", typeof(string));
                    table.Columns.Add("OriginalString", typeof(string));
                    table.Columns.Add("Translation", typeof(string));

                    foreach (var s in strings)
                    {
                        table.Rows.Add(s.NormalizedKey, s.Key, s.Variant, s.Source, s.Translation);
                    }

                    var copyDb = (SqlConnection)db.WrappedConnection;
                    using (var copy = new SqlBulkCopy(copyDb))
                    {
                        copy.DestinationTableName = "dbo.ImportTable";
                        foreach (DataColumn c in table.Columns)
                        {
                            copy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(c.ColumnName, c.ColumnName));
                        }

                        copyDb.Open();
                        copy.WriteToServer(table);
                    }
                }

                using (MiniProfiler.Current.Step("Fixing normalized keys"))
                {
                    // once upon a time, someone used to host Traducir in an Ubuntu docker image. That same someone used
                    // OrderBy without specifying a comparer.
                    // A year later, this subject decided to use the Alpine image to host it... it's smaller, they thought
                    // it compiles and it runs... that must be safe!
                    // BUT! our hero never realized that Comparer<string>.Default (and even StringComparer.InvariantCultureIgnoreCase)
                    // order things different. So when that happened, trying to join by NormalizedKey caused this method to
                    // try to insert a bunch of already existing strings. Fortunately, we do have an index that doesn't allow two
                    // strings with the same Key.
                    // Aaanyway, as a sacrifice to the Gods (the old and the new), they were required to fix all the keys before
                    // running the pull. They initially wrote an admin route, but considering this is all in-memory and pretty fast,
                    // let's do it here every time this runs. We'll remove it. Eventually.
                    await FixNormalizedKeysAsync(db);
                }

                using (MiniProfiler.Current.Step("Delete strings"))
                {
                    await db.ExecuteAsync(@"
Insert Into StringHistory
            (StringId, HistoryTypeId, CreationDate)
Select    s.Id, {=Deleted}, @now
From      Strings s
Left Join ImportTable feed On feed.NormalizedKey = s.NormalizedKey
Where     s.DeletionDate Is Null
And       feed.NormalizedKey Is Null;

Update    s
Set       s.DeletionDate = @now
From      Strings s
Left Join ImportTable feed On feed.NormalizedKey = s.NormalizedKey
Where     s.DeletionDate Is Null
And       feed.NormalizedKey Is Null;", new { now = DateTime.UtcNow, StringHistoryType.Deleted });
                }

                using (MiniProfiler.Current.Step("Update strings"))
                {
                    await db.ExecuteAsync(@"
Insert Into StringHistory
            (StringId, HistoryTypeId, Comment, CreationDate)
Select s.Id, {=Updated}, Concat('Key Updated from ', s.[Key], ' to ', feed.[Key]), @now
From   Strings s
Join   ImportTable feed On feed.NormalizedKey = s.NormalizedKey
Where  s.[Key] <> feed.[Key];

Update s
Set    s.[Key] = feed.[Key], s.Variant = feed.Variant
From   Strings s
Join   ImportTable feed On feed.NormalizedKey = s.NormalizedKey
Where  s.[Key] <> feed.[Key];", new { now = DateTime.UtcNow, StringHistoryType.Updated });
                }

                using (MiniProfiler.Current.Step("Undelete strings"))
                {
                    await db.ExecuteAsync(@"
Insert Into StringHistory
            (StringId, HistoryTypeId, CreationDate)
Select s.Id, {=Undeleted}, @now
From   Strings s
Join   ImportTable feed On feed.NormalizedKey = s.NormalizedKey
Where  s.DeletionDate Is Not Null;

Update s
Set    s.DeletionDate = Null
From   Strings s
Join   ImportTable feed On feed.NormalizedKey = s.NormalizedKey
Where  s.DeletionDate Is Not Null;", new { now = DateTime.UtcNow, StringHistoryType.Undeleted });
                }

                using (MiniProfiler.Current.Step("Add new strings"))
                {
                    await db.ExecuteAsync(@"
Declare @NormalizedKeysToInsert Table (
    NormalizedKey VarChar(255) Not Null
);

Insert Into @NormalizedKeysToInsert
Select    feed.NormalizedKey
From      ImportTable feed
Left Join Strings s On s.NormalizedKey = feed.NormalizedKey
Where     s.NormalizedKey Is Null;

Insert Into Strings ([Key], NormalizedKey, FamilyKey, Variant, OriginalString, Translation, CreationDate)
Select [Key], feed.NormalizedKey, Left([Key], 32), feed.Variant, feed.OriginalString, feed.Translation, @now
From   ImportTable feed
Join   @NormalizedKeysToInsert s On s.NormalizedKey = feed.NormalizedKey;

Insert Into StringHistory
            (StringId, HistoryTypeId, CreationDate)
Select s.Id, {=Created}, @now
From   @NormalizedKeysToInsert i
Join   Strings s On s.NormalizedKey = i.NormalizedKey;", new { now = DateTime.UtcNow, StringHistoryType.Created });
                }
            }

            ExpireCache();
        }

        public async Task<ImmutableArray<SOString>> GetStringsAsync(Func<SOString, bool> predicate = null, bool includeEverything = false)
        {
            if (Strings == null || Strings.Length == 0)
            {
                await RefreshCacheAsync();
            }

            using (MiniProfiler.Current.Step("Filtering the strings"))
            {
                var matching = predicate == null ? Strings : Strings.Where(predicate);
                if (!includeEverything)
                {
                    matching = matching.Take(200);
                }

                return matching.ToImmutableArray();
            }
        }

        public async Task<int> CountStringsAsync(Func<SOString, bool> predicate = null)
        {
            if (Strings == null || Strings.Length == 0)
            {
                await RefreshCacheAsync();
            }

            if (predicate == null)
            {
                return Strings.Length;
            }

            using (MiniProfiler.Current.Step("Filtering the strings"))
            {
                return Strings.Count(predicate);
            }
        }

        public async Task<SOString> GetStringByIdAsync(int stringId)
        {
            if (Strings == null || Strings.Length == 0)
            {
                await RefreshCacheAsync();
            }

            if (StringsById.TryGetValue(stringId, out var res))
            {
                return res;
            }

            return null;
        }

        public async Task<bool> CreateSuggestionAsync(int stringId, string suggestion, int userId, UserType userType, bool approve)
        {
            using (var db = _dbService.GetConnection())
            {
                try
                {
                    var initialState = userType >= UserType.TrustedUser ? StringSuggestionState.ApprovedByTrustedUser : StringSuggestionState.Created;
                    var suggestionId = await db.QuerySingleOrDefaultAsync<int?>(@"
Declare @suggestionId Int;

Insert Into StringSuggestions
            (StringId, Suggestion, StateId, CreatedById, CreationDate)
Values      (@stringId, @suggestion, @state, @userId, @now);

Select @suggestionId = Scope_Identity();

Insert Into StringSuggestionHistory
            (StringSuggestionId, HistoryTypeId, UserId, Comment, CreationDate)
Values      (@suggestionID, {=HistoryCreated}, @userId, @comment, @now);

Select @suggestionId;", new
                    {
                        stringId,
                        suggestion,
                        StringSuggestionState.Created,
                        userId,
                        state = initialState,
                        now = DateTime.UtcNow,
                        comment = initialState == StringSuggestionState.ApprovedByTrustedUser ? "Created by a trusted user" : null,
                        HistoryCreated = StringSuggestionHistoryType.Created
                    });

                    if (approve)
                    {
                        await ReviewSuggestionAsync(suggestionId.Value, true, userId, userType, null);
                    }
                }
                catch (SqlException e) when (e.Number == 547)
                {
                    return false;
                }

                await RefreshCacheAsync(stringId);
                return true;
            }
        }

        public async Task<bool> ReviewSuggestionAsync(int suggestionId, bool approve, int userId, UserType userType, string host)
        {
            if (userType != UserType.TrustedUser && userType != UserType.Reviewer)
            {
                return false;
            }

            using (var db = _dbService.GetConnection())
            {
                // is this suggestion eligible for review?
                var suggestionData = await db.QueryFirstOrDefaultAsync<(int stringId, int ownerId)>(@"
Select StringId, CreatedById
From   StringSuggestions
Where  Id = @suggestionId
And    StateId In @validStates", new
                {
                    userId,
                    suggestionId,
                    validStates = userType == UserType.Reviewer ? new[] { StringSuggestionState.Created, StringSuggestionState.ApprovedByTrustedUser } : new[] { StringSuggestionState.Created }
                });

                if (suggestionData.stringId > 0)
                {
                    StringSuggestionState newState;
                    StringSuggestionHistoryType historyType;

                    if (userType == UserType.Reviewer)
                    {
                        if (approve)
                        {
                            newState = StringSuggestionState.ApprovedByReviewer;
                            historyType = StringSuggestionHistoryType.ApprovedByReviewer;
                        }
                        else
                        {
                            newState = StringSuggestionState.Rejected;
                            historyType = StringSuggestionHistoryType.RejectedByReviewer;
                        }
                    }
                    else
                    {
                        if (approve)
                        {
                            newState = StringSuggestionState.ApprovedByTrustedUser;
                            historyType = StringSuggestionHistoryType.ApprovedByTrusted;
                        }
                        else
                        {
                            newState = StringSuggestionState.Rejected;
                            historyType = StringSuggestionHistoryType.RejectedByTrusted;
                        }
                    }

                    string sql = @"
Declare @historyId Int;
Insert Into StringSuggestionHistory
            (StringSuggestionId, HistoryTypeId, UserId, CreationDate)
Values      (@suggestionId, @historyType, @userId, @now);
Select @historyId = Scope_Identity();

Update StringSuggestions
Set    StateId = @newState,
       LastStateUpdatedDate = @now,
       LastStateUpdatedById = @userId
Where  Id = @suggestionId;";
                    if (newState == StringSuggestionState.ApprovedByReviewer)
                    {
                        sql += @"
Update h
Set    h.Comment = Concat('Previous translation: ', str.Translation)
From   StringSuggestionHistory h
Join   StringSuggestions sug On sug.Id = h.StringSuggestionId
Join   Strings str On str.Id = sug.StringId
Where  h.Id = @historyId;

Update str
Set    str.Translation = sug.Suggestion,
       str.NeedsPush = 1,
       str.IsUrgent = 0
From   Strings str
Join   StringSuggestions sug On sug.StringId = str.Id
Where  sug.Id = @suggestionId;

-- useful to avoid joins down the line
Declare @stringId Int;
Select @stringId = StringId From StringSuggestions Where Id = @suggestionId;

Insert Into StringSuggestionHistory
            (StringSuggestionId, HistoryTypeId, Comment, UserId, CreationDate)
Select Id, {=DismissedByOtherStringHistory}, Concat('Suggestion ', @suggestionId, ' approved'), @userId, @now
From   StringSuggestions
Where  StringId = @stringId
And    StateId In ({=Created}, {=ApprovedByTrustedUser});

Update StringSuggestions
Set    StateId = {=DismissedByOtherStringState}
Where  StringId = @stringId
And    StateId In ({=Created}, {=ApprovedByTrustedUser});";
                    }

                    await db.ExecuteAsync(sql, new
                    {
                        suggestionId,
                        historyType,
                        userId,
                        newState,
                        StringSuggestionState.Created,
                        StringSuggestionState.ApprovedByTrustedUser,
                        now = DateTime.UtcNow,
                        DismissedByOtherStringHistory = StringSuggestionHistoryType.DismissedByOtherString,
                        DismissedByOtherStringState = StringSuggestionState.DismissedByOtherString,
                    });

                    await RefreshCacheAsync(suggestionData.stringId);

                    var notificationType = newState.GetNotificationType();

                    if (notificationType.HasValue && userId != suggestionData.ownerId)
                    {
                        bool useHttps = _configuration.GetValue<bool>("USE_HTTPS");
                        await _userService.SendNotification(suggestionData.ownerId, notificationType.Value, useHttps, host);
                    }

                    return true;
                }

                return false;
            }
        }

        public async Task UpdateStringsPushed()
        {
            using (var db = _dbService.GetConnection())
            {
                var rows = await db.ExecuteAsync(@"Update Strings Set NeedsPush = 0 Where NeedsPush = 1");
                if (rows > 0)
                {
                    ExpireCache();
                }
            }
        }

        private async Task FixNormalizedKeysAsync(IDbConnection db)
        {
            var currentNormalized = await db.QueryAsync<string>(@"Select NormalizedKey From Strings Group By NormalizedKey");
            await db.ExecuteAsync(@"Update Strings Set NormalizedKey = @newNK Where NormalizedKey = @oldNK",
                currentNormalized.Select(oldNK => new { oldNK, newNk = oldNK.ToNormalizedKey() })
                                    .Where(e => e.oldNK != e.newNk));
        }

        public async Task UpdateTranslationsFromSODB(bool overrideExisting)
        {
            int? lcid = _configuration.GetValue<int?>("LCID");
            if (!lcid.HasValue)
            {
                throw new Exception("Need an LCID defined");
            }

            SODBString[] soStrings;
            using (MiniProfiler.Current.Step("Fetching data from SO"))
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync("https://stackoverflow.com/api/translation-strings");
                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(stream))
                {
                    soStrings = Jil.JSON.Deserialize<SODBString[]>(reader);
                }
            }

            using (MiniProfiler.Current.Step("Filtering out strings for other LCIDs"))
            {
                // filter the strings to the locale this site is using
                soStrings = soStrings.Where(s => s.LCID == lcid.Value).ToArray();
            }

            using (var db = _dbService.GetConnection())
            {
                using (MiniProfiler.Current.Step("Bulk inserting the data from the service"))
                {
                    await ResetDumpTable(db);
                    var table = new DataTable();
                    table.Columns.Add("Hash", typeof(string));
                    table.Columns.Add("NormalizedHash", typeof(string));
                    table.Columns.Add("Translation", typeof(string));

                    foreach (var s in soStrings)
                    {
                        table.Rows.Add(s.Hash, s.NormalizedHash, s.EffectiveTranslation);
                    }

                    var copyDb = (SqlConnection)db.WrappedConnection;
                    using (var copy = new SqlBulkCopy(copyDb))
                    {
                        copy.DestinationTableName = "dbo.SODumpTable";
                        foreach (DataColumn c in table.Columns)
                        {
                            copy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(c.ColumnName, c.ColumnName));
                        }

                        copyDb.Open();
                        copy.WriteToServer(table);
                    }
                }

                using (MiniProfiler.Current.Step("Updating the translations!"))
                {
                    await db.ExecuteAsync($@"
Insert Into StringHistory
            (StringId, HistoryTypeId, CreationDate)
Select s.Id, {{=TranslationUpdatedFromDump}}, @now
From   Strings s
Join   SODumpTable dump On dump.NormalizedHash = s.NormalizedKey
Where  s.Translation Is Null;

Update s
Set    s.Translation = dump.Translation,
       s.IsUrgent = 0
From   Strings s
Join   SODumpTable dump On dump.NormalizedHash = s.NormalizedKey
{(overrideExisting ? string.Empty : "Where  s.Translation Is Null;")}", new { now = DateTime.UtcNow, StringHistoryType.TranslationUpdatedFromDump });
                }
            }

            await RefreshCacheAsync();
        }

        public async Task<bool> ManageUrgencyAsync(int stringId, bool isUrgent, int userId)
        {
            var str = await GetStringByIdAsync(stringId);
            if (str == null)
            {
                return false;
            }

            if (str.IsUrgent == isUrgent)
            {
                return true;
            }

            using (var db = _dbService.GetConnection())
            {
                await db.ExecuteAsync(@"
Insert Into StringHistory
            (StringId, UserId, HistoryTypeId, CreationDate)
Values      (@stringId, @userId, @historyType, @now);

Update Strings
Set    IsUrgent = @isUrgent
Where  Id = @stringId", new
                {
                    stringId,
                    isUrgent,
                    userId,
                    historyType = isUrgent ? StringHistoryType.MadeUrgent : StringHistoryType.MadeNotUrgent,
                    now = DateTime.UtcNow
                });
            }

            await RefreshCacheAsync(stringId);
            return true;
        }

        public async Task<bool> ManageIgnoreAsync(int stringId, bool isIgnored, int userId, UserType userType)
        {
            var str = await GetStringByIdAsync(stringId);
            if (str == null)
            {
                return false;
            }

            if (userType < UserType.TrustedUser)
            {
                return false;
            }

            if (str.IsIgnored == isIgnored)
            {
                return true;
            }

            using (var db = _dbService.GetConnection())
            {
                await db.ExecuteAsync(@"
Insert Into StringHistory
            (StringId, UserId, HistoryTypeId, CreationDate)
Select Id, @userId, @historyType, @now
From   Strings
Where  FamilyKey = @familyKey;

Update Strings
Set    IsIgnored = @isIgnored
Where  FamilyKey = @familyKey", new
                {
                    isIgnored,
                    userId,
                    familyKey = str.FamilyKey,
                    historyType = isIgnored ? StringHistoryType.Ignored : StringHistoryType.UnIgnored,
                    now = DateTime.UtcNow
                });
            }

            await RefreshCacheAsync(str.FamilyKey);
            return true;
        }

        public async Task<bool> DeleteSuggestionAsync(int suggestionId, int userId)
        {
            using (var db = _dbService.GetConnection())
            {
                var idString = await db.QuerySingleOrDefaultAsync<int>(@"
Declare @idString int = 0;

Update StringSuggestions
Set    StateId = {=DeletedByOwner},
       LastStateUpdatedById = @userId,
       LastStateUpdatedDate = @now,
       @idString = StringId
Where  Id = @suggestionId
And    CreatedById = @userId;

Insert Into StringSuggestionHistory
            (StringSuggestionId, HistoryTypeId, UserId, CreationDate)
Select Id, {=DeletedByOwner}, CreatedById, LastStateUpdatedDate
From   StringSuggestions
Where  Id = @suggestionId
And    CreatedById = @userId
And    StateId = {=DeletedByOwner};

Select @idString;", new
                {
                    suggestionId,
                    StringSuggestionState.DeletedByOwner,
                    userId,
                    now = DateTime.UtcNow
                });

                // If the Id returned is zero, then no data was updated, because there is no suggestion
                // or the user is not the one who created it.
                if (idString > 0)
                {
                    await RefreshCacheAsync(idString);
                    return true;
                }

                return false;
            }
        }

        public async Task<ImmutableArray<SOStringSuggestion>> GetSuggestionsByUser(int userId, StringSuggestionState? state)
        {
            string hasFilters = string.Empty;
            string sql = $@"
Declare @Ids Table
(
  Id int
);

Insert Into @Ids
Select Top 100 Id
From   StringSuggestions sug
Where  sug.CreatedById = @userId
{(state.HasValue ? "And sug.StateId = @state" : string.Empty)}
Order By sug.CreationDate Desc;

Select    sug.Id, sug.Suggestion, sug.StringId, sug.StateId State, sug.CreationDate, sug.LastStateUpdatedDate, sug.LastStateUpdatedById,
          u.DisplayName LastStateUpdatedByName, s.OriginalString
From      StringSuggestions sug
Join      @Ids ids On ids.Id = sug.Id
Left Join Users u On u.Id = sug.LastStateUpdatedById
Join      Strings s On s.Id = sug.StringId;

Select h.Id, h.StringSuggestionId, h.HistoryTypeId HistoryType, h.Comment, h.UserId, h.CreationDate,
       u.DisplayName UserName
From   StringSuggestionHistory h
Join   @Ids ids On ids.Id = h.StringSuggestionId
Join   Users u On u.Id = h.UserId;
";

            using (var db = _dbService.GetConnection())
            using (var reader = await db.QueryMultipleAsync(sql, new { userId, state }))
            {
                var suggestions = (await reader.ReadAsync<SOStringSuggestion>()).AsList();
                var histories = (await reader.ReadAsync<SOStringSuggestionHistory>()).AsList();
                var historiesById = histories.ToLookup(h => h.StringSuggestionId);

                foreach (var suggestion in suggestions)
                {
                    suggestion.Histories = historiesById[suggestion.Id].ToArray();
                }

                return suggestions.ToImmutableArray();
            }
        }

        public async Task<bool> ReplaceSuggestionAsync(int suggestionId, string suggestion, int userId)
        {
            using (var db = _dbService.GetConnection())
            {
                var idString = await db.QuerySingleOrDefaultAsync<int>(@"
Declare @idString int = 0;
Declare @OriginalString nvarchar(max);

Select @OriginalString = 'Original Suggestion: ' + Suggestion,
@idString = StringId
From StringSuggestions
Where id = @suggestionId
And   CreatedById = @userId;

Update StringSuggestions
Set Suggestion = @suggestion
Where Id = @suggestionId
And   CreatedById = @userId;

Insert into StringSuggestionHistory
            (StringSuggestionId, HistoryTypeId, Comment, UserId, CreationDate)
Select Id, {=ReplacedByOwner}, @OriginalString, CreatedById, @now
from StringSuggestions
Where Id = @suggestionId
And   CreatedById = @userId;

Select @idString", new
                {
                    suggestion,
                    suggestionId,
                    StringSuggestionHistoryType.ReplacedByOwner,
                    userId,
                    now = DateTime.UtcNow
                });

                // If the Id returned is zero, then no data was updated, because there is no suggestion
                // or the user is not the one who created it.
                if (idString > 0)
                {
                    await RefreshCacheAsync(idString);
                    return true;
                }

                return false;
            }
        }

        public async Task<int> GetStringIdBySuggestionId(int suggestionId)
        {
            using (var db = _dbService.GetConnection())
            {
                return await db.QuerySingleAsync<int>(
                    "select StringId from StringSuggestions where Id=@suggestionId",
                    new { suggestionId });
            }
        }

        private static Task ResetDumpTable(DbConnection db)
        {
            return db.ExecuteAsync(@"
Drop Table If Exists dbo.SODumpTable;
Create Table dbo.SODumpTable
(
Id                  Int Not Null Identity (1, 1),
Hash                VarChar(255) Not Null,
NormalizedHash      VarChar(255) Not Null,
Translation         NVarChar(Max) Null,

Constraint PK_SODumpTable Primary Key Clustered (Id),
Constraint IX_SODumpTable_Hash Unique (Hash),
Index      IX_SODumpTable_NormalizedHash NonClustered (NormalizedHash)
)");
        }

        private static Task CreateTemporaryTable(DbConnection db)
        {
            return db.ExecuteAsync(@"
Drop Table If Exists dbo.ImportTable;
Create Table dbo.ImportTable
(
NormalizedKey  VarChar(255) Not Null,
[Key]          VarChar(255) Not Null,
Variant        VarChar(255) Null,
OriginalString NVarChar(Max) Not Null,
Translation    NVarChar(Max) Null,

Constraint PK_ImportTable Primary Key Clustered (NormalizedKey Asc)
)");
        }

        private void ExpireCache()
        {
            Strings = ImmutableArray<SOString>.Empty;
        }

        private Task RefreshCacheAsync(string familyKey)
        {
            return RefreshCacheAsync(null, familyKey);
        }

        private async Task RefreshCacheAsync(int? stringId = null, string familyKey = null)
        {
            string sql = $@"
Select   Id, [Key], FamilyKey, OriginalString, Translation, NeedsPush, IsUrgent, IsIgnored, Variant, CreationDate
From     Strings
Where    IsNull(DeletionDate, @deletionDateLimit) >= @deletionDateLimit
{(stringId.HasValue ? "And Id = @stringId" : string.Empty)}
{(familyKey.HasValue() ? "And FamilyKey = @familyKey" : string.Empty)}
Order By IsUrgent Desc, OriginalString Asc;

Select    ss.Id, ss.StringId, ss.Suggestion, ss.StateId State,
          ss.CreatedById, u.DisplayName CreatedByName,
          ss.LastStateUpdatedById, uu.DisplayName LastStateUpdatedByName,
          ss.CreationDate
From      StringSuggestions ss
Join      Strings s On s.Id = ss.StringId And IsNull(s.DeletionDate, @deletionDateLimit) >= @deletionDateLimit
Join      Users u On ss.CreatedById = u.Id
Left Join Users uu On uu.Id = ss.LastStateUpdatedById
Where     ss.StateId In ({{=Created}}, {{=ApprovedByTrustedUser}})
{(stringId.HasValue ? "And s.Id = @stringId" : string.Empty)}
{(familyKey.HasValue() ? "And s.FamilyKey = @familyKey" : string.Empty)}";

            using (MiniProfiler.Current.Step("Refreshing the strings cache"))
            using (var db = _dbService.GetConnection())
            using (var reader = await db.QueryMultipleAsync(sql, new
            {
                StringSuggestionState.Created,
                StringSuggestionState.ApprovedByTrustedUser,
                stringId,
                familyKey,
                deletionDateLimit = DateTime.UtcNow.AddDays(-5)
            }))
            {
                var strings = (await reader.ReadAsync<SOString>()).AsList();
                var suggestions = (await reader.ReadAsync<SOStringSuggestion>()).AsList();
                Dictionary<int, SOString> stringsById;

                using (MiniProfiler.Current.Step("Attaching the suggestions to the strings"))
                {
                    stringsById = strings.ToDictionary(s => s.Id);
                    foreach (var g in suggestions.GroupBy(g => g.StringId))
                    {
                        if (stringsById.TryGetValue(g.Key, out var str))
                        {
                            str.Suggestions = g.ToArray();
                        }
                    }
                }

                if (stringId.HasValue)
                {
                    StringsById[stringId.Value] = stringsById[stringId.Value];
                    Strings = StringsById.Values.OrderByDescending(s => s.IsUrgent).ThenBy(s => s.OriginalString).ToImmutableArray();
                }
                else if (familyKey.HasValue())
                {
                    foreach (var str in strings)
                    {
                        StringsById[str.Id] = str;
                    }

                    Strings = StringsById.Values.OrderByDescending(s => s.IsUrgent).ThenBy(s => s.OriginalString).ToImmutableArray();
                }
                else
                {
                    StringsById = stringsById;
                    Strings = strings.ToImmutableArray();
                }
            }
        }
    }
}