using System;
using System.Collections.Generic;
using System.Linq;
using Traducir.Core.Models;
using Traducir.Core.Models.Enums;

namespace Traducir.Web.ViewModels.Users
{
    public class SuggestionsByUserViewModel
    {
        public IEnumerable<SOStringSuggestion> Suggestions { get; set; }

        public int UserId { get; set; }

        public string SiteDomain { get; set; }

        public StringSuggestionState? CurrentState { get; set; }

        public string BadgeClassForState(StringSuggestionState state) {
            switch (state) {
                case StringSuggestionState.Created:
                    return "badge-secondary";
                case StringSuggestionState.ApprovedByReviewer:
                case StringSuggestionState.ApprovedByTrustedUser:
                    return "badge-success";
                case StringSuggestionState.Rejected:
                    return "badge-danger";
                case StringSuggestionState.DeletedByOwner:
                    return "badge-dark";
                default:
                    return null;
            }
        }

        public IEnumerable<StringSuggestionState> SuggestionStatesForFilters =>
            Enum.GetValues(typeof(StringSuggestionState)).OfType<StringSuggestionState>().OrderBy(s => (int)s);
    }
}
