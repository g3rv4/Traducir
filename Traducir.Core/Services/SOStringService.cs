using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper;
using Dapper;
using StackExchange.Profiling;
using Traducir.Core.Models;
using Traducir.Core.Models.Enums;
using Traducir.Core.Models.Services;

namespace Traducir.Core.Services
{
    public interface ISOStringService
    {
        Task StoreNewStringsAsync(ImmutableArray<TransifexString> strings);
        Task<SOString> GetStringByIdAsync(int stringId);
        Task<ImmutableArray<SOString>> GetStringsAsync();
        Task<ImmutableArray<SOString>> GetStringsAsync(Func<SOString, bool> predicate);
        Task<bool> CreateSuggestionAsync(int stringId, string suggestion, int userId, UserType userType, bool approve);
        Task<bool> ReviewSuggestionAsync(int suggestionId, bool approve, int userId, UserType userType);
        Task UpdateStringsPushed();
        Task PullSODump(string dumpUrl);
        Task UpdateTranslationsFromSODump();
        Task<bool> ManageUrgencyAsync(int stringId, bool isUrgent, int userId);
    }
    public class SOStringService : ISOStringService
    {
        private IDbService _dbService { get; set; }
        private ImmutableArray<SOString> _strings { get; set; }
        private Dictionary<int, SOString> _stringsById { get; set; }
        public SOStringService(IDbService dbService)
        {
            _dbService = dbService;
        }

        private Task CreateTemporaryTable(DbConnection db)
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

                    var copyDb = (SqlConnection)db.InnerConnection;
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

Insert Into Strings ([Key], NormalizedKey, Variant, OriginalString, Translation, CreationDate)
Select [Key], feed.NormalizedKey, feed.Variant, feed.OriginalString, feed.Translation, @now
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

        private void ExpireCache()
        {
            _strings = ImmutableArray<SOString>.Empty;
        }

