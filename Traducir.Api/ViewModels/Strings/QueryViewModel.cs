using System.ComponentModel.DataAnnotations;

namespace Traducir.Api.ViewModels.Strings
{
#pragma warning disable CA1717
    public enum SuggestionApprovalStatus
    {
        [Display(Name = "Any string", Order = 0)]
        AnyStatus = 0,

        [Display(Name = "Strings without suggestions", Order = 1)]
        DoesNotHaveSuggestions = 1,

        [Display(Name = "Strings with suggestions awaiting review", Order = 3)]
        HasSuggestionsNeedingReview = 2,

        [Display(Name = "Strings with suggestions awaiting approval", Order = 2)]
        HasSuggestionsNeedingApproval = 3,

        [Display(Name = "Strings with approved suggestions awaiting review", Order = 4)]
        HasSuggestionsNeedingReviewApprovedByTrustedUser = 4
    }

    public enum TranslationStatus
    {
        [Display(Name = "Any string", Order = 0)]
        AnyStatus = 0,

        [Display(Name = "Only strings with translation", Order = 2)]
        WithTranslation = 1,

        [Display(Name = "Only strings without translation", Order = 1)]
        WithoutTranslation = 2
    }

    public enum PushStatus
    {
        [Display(Name = "Any status", Order = 0)]
        AnyStatus = 0,

        [Display(Name = "Need push", Order = 1)]
        NeedsPush = 1,

        [Display(Name = "Don't need push", Order = 2)]
        DoesNotNeedPush = 2
    }

    public enum UrgencyStatus
    {
        [Display(Name = "Any string", Order = 0)]
        AnyStatus = 0,

        [Display(Name = "Is urgent", Order = 1)]
        IsUrgent = 1,

        [Display(Name = "Is not urgent", Order = 2)]
        IsNotUrgent = 2
    }

    public enum IgnoredStatus
    {
        [Display(Name = "Hide ignored", Order = 0)]
        AvoidIgnored = 0,

        [Display(Name = "Ignored only", Order = 1)]
        OnlyIgnored = 1,

        [Display(Name = "Ignored an not ignored", Order = 2)]
        IncludeIgnored = 2
    }
#pragma warning restore CA1717

    public class QueryViewModel
    {
        public static QueryViewModel Empty => new QueryViewModel();

        public bool IsEmpty =>
            string.IsNullOrEmpty(SourceRegex) &&
            string.IsNullOrEmpty(TranslationRegex) &&
            string.IsNullOrEmpty(Key) &&
            TranslationStatus == TranslationStatus.AnyStatus &&
            SuggestionsStatus == SuggestionApprovalStatus.AnyStatus &&
            PushStatus == PushStatus.AnyStatus &&
            UrgencyStatus == UrgencyStatus.AnyStatus &&
            IgnoredStatus == IgnoredStatus.AvoidIgnored;

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