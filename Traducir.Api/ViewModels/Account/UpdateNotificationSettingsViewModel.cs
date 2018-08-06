using Traducir.Core.Models;

namespace Traducir.Api.ViewModels.Account
{
    public class UpdateNotificationSettingsViewModel
    {
        public NotificationSettings Notifications { get; set; }

        public SubscriptionViewModel Subscription { get; set; }
    }
}