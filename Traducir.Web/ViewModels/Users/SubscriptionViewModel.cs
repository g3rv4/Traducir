using Traducir.Core.Models;

namespace Traducir.Web.ViewModels.Users
{
    public class SubscriptionViewModel
    {
        public string Endpoint { get; set; }

        public KeysData Keys { get; set; }

        public WebPushSubscription ToWebPushSubscription()
        {
            return new WebPushSubscription
            {
                Endpoint = Endpoint,
                Auth = Keys.Auth,
                P256dh = Keys.P256dh
            };
        }

        public class KeysData
        {
            public string Auth { get; set; }

            public string P256dh { get; set; }
        }
    }
}