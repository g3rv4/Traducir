using System;

namespace Traducir.Core.Models.Enums
{
    public enum StringSuggestionState
    {
        Created = 1,
        ApprovedByTrustedUser = 2,
        ApprovedByReviewer = 3,
        Rejected = 4,
        DeletedByOwner = 5,
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
    }
}