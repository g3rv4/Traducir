namespace Traducir.Web.ViewModels.Home
{
    public class StringCountsViewModel
    {
        public int TotalStrings { get; set; }

        public int WithoutTranslation { get; set; }

        public int WithPendingSuggestions { get; set; }

        public int WaitingApproval { get; set; }

        public int WaitingReview { get; set; }

        public int UrgentStrings { get; set; }
    }
}
