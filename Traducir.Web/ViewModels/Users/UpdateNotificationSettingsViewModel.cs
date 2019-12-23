using Traducir.Core.Models;

namespace Traducir.Web.ViewModels.Users
{
    public class UpdateNotificationSettingsViewModel
    {
        public NotificationSettings Notifications { get; set; }

        public SubscriptionViewModel Subscription { get; set; }
    }
}