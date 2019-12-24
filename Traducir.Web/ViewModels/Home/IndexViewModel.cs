namespace Traducir.Web.ViewModels.Home
{
    public class IndexViewModel
    {
        public StringCountsViewModel StringCounts { get; set; }

        public QueryViewModel StringsQuery { get; set; }

        public FilterResultsViewModel FilterResults { get; set; }

        public bool UserCanSeeIgnoredAndPushStatus { get; set; }

        public int? StringId { get; set; }
    }
}
