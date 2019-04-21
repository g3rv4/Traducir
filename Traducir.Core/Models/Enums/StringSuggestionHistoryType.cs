using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Traducir.Core.Models.Enums
{
    public enum StringSuggestionHistoryType
    {
        [Display(Name = "Created")]
        Created = 1,

        [Display(Name = "Approved by trusted user")]
        ApprovedByTrusted = 2,

        [Display(Name = "Approved by reviewer")]
        ApprovedByReviewer = 3,

        [Display(Name = "Rejected by trusted user")]
        RejectedByTrusted = 4,

        [Display(Name = "Rejected by reviewer")]
        RejectedByReviewer = 5,

        [Display(Name = "Deleted by owner")]
        DeletedByOwner = 6,

        [Display(Name = "Dismissed by other string")]
        DismissedByOtherString = 7,

        [Display(Name = "Replaced by owner")]
        ReplacedByOwner = 8
    }

    public static class StringSuggestionHistoryTypeExtensions
    {
        public static string DisplayName(this StringSuggestionHistoryType state)
        {
            return typeof(StringSuggestionHistoryType)
                .GetMember(state.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()
                .Name;
        }
    }
}