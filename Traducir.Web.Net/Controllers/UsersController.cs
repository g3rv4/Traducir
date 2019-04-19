using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Traducir.Core.Helpers;
using Traducir.Core.Services;
using Traducir.Web.Net.ViewModels.Users;

namespace Traducir.Web.Net.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IConfiguration _configuration;

        public UsersController(
            IUserService userService,
            IAuthorizationService authorizationService,
            IConfiguration configuration)
        {
            this._userService = userService;
            this._authorizationService = authorizationService;
            this._configuration = configuration;
        }

        [Route("/users")]
        public async Task<IActionResult> Users()
        {
            var users = (await _userService.GetUsersAsync())
                .OrderByDescending(u => u.UserType)
                .ThenByDescending(u => u.LastSeenDate)
                .ToList();
            var siteDomain = _configuration.GetValue<string>("STACKAPP_SITEDOMAIN");
            var currentUserCanManageUsers = (await _authorizationService.AuthorizeAsync(User, TraducirPolicy.CanManageUsers)).Succeeded;

            var model = new UsersListViewModel(users, siteDomain, currentUserCanManageUsers);
            return View(model);
        }
    }
}