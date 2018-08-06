using Traducir.Core.Models;

namespace Traducir.Api.ViewModels
{
    public class UpdateNotificationSettingsViewModel
    {
        public NotificationSettings Notifications { get; set; }

        public WebPushSubscription Subscription { get; set; }
    }
}