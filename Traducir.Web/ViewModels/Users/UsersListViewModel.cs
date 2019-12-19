using System.Collections.Generic;
using System.Linq;
using Traducir.Core.Models;

namespace Traducir.Web.ViewModels.Users
{
    public class UsersListViewModel
    {
        private UsersListViewModel()
        {
        }

        public UsersListViewModel(IEnumerable<User> users, string siteDomain, bool currentUserCanManageUsers)
        {
            Users = users.Select(u =>
                new UserSummaryViewModel(u, siteDomain, currentUserCanManageUsers));
        }

        public IEnumerable<UserSummaryViewModel> Users { get; }
    }
}
