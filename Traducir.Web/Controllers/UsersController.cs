using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Traducir.Core.Helpers;
using Traducir.Core.Models.Enums;
using Traducir.Core.Services;
using Traducir.Web.ViewModels.Users;

namespace Traducir.Web.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IConfiguration _configuration;
        private readonly ISOStringService _soStringService;

        public UsersController(
            IUserService userService,
            IAuthorizationService authorizationService,
            IConfiguration configuration,
            ISOStringService soStringService)
        {
            _userService = userService;
            _authorizationService = authorizationService;
            _configuration = configuration;
            _soStringService = soStringService;
        }

        [Route("users")]
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

        [HttpPost]
        [Authorize(Policy = TraducirPolicy.CanManageUsers)]
        [Route("users/change-type")]
        public async Task<IActionResult> ChangeUserType([FromBody] ChangeUserTypeViewModel model)
        {
            // explicitly whitelist accepted types
            if (model.UserType != UserType.Banned && model.UserType != UserType.User && model.UserType != UserType.TrustedUser)
            {
                return BadRequest();
            }

            var success = await _userService.ChangeUserTypeAsync(model.UserId, model.UserType, User.GetClaim<int>(ClaimType.Id));
            if (!success)
            {
                return BadRequest();
            }

            var user = await _userService.GetUserAsync(model.UserId);
            var siteDomain = _configuration.GetValue<string>("STACKAPP_SITEDOMAIN");
            var currentUserCanManageUsers = (await _authorizationService.AuthorizeAsync(User, TraducirPolicy.CanManageUsers)).Succeeded;
            return View("UserSummary", new UserSummaryViewModel(user, siteDomain, currentUserCanManageUsers));
        }

        [Authorize]
        [Route("users/{userId:INT}/suggestions")]
        public async Task<IActionResult> SuggestionsByUser(int userId, StringSuggestionState? state)
        {
            var suggestions = await _soStringService.GetSuggestionsByUser(userId, state);
            var siteDomain = _configuration.GetValue<string>("STACKAPP_SITEDOMAIN");
            var model = new SuggestionsByUserViewModel { CurrentState = state, SiteDomain = siteDomain, UserId = userId, Suggestions = suggestions };

            return View(model);
        }

        [Authorize]
        [Route("users/me/notifications")]
        public async Task<IActionResult> Notifications()
        {
            var userId = User.GetClaim<int>(ClaimType.Id);
            var notificationSettings = await _userService.GetNotificationSettings(userId);
            var vapid = _configuration.GetValue<string>("VAPID_PUBLIC");

            var model = new NotificationsViewModel
            {
                NotificationSettings = notificationSettings,
                VapidPublic = vapid
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [Route("users/me/notifications/update")]
        public async Task<IActionResult> UpdateNotificationSettings([FromBody] UpdateNotificationSettingsViewModel model)
        {
            var userId = User.GetClaim<int>(ClaimType.Id);
            if (!await _userService.UpdateNotificationSettings(userId, model.Notifications))
            {
                return BadRequest();
            }

            if (!await _userService.AddNotificationBrowser(userId, model.Subscription.ToWebPushSubscription()))
            {
                return BadRequest();
            }

            return NoContent();
        }

        [HttpPost]
        [Authorize]
        [Route("/delete-notifications")]
        public async Task<IActionResult> DeleteNotifications()
        {
            var userId = User.GetClaim<int>(ClaimType.Id);
            await _userService.WipeNotificationDataAsync(userId);
            return NoContent();
        }
    }
}