using System.Collections.Generic;
using System.Linq;
using Traducir.Core.Models;
using Traducir.Core.Models.Enums;

namespace Traducir.Web.Net.ViewModels.Home
{
    public class FilterResultsViewModel
    {
        public UserType UserType { get; set; }

        public IEnumerable<SOString> Strings { get; set; }

        public int Count => Strings.Count();

        public int ApprovedSuggestionsCountFor(SOString str) => str.Suggestions?.Count(s => s.State == StringSuggestionState.ApprovedByTrustedUser) ?? 0;

        public int PendingSuggestionsCountFor(SOString str) => str.Suggestions?.Count(s => s.State == StringSuggestionState.Created) ?? 0;

        public bool UserCanManageIgnoring => UserType >= UserType.TrustedUser;
    }
}
