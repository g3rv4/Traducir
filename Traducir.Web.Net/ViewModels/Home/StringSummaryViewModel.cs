using System.Linq;
using Traducir.Core.Models;
using Traducir.Core.Models.Enums;

namespace Traducir.Web.Net.ViewModels.Home
{
    public class StringSummaryViewModel
    {
        public SOString String { get; set; }

        public int ApprovedSuggestionsCount => String.Suggestions?.Count(s => s.State == StringSuggestionState.ApprovedByTrustedUser) ?? 0;

        public int PendingSuggestionsCount => String.Suggestions?.Count(s => s.State == StringSuggestionState.Created) ?? 0;

        public bool UserCanManageIgnoring { get; set; }

        public bool RenderAsChanged { get; set; }
    }
}
