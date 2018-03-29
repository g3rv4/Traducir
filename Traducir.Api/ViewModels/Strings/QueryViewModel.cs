namespace Traducir.ViewModels.Strings
{
    public class QueryViewModel
    {
        public enum SuggestionApprovalStatus
        {
            AnyStatus = 0, // will default to this one
            DoesNotHaveSuggestionsNeedingApproval = 1,
            HasSuggestionsNeedingApproval = 2,
            HasSuggestionsNeedingApprovalApprovedByTrustedUser = 3
        }

        public enum TranslationStatuses
        {
            AnyStatus = 0,
            WithTranslation = 1,
            WithoutTranslation = 2
        }

        public string SourceRegex { get; set; }
        public string TranslationRegex { get; set; }
        public TranslationStatuses TranslationStatus { get; set; }
        public SuggestionApprovalStatus SuggestionsStatus { get; set; }
    }
}