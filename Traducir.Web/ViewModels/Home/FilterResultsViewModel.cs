using System.Collections.Generic;
using System.Linq;
using Traducir.Core.Models;

namespace Traducir.Web.ViewModels.Home
{
    public class FilterResultsViewModel
    {
        private FilterResultsViewModel()
        {
        }

        public FilterResultsViewModel(IEnumerable<SOString> strings, bool userCanManageIgnoring)
        {
            StringSummaries = strings.Select(str => new StringSummaryViewModel { String = str, UserCanManageIgnoring = userCanManageIgnoring });
            Count = strings.Count();
        }

        public IEnumerable<StringSummaryViewModel> StringSummaries { get; }

        public int Count { get; }
    }
}
