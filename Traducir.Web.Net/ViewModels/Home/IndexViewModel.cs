namespace Traducir.Web.Net.ViewModels.Home
{
    public class IndexViewModel
    {
        public int TotalStringsCount { get; set; }

        public int UrgentStringsCount { get; set; }

        public int UntranslatedStringsCount { get; set; }

        public int SugestionsAwaitingApprovalCount { get; set; }

        public int ApprovedSugestionsAwaitingReviewCount { get; set; }
    }
}
