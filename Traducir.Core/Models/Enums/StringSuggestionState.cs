using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Traducir.Core.Models.Enums
{
    public enum StringSuggestionState
    {
        [Display(Name = "Created")]
        Created = 1,

        [Display(Name = "Approved by trusted user")]
        ApprovedByTrustedUser = 2,

        [Display(Name = "Approved by reviewer")]
        ApprovedByReviewer = 3,

        [Display(Name = "Rejected")]
        Rejected = 4,

        [Display(Name = "Deleted by owner")]
        DeletedByOwner = 5,

        [Display(Name = "Dismissed by other string")]
        DismissedByOtherString = 6
    }

    public static class StringSuggestionStateExtensions
    {
        public static NotificationType? GetNotificationType(this StringSuggestionState state)
        {
            switch (state)
            {
                case StringSuggestionState.ApprovedByTrustedUser:
                    return NotificationType.SuggestionsApproved;
                case StringSuggestionState.ApprovedByReviewer:
                    return NotificationType.SuggestionsReviewed;
                case StringSuggestionState.Rejected:
                    return NotificationType.SuggestionsRejected;
                case StringSuggestionState.DismissedByOtherString:
                    return NotificationType.SuggestionsOverriden;
                case StringSuggestionState.Created:
                case StringSuggestionState.DeletedByOwner:
                    return null;
                default:
                    throw new ArgumentException($"Missing NotificationType for {state}");
            }
        }

        public static string DisplayName(this StringSuggestionState state)
        {
            if (!Enum.IsDefined(typeof(StringSuggestionState), state))
            {
                return "Unknown";
            }

            return typeof(StringSuggestionState)
                .GetMember(state.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()
                .Name;
        }
    }
}