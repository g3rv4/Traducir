using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Traducir.Core.Models;
using Traducir.Core.Models.Enums;

namespace Traducir.Core.Services
{
    public interface INotificationService
    {
        Task SendStateNotifications(string host);
    }

    public class NotificationService : INotificationService
    {
        private readonly IDbService _dbService;
        private readonly IUserService _userService;
        private IConfiguration _configuration;

        public NotificationService(IDbService dbService, IUserService userService, IConfiguration configuration)
        {
            _dbService = dbService;
            _userService = userService;
            _configuration = configuration;
        }

        public async Task SendStateNotifications(string host)
        {
            const string sql = @"
Select Count(1)
From   Strings
Where  IsUrgent = 1
And    DeletionDate Is Null;

Select   ss.StateId, Count(1)
From     StringSuggestions ss
Join     Strings s on s.Id = ss.StringId
Where    s.DeletionDate Is Null
Group By StateId";

            using (var db = _dbService.GetConnection())
            {
                using (var reader = await db.QueryMultipleAsync(sql))
                {
                    var urgentStrings = await reader.ReadFirstAsync<int>();
                    var suggestionCounts = (await reader.ReadAsync<(StringSuggestionState state, int count)>()).ToDictionary(e => e.state, e => e.count);
                    NotificationType type;

                    if (urgentStrings > 0)
                    {
                        type = NotificationType.UrgentStrings;
                        await _userService.SendBatchNotifications(type, host, urgentStrings);
                    }

                    if (suggestionCounts.TryGetValue(StringSuggestionState.Created, out var count))
                    {
                        type = NotificationType.SuggestionsAwaitingApproval;
                        await _userService.SendBatchNotifications(type, host, count);
                    }

                    if (suggestionCounts.TryGetValue(StringSuggestionState.ApprovedByTrustedUser, out count))
                    {
                        type = NotificationType.SuggestionsAwaitingReview;
                        await _userService.SendBatchNotifications(type, host, count);
                    }
                }
            }
        }
    }
}