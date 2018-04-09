namespace Traducir.ViewModels.Strings
{
    public class QueryViewModel
    {
        public enum SuggestionApprovalStatus
        {
            AnyStatus = 0, // will default to this one
            DoesNotHaveSuggestions = 1,
            HasSuggestionsNeedingReview = 2,
            HasSuggestionsNeedingApproval = 3,
            HasSuggestionsNeedingReviewApprovedByTrustedUser = 4
        }

        public enum TranslationStatuses
        {
            AnyStatus = 0,
            WithTranslation = 1,
            WithoutTranslation = 2
        }

        public enum PushStatuses
        {
            AnyStatus = 0,
            NeedsPush = 1,
            DoesNotNeedPush = 2
        }

        public string SourceRegex { get; set; }
        public string TranslationRegex { get; set; }
        public string Key { get; set; }
        public TranslationStatuses TranslationStatus { get; set; }
        public SuggestionApprovalStatus SuggestionsStatus { get; set; }
        public PushStatuses PushStatus { get; set; }
    }
}