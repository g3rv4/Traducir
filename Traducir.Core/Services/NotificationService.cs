using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Traducir.Core.Models;
using Traducir.Core.Models.Enums;

namespace Traducir.Core.Services
{
    public interface INotificationService
    {
        Task GenerateNotifications();

        Task SendNotifications();
    }

    public class NotificationService : INotificationService
    {
        private readonly IDbService _dbService;

        public NotificationService(IDbService dbService)
        {
            _dbService = dbService;
        }

        public async Task GenerateNotifications()
        {
            const string sql = @"
Select Count(1)
From   Strings
Where  IsUrgent = 1
And    DeletionDate Is Null;

Select   StateId, Count(1)
From     StringSuggestions
Group By StateId";

            using (var db = _dbService.GetConnection())
            {
                Task sendGenericNotification(string notificationField, NotificationType notificationType, int count)
                {
                    var data = Jil.JSON.Serialize(count);
                    return db.ExecuteAsync($@"
Insert Into Notifications
            (UserId, Data, NotificationTypeId, CreationDate)
Select Id, @data, {{=notificationType}}, @now
From   Users u
Where  {notificationField} = 1" + GetNotExistsPart(), new
                    {
                        now = DateTime.UtcNow,
                        notificationType,
                        data,
                        NotificationInterval.Days,
                        NotificationInterval.Hours,
                        NotificationInterval.Minutes
                    });
                }

                using (var reader = await db.QueryMultipleAsync(sql))
                {
                    var urgentStrings = (await reader.ReadAsync<int>()).First();
                    var suggestionCounts = (await reader.ReadAsync<(StringSuggestionState state, int count)>()).ToDictionary(e => e.state, e => e.count);

                    if (urgentStrings > 0)
                    {
                        await sendGenericNotification(nameof(NotificationSettings.NotifyUrgentStrings), NotificationType.UrgentStrings, urgentStrings);
                    }

                    if (suggestionCounts.TryGetValue(StringSuggestionState.Created, out var count))
                    {
                        await sendGenericNotification(nameof(NotificationSettings.NotifySuggestionsAwaitingApproval), NotificationType.SuggestionsAwaitingApproval, count);
                    }

                    if (suggestionCounts.TryGetValue(StringSuggestionState.ApprovedByTrustedUser, out count))
                    {
                        await sendGenericNotification(nameof(NotificationSettings.NotifySuggestionsAwaitingReview), NotificationType.SuggestionsAwaitingReview, count);
                    }
                }
            }
        }

        public Task SendNotifications()
        {
            throw new System.NotImplementedException();
        }

        private static string GetNotExistsPart()
        {
            return @"
And    Not Exists (Select 1
                   From   Notifications n
                   Where  n.UserId = u.Id
                   And    n.NotificationTypeId = {=notificationType}
                   And    n.CreationDate > Case
                               When u.NotificationsIntervalId = {=Minutes} Then DateAdd(minute, -u.NotificationsIntervalValue, @now)
                               When u.NotificationsIntervalId = {=Hours} Then DateAdd(hour, -u.NotificationsIntervalValue, @now)
                               Else DateAdd(day, -u.NotificationsIntervalValue, @now)
                          End)";
        }
    }
}