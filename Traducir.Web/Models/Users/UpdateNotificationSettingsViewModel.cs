using Traducir.Core.Models;
using Traducir.Web.ViewModels.Users;

namespace Traducir.Web.Models.Users
{
    public class UpdateNotificationSettingsViewModel
    {
        public NotificationSettings Notifications { get; set; }

        public SubscriptionViewModel Subscription { get; set; }
    }
}