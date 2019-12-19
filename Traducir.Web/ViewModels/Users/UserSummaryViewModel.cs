using Traducir.Core.Models;
using Traducir.Core.Models.Enums;

namespace Traducir.Web.ViewModels.Users
{
    public class UserSummaryViewModel
    {
        private UserSummaryViewModel()
        {
        }

        public UserSummaryViewModel(User user, string siteDomain, bool currentUserCanManageUsers)
        {
            this.User = user;
            this.SiteDomain = siteDomain;
            this.CurrentUserCanManageUsers = currentUserCanManageUsers;
        }

        public User User { get; }

        public bool CurrentUserCanManageUsers { get; }

        private string SiteDomain { get; }

        public string UserProfileLink =>
            $"https://{SiteDomain}/users/{User.Id}";

        public bool UserCanBeBanned =>
            User.UserType != UserType.Banned &&
            User.UserType != UserType.TrustedUser &&
            User.UserType != UserType.Reviewer &&
            !User.IsModerator;
    }
}
