namespace Traducir.Core.Models
{
    public class NotificationSettings
    {
        public bool NotifyUrgentStrings { get; set; }

        public bool NotifySuggestionsAwaitingApproval { get; set; }

        public bool NotifySuggestionsAwaitingReview { get; set; }

        public bool NotifyStringsPushedToTransifex { get; set; }

        public bool NotifySuggestionsApproved { get; set; }

        public bool NotifySuggestionsRejected { get; set; }

        public bool NotifySuggestionsReviewed { get; set; }

        public bool NotifySuggestionsOverriden { get; set; }
    }
}