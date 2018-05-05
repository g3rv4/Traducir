using System;
using System.Linq;
using Traducir.Core.Helpers;
using Traducir.Core.Models.Enums;

namespace Traducir.Core.Models
{
    public class SOString
    {
        private bool? _hasTranslation;

        private bool? _hasSuggestions;

        private bool? _hasSuggestionsWaitingApproval;

        private bool? _hasApprovedSuggestionsWaitingReview;

        public int Id { get; set; }

        public string Key { get; set; }

        public string FamilyKey { get; set; }

        public string OriginalString { get; set; }

        public string Translation { get; set; }

        public bool HasTranslation => _hasTranslation ?? (_hasTranslation = Translation.HasValue()).Value;

        public bool NeedsPush { get; set; }

        public bool IsUrgent { get; set; }

        public bool IsIgnored { get; set; }

        public string Variant { get; set; }

        public DateTime CreationDate { get; set; }

        public SOStringSuggestion[] Suggestions { get; set; }

        public bool HasSuggestions => _hasSuggestions ?? (_hasSuggestions = Suggestions != null && Suggestions.Any()).Value;

        public bool HasSuggestionsWaitingApproval => _hasSuggestionsWaitingApproval ??
            (_hasSuggestionsWaitingApproval = Suggestions != null && Suggestions.Any(sug => sug.State == StringSuggestionState.Created)).Value;

        public bool HasApprovedSuggestionsWaitingReview => _hasApprovedSuggestionsWaitingReview ??
            (_hasApprovedSuggestionsWaitingReview = Suggestions != null && Suggestions.Any(sug => sug.State == StringSuggestionState.ApprovedByTrustedUser)).Value;
    }
}