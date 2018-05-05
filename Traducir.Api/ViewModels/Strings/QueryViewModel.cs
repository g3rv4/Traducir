namespace Traducir.Api.ViewModels.Strings
{
#pragma warning disable CA1717
    public enum SuggestionApprovalStatus
    {
        AnyStatus = 0, // will default to this one

        DoesNotHaveSuggestions = 1,

        HasSuggestionsNeedingReview = 2,

        HasSuggestionsNeedingApproval = 3,

        HasSuggestionsNeedingReviewApprovedByTrustedUser = 4
    }

    public enum TranslationStatus
    {
        AnyStatus = 0,

        WithTranslation = 1,

        WithoutTranslation = 2
    }

    public enum PushStatus
    {
        AnyStatus = 0,

        NeedsPush = 1,

        DoesNotNeedPush = 2
    }

    public enum UrgencyStatus
    {
        AnyStatus = 0,

        IsUrgent = 1,

        IsNotUrgent = 2
    }

    public enum IgnoredStatus
    {
        AvoidIgnored = 0,

        OnlyIgnored = 1,

        IncludeIgnored = 2
    }
#pragma warning restore CA1717

    public class QueryViewModel
    {
        public string SourceRegex { get; set; }

        public string TranslationRegex { get; set; }

        public string Key { get; set; }

        public TranslationStatus TranslationStatus { get; set; }

        public SuggestionApprovalStatus SuggestionsStatus { get; set; }

        public PushStatus PushStatus { get; set; }

        public UrgencyStatus UrgencyStatus { get; set; }

        public IgnoredStatus IgnoredStatus { get; set; }
    }
}