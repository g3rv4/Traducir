using System;
using System.Collections.Generic;
using System.Linq;
using Traducir.Core.Models;
using Traducir.Core.Models.Enums;

namespace Traducir.Web.ViewModels.Home
{
    public class SuggestionsByStringViewModel
    {
        public IEnumerable<SOStringSuggestion> Suggestions { get; set; }

        public int StringId { get; set; }

        public SOString String { get; set; }

        public string SiteDomain { get; set; }

        public static string BadgeClassForState(StringSuggestionState state)
        {
            switch (state)
            {
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
    }
}
