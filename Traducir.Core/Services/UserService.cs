using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Traducir.Core.Helpers;
using Traducir.Core.Models;
using Traducir.Core.Models.Enums;
using WebPush;

namespace Traducir.Core.Services
{
    public interface IUserService
    {
        Task UpsertUserAsync(User user);

        Task<User> GetUserAsync(int userId);

        Task<List<User>> GetUsersAsync();

        Task<NotificationSettings> GetNotificationSettings(int userId);

        Task<bool> ChangeUserTypeAsync(int userId, UserType userType, int editorId);

        Task<bool> UpdateNotificationSettings(int userId, NotificationSettings newSettings);

        Task<bool> AddNotificationBrowser(int userId, WebPushSubscription subscription);

        Task<bool> SendNotification(int userId, Models.Services.PushNotificationMessage message);
    }

    public class UserService : IUserService
    {
        private readonly IDbService _dbService;
        private readonly VapidDetails _vapidDetails;
        private readonly WebPushClient _webPushClient;

        public UserService(IDbService dbService, IConfiguration configuration)
        {
            _dbService = dbService;

            var subject = configuration.GetValue<string>("VAPID_SUBJECT");
            var publicKey = configuration.GetValue<string>("VAPID_PUBLIC");
            var privateKey = configuration.GetValue<string>("VAPID_PRIVATE");

            _vapidDetails = new VapidDetails(subject, publicKey, privateKey);
            _webPushClient = new WebPushClient();
        }

