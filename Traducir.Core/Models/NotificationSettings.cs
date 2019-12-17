using System.ComponentModel.DataAnnotations;
using Traducir.Core.Models.Enums;

namespace Traducir.Core.Models
{
    public class NotificationSettings
    {
        [Display(Name = "Urgent Strings", Order = 0)]
        public bool NotifyUrgentStrings { get; set; }

        [Display(Name = "Suggestions awaiting approval", Order = 1)]
        public bool NotifySuggestionsAwaitingApproval { get; set; }

        [Display(Name = "Suggestions awaiting review", Order = 2)]
        public bool NotifySuggestionsAwaitingReview { get; set; }

        [Display(Name = "Strings pushed to Transifex", Order = 3)]
        public bool NotifyStringsPushedToTransifex { get; set; }

        [Display(Name = "Suggestion approved", Order = 4)]
        public bool NotifySuggestionsApproved { get; set; }

        [Display(Name = "Suggestion rejected", Order = 5)]
        public bool NotifySuggestionsRejected { get; set; }

        [Display(Name = "Suggestion reviewed", Order = 6)]
        public bool NotifySuggestionsReviewed { get; set; }

        [Display(Name = "Suggestion overriden", Order = 7)]
        public bool NotifySuggestionsOverriden { get; set; }

        public NotificationInterval NotificationsInterval { get; set; }

        public int NotificationsIntervalValue { get; set; }
    }
}