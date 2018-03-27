namespace Traducir.Core.Models.Enums
{
    public enum StringSuggestionHistoryType
    {
        Created = 1,
        ApprovedByTrusted = 2,
        ApprovedByReviewer = 3,
        RejectedByTrusted = 4,
        RejectedByReviewer = 5,
        DeletedByOwner = 6,
        DismissedByOtherString = 7
    }
}