        private async Task RefreshCacheAsync(int? stringId = null)
        {
            const string sql = @"
Select   Id, [Key], OriginalString, Translation, NeedsPush, IsUrgent, Variant, CreationDate
From     Strings
Where    DeletionDate Is Null
-- And   Id = @stringId
Order By IsUrgent Desc, OriginalString Asc;

Select    ss.Id, ss.StringId, ss.Suggestion, ss.StateId State,
          ss.CreatedById, u.DisplayName CreatedByName,
          ss.LastStateUpdatedById, uu.DisplayName LastStateUpdatedByName,
          ss.CreationDate
From      StringSuggestions ss
Join      Strings s On s.Id = ss.StringId And s.DeletionDate Is Null
Join      Users u On ss.CreatedById = u.Id
Left Join Users uu On uu.Id = ss.LastStateUpdatedById
Where     ss.StateId In ({=Created}, {=ApprovedByTrustedUser})
-- And s.Id = @stringId";
            var finalSql = stringId.HasValue ? sql.Replace("--", "") : sql;

            using (MiniProfiler.Current.Step("Refreshing the strings cache"))
            using (var db = _dbService.GetConnection())
            using (var reader = await db.QueryMultipleAsync(finalSql, new
            {
                StringSuggestionState.Created,
                StringSuggestionState.ApprovedByTrustedUser,
                stringId
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
                    _stringsById[stringId.Value] = stringsById[stringId.Value];
                    _strings = _stringsById.Values.OrderByDescending(s => s.IsUrgent).ThenBy(s => s.OriginalString).ToImmutableArray();
                }
                else
                {
                    _stringsById = stringsById;
                    _strings = strings.ToImmutableArray();
                }
            }
        }

        public async Task<ImmutableArray<SOString>> GetStringsAsync(Func<SOString, bool> predicate)
        {
            if (_strings == null || _strings.Length == 0)
            {
                await RefreshCacheAsync();
            }

            ImmutableArray<SOString> result;
            using (MiniProfiler.Current.Step("Filtering the strings"))
            {
                result = _strings.Where(predicate).ToImmutableArray();
            }
            return result;
        }

        public async Task<SOString> GetStringByIdAsync(int stringId)
        {
            if (_strings == null || _strings.Length == 0)
            {
                await RefreshCacheAsync();
            }
            if (_stringsById.TryGetValue(stringId, out var res))
            {
                return res;
            }
            return null;
        }

        public async Task<bool> CreateSuggestionAsync(int stringId, string suggestion, int userId, UserType userType, bool approve)
        {
            using (var db = _dbService.GetConnection())
            {

                int? suggestionId;
                try
                {
                    var initialState = userType >= UserType.TrustedUser ? StringSuggestionState.ApprovedByTrustedUser : StringSuggestionState.Created;
                    suggestionId = await db.QuerySingleOrDefaultAsync<int?>(@"
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
                        await ReviewSuggestionAsync(suggestionId.Value, true, userId, userType);
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

        public async Task<bool> ReviewSuggestionAsync(int suggestionId, bool approve, int userId, UserType userType)
        {
            if (userType != UserType.TrustedUser && userType != UserType.Reviewer)
            {
                return false;
            }
            using (var db = _dbService.GetConnection())
            {
                // is this suggestion eligible for review?
                int? stringId = await db.QuerySingleOrDefaultAsync<int?>($@"
Select StringId
From   StringSuggestions
Where  Id = @suggestionId
And    StateId In @validStates", new
                {
                    userId,
                    suggestionId,
                    validStates = userType == UserType.Reviewer ? new[] { StringSuggestionState.Created, StringSuggestionState.ApprovedByTrustedUser } : new[] { StringSuggestionState.Created }
                });

                if (stringId.HasValue)
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

                    await RefreshCacheAsync(stringId.Value);
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

        private Task ResetDumpTable(DbConnection db)
        {
            return db.ExecuteAsync(@"
Drop Table If Exists dbo.SODumpTable;
Create Table dbo.SODumpTable
(
Id                  Int Not Null,
LocaleId            SmallInt Not Null,
Hash                VarChar(255) Not Null,
NormalizedHash      VarChar(255) Not Null,
Translation         NVarChar(Max) Null,
CreationDate        DateTime Not Null,
ModifiedDate        DateTime Null,
LastSeenDate        DateTime Not Null,
TranslationOverride NVarChar(Max) Null,

Constraint PK_SODumpTable Primary Key Clustered (Id),
Constraint IX_SODumpTable_Hash Unique (Hash),
Index      IX_SODumpTable_NormalizedHash NonClustered (NormalizedHash)
)");
        }

        public async Task PullSODump(string dumpUrl)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(dumpUrl);
                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(stream))
                using (var csv = new CsvReader(reader))
                using (var db = _dbService.GetConnection())
                {
                    await ResetDumpTable(db);
                    var table = new DataTable();
                    table.Columns.Add("Id", typeof(int));
                    table.Columns.Add("LocaleId", typeof(int));
                    table.Columns.Add("Hash", typeof(string));
                    table.Columns.Add("NormalizedHash", typeof(string));
                    table.Columns.Add("CreationDate", typeof(DateTime));
                    table.Columns.Add("ModifiedDate", typeof(DateTime));
                    table.Columns.Add("LastSeenDate", typeof(DateTime));
                    table.Columns.Add("Translation", typeof(string));
                    table.Columns.Add("TranslationOverride", typeof(string));

                    var str = new SODumpString();

                    foreach (var s in csv.EnumerateRecords(str))
                    {
                        table.Rows.Add(s.Id, s.LocaleId, s.Hash, s.NormalizedHash, s.CreationDate,
                            s.ModifiedDate, s.LastSeenDate,
                            s.Translation == "NULL" ? null : s.Translation,
                            s.TranslationOverride == "NULL" ? null : s.TranslationOverride);
                    }

                    var copyDb = (SqlConnection)db.InnerConnection;
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
            }
        }

        public async Task UpdateTranslationsFromSODump()
        {
            using (var db = _dbService.GetConnection())
            {
                // update the ones in the db that are not in Transifex
                await db.ExecuteAsync(@"
Insert Into StringHistory
            (StringId, HistoryTypeId, CreationDate)
Select s.Id, {=TranslationUpdatedFromDump}, @now
From   Strings s
Join   SODumpTable dump On dump.Hash = s.[Key]
Where  s.Translation Is Null;

Update s
Set    s.Translation = dump.Translation,
       s.IsUrgent = 0
From   Strings s
Join   SODumpTable dump On dump.Hash = s.[Key]
Where  s.Translation Is Null;", new { now = DateTime.UtcNow, StringHistoryType.TranslationUpdatedFromDump });

                // update the ones in the db that have a translation with a different variant order
                await db.ExecuteAsync(@"
Insert Into StringHistory
            (StringId, HistoryTypeId, CreationDate)
Select s.Id, {=TranslationUpdatedFromDump}, @now
From   Strings s
Join   SODumpTable dump On dump.NormalizedHash = s.NormalizedKey
Where  s.Translation Is Null;

Update s
Set    s.Translation = dump.Translation,
       s.IsUrgent = 0
From   Strings s
Join   SODumpTable dump On dump.NormalizedHash = s.NormalizedKey
Where  s.Translation Is Null;", new { now = DateTime.UtcNow, StringHistoryType.TranslationUpdatedFromDump });

            }
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

        public async Task<ImmutableArray<SOString>> GetStringsAsync()
        {
            if (_strings == null || _strings.Length == 0)
            {
                await RefreshCacheAsync();
            }
            return _strings;
        }
    }
}