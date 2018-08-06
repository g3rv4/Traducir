namespace Traducir.Core.Models.Services
{
    public class PushNotificationMessage
    {
        public string Title { get; set; }

        public string Content { get; set; }

        public bool RequireInteraction { get; set; }
    }
}