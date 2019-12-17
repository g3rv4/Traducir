using Traducir.Core.Models;
using Traducir.Core.Models.Enums;

namespace Traducir.Web.Net.ViewModels.Home
{
    public class EditStringViewModel
    {
        public string SiteDomain { get; set; }

        public string TransifexPath { get; set; }

        public bool UserIsLoggedIn { get; set; }

        public int UserId { get; set; }

        public UserType UserType { get; set; }

        public bool UserCanSuggest { get; set; }

        public bool UserCanReview { get; set; }

        public SOString String { get; set; }

        public bool CurrentUserSuggested(SOStringSuggestion suggestion) =>
            UserIsLoggedIn && suggestion.CreatedById == UserId;

        public bool MustRenderSuggestionActionsFor(SOStringSuggestion suggestion)
        {
            if(!UserIsLoggedIn || !UserCanReview)
            {
                return false;
            }

            // a trusted user can't act on a suggestion approved by a trusted user
            return !(suggestion.State == StringSuggestionState.ApprovedByTrustedUser && UserType == UserType.TrustedUser);
        }

        public bool CanSendFinalTranslation => UserType == UserType.Reviewer;
    }
}
