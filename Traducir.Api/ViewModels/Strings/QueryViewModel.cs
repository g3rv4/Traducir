namespace Traducir.ViewModels.Strings
{
    public class QueryViewModel
    {
        public enum SuggestionApprovalStatuses {
            AnyStatus = 0, // will default to this one
            DoesNotHaveSuggestionsNeedingApproval = 1,
            HasSuggestionsNeedingApproval = 2,
            HasSuggestionsNeedingApprovalApprovedByTrustedUser = 3
        }

        public string SourceRegex { get; set; }
        public string TranslationRegex { get; set; }
        public bool? WithoutTranslation { get; set; }
        public SuggestionApprovalStatuses SuggestionsStatus { get; set; }
    }
}