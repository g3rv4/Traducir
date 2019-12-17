using Traducir.Api.ViewModels.Strings;

namespace Traducir.Web.Net.ViewModels.Home
{
    public class IndexViewModel
    {
        public StringCountsViewModel StringCounts { get; set; }
        public QueryViewModel StringsQuery { get; set; }
        public FilterResultsViewModel FilterResults { get; set; }
        public bool UserCanSeeIgnoredAndPushStatus { get; set; }
    }
}
