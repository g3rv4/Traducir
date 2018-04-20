using System;
using System.Linq;
using Traducir.Core.Helpers;
using Traducir.Core.Models.Enums;

namespace Traducir.Core.Models
{
    public class SOString
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string OriginalString { get; set; }
        public string Translation { get; set; }
        private bool? _hasTranslation;
        public bool HasTranslation => _hasTranslation ?? (_hasTranslation = Translation.HasValue()).Value;
        public bool NeedsPush { get; set; }
        public bool IsUrgent { get; set; }
        public string Variant { get; set; }
        public DateTime CreationDate { get; set; }

        public SOStringSuggestion[] Suggestions { get; set; }
        private bool? _hasSuggestions;
        public bool HasSuggestions => _hasSuggestions ?? (_hasSuggestions = Suggestions != null && Suggestions.Any()).Value;
        private bool? _hasSuggestionsWaitingApproval;
        public bool HasSuggestionsWaitingApproval => _hasSuggestionsWaitingApproval ??
            (_hasSuggestionsWaitingApproval = Suggestions != null && Suggestions.Any(sug => sug.State == StringSuggestionState.Created)).Value;
        private bool? _hasApprovedSuggestionsWaitingReview;
        public bool HasApprovedSuggestionsWaitingReview => _hasApprovedSuggestionsWaitingReview ??
            (_hasApprovedSuggestionsWaitingReview = Suggestions != null && Suggestions.Any(sug => sug.State == StringSuggestionState.ApprovedByTrustedUser)).Value;
    }
}