        public async Task UpsertUserAsync(User user)
        {
            using (var db = _dbService.GetConnection())
            {
                await db.ExecuteAsync(@"
Declare @wasReviewer Bit, @wasTrusted Bit;
Select @wasReviewer = IsReviewer, @wasTrusted = IsTrusted From Users Where Id = @Id;

If @wasReviewer Is Null
  -- It's an insert!
  Insert Into Users (Id, DisplayName, IsModerator, IsTrusted, IsReviewer, IsBanned, CreationDate, LastSeenDate)
  Values            (@Id, @DisplayName, @IsModerator, @IsModerator, @IsModerator, 0, @CreationDate, @LastSeenDate)
Else
  -- It's an update
  Update Users
  Set    DisplayName = @DisplayName,
         IsModerator = @IsModerator,
         IsReviewer = Case When @wasReviewer = 1 Then 1 Else @IsModerator End, -- if the user was a reviewer, keep them
                                                                               -- if they're now a mod, make them a reviewer
         IsTrusted = Case When @wasTrusted = 1 Then 1 Else @IsModerator End,
         LastSeenDate = @LastSeenDate
Where    Id = @Id", user);
            }
        }

        public async Task<User> GetUserAsync(int userId)
        {
            using (var db = _dbService.GetConnection())
            {
                return await db.QueryFirstOrDefaultAsync<User>(@"
Select Id, DisplayName, IsModerator, IsBanned, IsTrusted, IsReviewer,
       CreationDate, LastSeenDate
From   Users
Where  Id = @userId", new
                {
                    userId
                });
            }
        }

        public async Task<List<User>> GetUsersAsync()
        {
            using (var db = _dbService.GetConnection())
            {
                return (await db.QueryAsync<User>(@"
Select Id, DisplayName, IsModerator, IsBanned, IsTrusted, IsReviewer,
       CreationDate, LastSeenDate
From Users")).AsList();
            }
        }

        public async Task<bool> ChangeUserTypeAsync(int userId, UserType userType, int editorId)
        {
            using (var db = _dbService.GetConnection())
            {
                var rows = await db.ExecuteAsync(@"
Insert Into UserHistory
            (UserId, UpdatedById, CreationDate, HistoryTypeId)
Select Id, @editorId, @now,
       Case When @userType = {=TrustedUser} Then {=HistoryMadeTrustedUser}
            When @userType = {=Banned} Then {=HistoryBanned}
            When @userType = {=User} And IsBanned = 1 Then {=HistoryBanLifted}
            When @userType = {=User} And IsTrusted = 1 Then {=HistoryDemotedToRegularUser} End
From   Users
Where  Id = @userId
And    IsModerator = 0
And    IsReviewer = 0;

Update Users
Set    IsTrusted = Case When @userType = {=TrustedUser} Then 1 Else 0 End,
       IsBanned = Case When @userType = {=Banned} Then 1 Else 0 End
Where  Id = @userId
And    IsModerator = 0
And    IsReviewer = 0;", new
                {
                    userId,
                    editorId,
                    userType,
                    UserType.TrustedUser,
                    UserType.Banned,
                    UserType.User,
                    now = DateTime.UtcNow,
                    HistoryMadeTrustedUser = UserHistoryType.MadeTrustedUser,
                    HistoryBanned = UserHistoryType.Banned,
                    HistoryBanLifted = UserHistoryType.BanLifted,
                    HistoryDemotedToRegularUser = UserHistoryType.DemotedToRegularUser
                });
                return rows > 0;
            }
        }

        public async Task<NotificationSettings> GetNotificationSettings(int userId)
        {
            using (var db = _dbService.GetConnection())
            {
                return await db.QueryFirstOrDefaultAsync<NotificationSettings>(@"
Select NotifyUrgentStrings, NotifySuggestionsAwaitingApproval, NotifySuggestionsAwaitingReview,
       NotifyStringsPushedToTransifex, NotifySuggestionsApproved, NotifySuggestionsRejected,
       NotifySuggestionsReviewed, NotifySuggestionsOverriden, NotificationsIntervalId NotificationsInterval,
       NotificationsIntervalValue
From   Users
Where  Id = @userId", new { userId });
            }
        }

        public async Task<bool> UpdateNotificationSettings(int userId, NotificationSettings newSettings)
        {
            using (var db = _dbService.GetConnection())
            {
                return (await db.ExecuteAsync(@"
Update Users
Set    NotifyUrgentStrings = @NotifyUrgentStrings,
       NotifySuggestionsAwaitingApproval = @NotifySuggestionsAwaitingApproval,
       NotifySuggestionsAwaitingReview = @NotifySuggestionsAwaitingReview,
       NotifyStringsPushedToTransifex = @NotifyStringsPushedToTransifex,
       NotifySuggestionsApproved = @NotifySuggestionsApproved,
       NotifySuggestionsRejected = @NotifySuggestionsRejected,
       NotifySuggestionsReviewed = @NotifySuggestionsReviewed,
       NotifySuggestionsOverriden = @NotifySuggestionsOverriden,
       NotificationsIntervalId = @NotificationsInterval,
       NotificationsIntervalValue = @NotificationsIntervalValue
Where  Id = @userId", new
                {
                    newSettings.NotifyUrgentStrings,
                    newSettings.NotifySuggestionsAwaitingApproval,
                    newSettings.NotifySuggestionsAwaitingReview,
                    newSettings.NotifyStringsPushedToTransifex,
                    newSettings.NotifySuggestionsApproved,
                    newSettings.NotifySuggestionsRejected,
                    newSettings.NotifySuggestionsReviewed,
                    newSettings.NotifySuggestionsOverriden,
                    newSettings.NotificationsInterval,
                    newSettings.NotificationsIntervalValue,
                    userId
                })) == 1;
            }
        }

        public async Task<bool> AddNotificationBrowser(int userId, WebPushSubscription subscription)
        {
            using (var db = _dbService.GetConnection())
            {
                var existingNotifications = (await GetCurrentSubscriptions(userId, db)).AsList();
                if (!existingNotifications.Any(n => n.Endpoint == subscription.Endpoint))
                {
                    existingNotifications.Add(subscription);
                }
                else
                {
                    return true;
                }

                return (await SetCurrentSubscriptions(userId, existingNotifications, db)) == 1;
            }
        }

        public async Task<bool> SendNotification(int userId, Models.Services.PushNotificationMessage message)
        {
            using (var db = _dbService.GetConnection())
            {
                var endpointsToRemove = new List<string>();
                var success = false;

                var currentSubscriptions = await GetCurrentSubscriptions(userId, db);
                var options = new Dictionary<string, object> { ["vapidDetails"] = _vapidDetails };
                if (message.Topic.HasValue())
                {
                    options["headers"] = new Dictionary<string, object>
                    {
                        ["topic"] = message.Topic
                    };
                }

                foreach (var subscriptionData in currentSubscriptions)
                {
                    var subscription = new PushSubscription(subscriptionData.Endpoint, subscriptionData.P256dh, subscriptionData.Auth);
                    try
                    {
                        await _webPushClient.SendNotificationAsync(subscription, Jil.JSON.Serialize(message, Jil.Options.CamelCase), options);
                        success = true;
                    }
                    catch (WebPushException exception)
                    {
                        if (exception.StatusCode == HttpStatusCode.NotFound || exception.StatusCode == HttpStatusCode.Gone)
                        {
                            endpointsToRemove.Add(subscriptionData.Endpoint);
                        }
                    }
                }

                if (endpointsToRemove.Count > 0)
                {
                    await SetCurrentSubscriptions(userId, currentSubscriptions.Where(s => !endpointsToRemove.Contains(s.Endpoint)), db);
                }

                return success;
            }
        }

        private static Task<IEnumerable<WebPushSubscription>> GetCurrentSubscriptions(int userId, DbConnection db)
        {
            return db.QueryAsync<WebPushSubscription>(@"
Select      json_value(a.value, '$.Endpoint') Endpoint,
            json_value(a.value, '$.Auth') Auth,
            json_value(a.value, '$.P256dh') P256dh
From        Users u
Cross Apply OpenJson(u.NotificationDetails, '$') a
Where       Id = @userId", new { userId });
        }

        private static Task<int> SetCurrentSubscriptions(int userId, IEnumerable<WebPushSubscription> subscriptions, DbConnection db)
        {
            var content = Jil.JSON.Serialize(subscriptions);
            return db.ExecuteAsync(@"
Update Users
Set    NotificationDetails = @content
Where  Id = @userId", new { userId, content });
        }
    }
}