using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using StackExchange.Profiling;
using Traducir.Core.Helpers;
using Traducir.Core.Models.Enums;
using Traducir.Core.Models.Services;

namespace Traducir.Core.Services
{
    public interface ISOStringService
    {
        Task StoreNewStrings(TransifexString[] strings);
    }
    public class SOStringService : ISOStringService
    {
        private IDbService _dbService { get; set; }
        public SOStringService(IDbService dbService)
        {
            _dbService = dbService;
        }

        public async Task StoreNewStrings(TransifexString[] strings)
        {
            using(var db = _dbService.GetConnection())
            {
                var currentKeys = (await db.QueryAsync<(int id, string key)>(@"
Select Id, NormalizedKey
From   Strings
Where  DeletionDate Is Null"))
                    .ToDictionary(e => e.key, e => e.id, StringComparer.InvariantCultureIgnoreCase);
                var newKeys = strings.ToDictionary(s => s.NormalizedKey, StringComparer.InvariantCultureIgnoreCase);

                // delete old ones
                var idsToDelete = currentKeys.Where(e => !newKeys.ContainsKey(e.Key)).Select(e => e.Value);
                if (idsToDelete.Any())
                {
                    using(MiniProfiler.Current.Step("Deleting strings"))
                    {
                        await db.ExecuteAsync(@"
Update Strings
Set    DeletionDate = @now
Where  Id In @idsToDelete", new { now = DateTime.UtcNow, idsToDelete });

                        await db.ExecuteAsync(@"
Insert Into StringHistory
            (StringId, HistoryTypeId, CreationDate)
Select      Id, {=Deleted}, @now
From        Strings
Where       Id In @idsToDelete", new { now = DateTime.UtcNow, idsToDelete, StringHistoryType.Deleted });
                    }
                }

                // add new ones
                var stringsToAdd = newKeys.Where(e => !currentKeys.ContainsKey(e.Key)).Select(e => e.Value).ToList();
                if (stringsToAdd.Any())
                {
                    var existing = new List<(string key, int id)>();
                    foreach (var stringsBatch in stringsToAdd.Batch(2000))
                    {
                        existing.AddRange(await db.QueryAsync<(string key, int id)>(@"
Select NormalizedKey, Id
From   Strings
Where  NormalizedKey In @keys", new { keys = stringsBatch.Select(s => s.NormalizedKey)}));
                    }
                    var existingKeys = existing.Select(e => e.key).ToHashSet(StringComparer.InvariantCultureIgnoreCase);

                    if (existingKeys.Any())
                    {
                        using(MiniProfiler.Current.Step("Undeleting existing strings"))
                        {
                            await db.ExecuteAsync(@"
Update Strings
Set    DeletionDate = Null
Where  Id In @ids;

Insert Into StringHistory
            (StringId, HistoryTypeId, CreationDate)
Select      Id, {=Created}, @now
From        Strings
Where       Id In @ids", new
                            {
                                ids = existing.Select(e => e.id),
                                    now = DateTime.UtcNow,
                                    StringHistoryType.Created
                            });
                        }
                    }

                    var stringsToInsert = stringsToAdd.Where(st => !existingKeys.Contains(st.NormalizedKey)).ToList();
                    if (stringsToInsert.Any())
                    {
                        using(MiniProfiler.Current.Step("Adding new strings to the database - SqlBulkCopy"))
                        {
                            // add the really new ones
                            var table = new DataTable();
                            table.Columns.Add("Key", typeof(string));
                            table.Columns.Add("NormalizedKey", typeof(string));
                            table.Columns.Add("OriginalString", typeof(string));
                            table.Columns.Add("Translation", typeof(string));
                            table.Columns.Add("CreationDate", typeof(DateTime));

                            foreach (var s in stringsToInsert)
                            {
                                table.Rows.Add(s.Key, s.NormalizedKey, s.Source, s.Translation, DateTime.UtcNow);
                            }

                            using(var copyDb = _dbService.GetRawConnection())
                            using(var copy = new SqlBulkCopy(copyDb))
                            {
                                copy.DestinationTableName = "Strings";
                                foreach (DataColumn c in table.Columns)
                                {
                                    copy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(c.ColumnName, c.ColumnName));
                                }

                                copyDb.Open();
                                copy.WriteToServer(table);
                            }
                        }
                        using(MiniProfiler.Current.Step("Adding new strings to the database - Add history"))
                        {
                            // add the history to the string
                            foreach (var stringsBatch in stringsToInsert.Batch(2000))
                            {
                                await db.ExecuteAsync(@"
Insert Into StringHistory
            (StringId, HistoryTypeId, CreationDate)
Select      Id, {=Created}, @now
From        Strings
Where       NormalizedKey In @keys", new
                                {
                                    keys = stringsBatch.Select(s => s.NormalizedKey),
                                        now = DateTime.UtcNow,
                                        StringHistoryType.Created
                                });
                            }
                        }
                    }
                }
            }
        }
    }
}