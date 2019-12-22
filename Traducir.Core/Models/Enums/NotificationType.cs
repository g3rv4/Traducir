using System;

namespace Traducir.Core.Models.Enums
{
    public enum NotificationType
    {
        UrgentStrings = 1,
        SuggestionsAwaitingApproval = 2,
        SuggestionsAwaitingReview = 3,
        StringsPushedToTransifex = 4,
        SuggestionsApproved = 5,
        SuggestionsRejected = 6,
        SuggestionsReviewed = 7,
        SuggestionsOverriden = 8,
    }

    public static class NotificationTypeExtensions
    {
        public static string GetTitle(this NotificationType type, int? count = null)
        {
            switch (type)
            {
                case NotificationType.UrgentStrings:
                    return $"{count} urgent strings";
                case NotificationType.SuggestionsAwaitingApproval:
                    return $"{count} awaiting approval";
                case NotificationType.SuggestionsAwaitingReview:
                    return $"{count} awaiting review";
                case NotificationType.StringsPushedToTransifex:
                    return "Strings pushed to transifex";
                case NotificationType.SuggestionsApproved:
                    return "Suggestions approved";
                case NotificationType.SuggestionsRejected:
                    return "Suggestions rejected";
                case NotificationType.SuggestionsReviewed:
                    return "Suggestions reviewed";
                case NotificationType.SuggestionsOverriden:
                    return "Suggestions overriden";
                default:
                    throw new ArgumentException($"Missing title for notification {type}");
            }
        }

        public static string GetBody(this NotificationType type)
        {
            switch (type)
            {
                case NotificationType.UrgentStrings:
                    return "Go to urgent strings";
                case NotificationType.SuggestionsAwaitingApproval:
                case NotificationType.SuggestionsAwaitingReview:
                    return "Go to these suggestions";
                case NotificationType.StringsPushedToTransifex:
                    return "Now we need a CM";
                case NotificationType.SuggestionsApproved:
                case NotificationType.SuggestionsRejected:
                case NotificationType.SuggestionsReviewed:
                case NotificationType.SuggestionsOverriden:
                    return "Go to your suggestions";
                default:
                    throw new ArgumentException($"Missing body for notification {type}");
            }
        }

        public static string GetUserColumnName(this NotificationType type)
        {
            switch (type)
            {
                case NotificationType.UrgentStrings:
                    return "NextNotificationUrgentStrings";
                case NotificationType.SuggestionsAwaitingApproval:
                    return "NextNotificationSuggestionsAwaitingApproval";
                case NotificationType.SuggestionsAwaitingReview:
                    return "NextNotificationSuggestionsAwaitingReview";
                case NotificationType.StringsPushedToTransifex:
                    return "NextNotificationStringsPushedToTransifex";
                case NotificationType.SuggestionsApproved:
                    return "NextNotificationSuggestionsApproved";
                case NotificationType.SuggestionsRejected:
                    return "NextNotificationSuggestionsRejected";
                case NotificationType.SuggestionsReviewed:
                    return "NextNotificationSuggestionsReviewed";
                case NotificationType.SuggestionsOverriden:
                    return "NextNotificationSuggestionsOverriden";
                default:
                    throw new ArgumentException($"Missing user column for notification {type}");
            }
        }

        public static bool ShouldBeBatched(this NotificationType type)
        {
            switch (type)
            {
                case NotificationType.UrgentStrings:
                case NotificationType.SuggestionsAwaitingApproval:
                case NotificationType.SuggestionsAwaitingReview:
                case NotificationType.StringsPushedToTransifex:
                    return true;
                case NotificationType.SuggestionsApproved:
                case NotificationType.SuggestionsRejected:
                case NotificationType.SuggestionsReviewed:
                case NotificationType.SuggestionsOverriden:
                    return false;
                default:
                    throw new ArgumentException($"Missing batching info for notification {type}");
            }
        }

        public static string GetUrl(this NotificationType type, bool useHttps, string host, int userId)
        {
            var basePath = $"{(useHttps ? "https" : "http")}://{host}";
            switch (type)
            {
                case NotificationType.UrgentStrings:
                    return $"{basePath}/filters?urgencyStatus=1";
                case NotificationType.SuggestionsAwaitingApproval:
                    return $"{basePath}/filters?suggestionsStatus=3";
                case NotificationType.SuggestionsAwaitingReview:
                    return $"{basePath}/filters?suggestionsStatus=4";
                case NotificationType.StringsPushedToTransifex:
                    return null;
                case NotificationType.SuggestionsApproved:
                case NotificationType.SuggestionsRejected:
                case NotificationType.SuggestionsReviewed:
                case NotificationType.SuggestionsOverriden:
                    return $"{basePath}/users/{userId}/suggestions";
                default:
                    throw new ArgumentException($"Missing url info for notification {type}");
            }
        }
    }
}