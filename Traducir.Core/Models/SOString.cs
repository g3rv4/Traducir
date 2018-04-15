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
        private bool? _HasTranslation;
        public bool HasTranslation => _HasTranslation ?? (_HasTranslation = Translation.HasValue()).Value;
        public bool NeedsPush { get; set; }
        public bool IsUrgent { get; set; }
        public string Variant { get; set; }
        public DateTime CreationDate { get; set; }

        public SOStringSuggestion[] Suggestions { get; set; }
        private bool? _HasSuggestions;
        public bool HasSuggestions => _HasSuggestions ?? (_HasSuggestions = Suggestions != null && Suggestions.Any()).Value;
        private bool? _HasSuggestionsWaitingApproval;
        public bool HasSuggestionsWaitingApproval => _HasSuggestionsWaitingApproval ?? 
            (_HasSuggestionsWaitingApproval = Suggestions != null && Suggestions.Any(sug => sug.State == StringSuggestionState.Created)).Value;
        private bool? _HasApprovedSuggestionsWaitingReview;
        public bool HasApprovedSuggestionsWaitingReview => _HasApprovedSuggestionsWaitingReview ?? 
            (_HasApprovedSuggestionsWaitingReview = Suggestions != null && Suggestions.Any(sug => sug.State == StringSuggestionState.ApprovedByTrustedUser)).Value;
    }
}