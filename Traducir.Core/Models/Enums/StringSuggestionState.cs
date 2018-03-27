namespace Traducir.Core.Models.Enums
{
    public enum StringSuggestionState
    {
        Created = 1,
        ApprovedByTrustedUser = 2,
        ApprovedByReviewer = 3,
        Rejected = 4,
        DeletedByOwner = 5,
        DismissedByOtherString = 6
    }
}