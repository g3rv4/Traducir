using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Traducir.Api.ViewModels.Account;
using Traducir.Core.Helpers;
using Traducir.Core.Models;
using Traducir.Core.Models.Enums;
using Traducir.Core.Services;

namespace Traducir.Api.Controllers
{
    public class AccountController : Controller
    {
        private readonly ISEApiService _seApiService;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly IAuthorizationService _authorizationService;
        private readonly bool _isDevelopmentEnvironment;

        public AccountController(
            IConfiguration configuration,
            ISEApiService seApiService,
            IUserService userService,
            IAuthorizationService authorizationService,
            IHostingEnvironment hostingEnvironment)
        {
            _seApiService = seApiService;
            _configuration = configuration;
            _userService = userService;
            _authorizationService = authorizationService;
            _isDevelopmentEnvironment = hostingEnvironment.IsDevelopment();
        }

        [Route("app/impersonate")]
        public async Task<IActionResult> Impersonate(string returnUrl, int userId)
        {
            if (!_isDevelopmentEnvironment)
            {
                return NotFound();
            }

            await _userService.CreateFakeUsers();

            var user = await _userService.GetUserAsync(userId);
            await LoginUser(user.Id, null, false);

            return Redirect(returnUrl ?? "/");
        }

        [Route("app/login")]
        public IActionResult LogIn(string returnUrl, string asUserType = null, bool asModerator = false)
        {
            var state = _isDevelopmentEnvironment ? $"{asUserType} {(asModerator ? "true" : null)} {returnUrl}" : returnUrl;

            return Redirect(_seApiService.GetInitialOauthUrl(GetOauthReturnUrl(), state));
        }

        [Route("app/logout")]
        public async Task<IActionResult> LogOut(string returnUrl = null)
        {
            await HttpContext.SignOutAsync();
            return Redirect(returnUrl ?? "/");
        }

        [Route("app/oauth-callback")]
        public async Task<IActionResult> OauthCallback(string code, string state = null)
        {
            string returnUrl, asUserType;
            bool asModerator;
            if (_isDevelopmentEnvironment && state != null)
            {
                var splitParts = state.Split(' ');
                asUserType = splitParts[0].NullIfEmpty();
                asModerator = splitParts[1] == "true";
                returnUrl = splitParts[2].NullIfEmpty();
            }
            else
            {
                asUserType = null;
                asModerator = false;
                returnUrl = state;
            }

            var siteDomain = _configuration.GetValue<string>("STACKAPP_SITEDOMAIN");

            var accessToken = await _seApiService.GetAccessTokenFromCodeAsync(code, GetOauthReturnUrl());
            var currentUser = await _seApiService.GetMyUserAsync(siteDomain, accessToken);

            if (currentUser == null)
            {
                return Content("Could not retrieve a user account on " + siteDomain);
            }

            var minRep = _configuration.GetValue<int>("MIN_REP_TO_LOGIN");
            if (currentUser.Reputation < minRep)
            {
                return Content($"You need at least {minRep} to log in");
            }

            await _userService.UpsertUserAsync(new User
            {
                Id = currentUser.UserId,
                DisplayName = currentUser.DisplayName,
                IsModerator = asModerator || currentUser.UserType == "moderator",
                CreationDate = DateTime.UtcNow,
                LastSeenDate = DateTime.UtcNow
            });

            await LoginUser(currentUser.UserId, asUserType, asModerator);

            return Redirect(returnUrl ?? "/");
        }

        [Authorize]
        [Route("app/api/me")]
        public async Task<IActionResult> WhoAmI()
        {
            var canSuggest = (await _authorizationService.AuthorizeAsync(User, TraducirPolicy.CanSuggest)).Succeeded;
            var canReview = (await _authorizationService.AuthorizeAsync(User, TraducirPolicy.CanReview)).Succeeded;
            var canManageUsers = (await _authorizationService.AuthorizeAsync(User, TraducirPolicy.CanManageUsers)).Succeeded;

            return Json(new UserInfo
            {
                Name = User.GetClaim<string>(ClaimType.Name),
                UserType = User.GetClaim<UserType>(ClaimType.UserType),
                CanSuggest = canSuggest,
                CanReview = canReview,
                CanManageUsers = canManageUsers,
                Id = User.GetClaim<int>(ClaimType.Id)
            });
        }

        [HttpGet]
        [Authorize]
        [Route("app/api/me/notification-settings")]
        public async Task<IActionResult> NotificationSettings()
        {
            var userId = User.GetClaim<int>(ClaimType.Id);
            return Json(await _userService.GetNotificationSettings(userId));
        }

        [HttpPut]
        [Authorize]
        [Route("app/api/me/notification-settings")]
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

        [Authorize]
        [Route("app/api/users")]
        public async Task<IActionResult> GetUsers()
        {
            return Json((await _userService.GetUsersAsync())
                .OrderByDescending(u => u.UserType)
                .ThenByDescending(u => u.LastSeenDate));
        }

        [HttpPut]
        [Authorize(Policy = TraducirPolicy.CanManageUsers)]
        [Route("app/api/users/change-type")]
        public async Task<IActionResult> ChangeUserType([FromBody] ChangeUserTypeViewModel model)
        {
            // explicitly whitelist accepted types
            if (model.UserType != UserType.Banned && model.UserType != UserType.User && model.UserType != UserType.TrustedUser)
            {
                return BadRequest();
            }

            var success = await _userService.ChangeUserTypeAsync(model.UserId, model.UserType, User.GetClaim<int>(ClaimType.Id))
                ;
            if (!success)
            {
                return BadRequest();
            }

            return NoContent();
        }

        private async Task LoginUser(int userId, string asUserType, bool asModerator)
        {
            var user = await _userService.GetUserAsync(userId);
            var userTypeString = asUserType == null ? user.UserType.ToString() : Enum.Parse(typeof(UserType), asUserType).ToString();
            var userType = (UserType)Enum.Parse(typeof(UserType), userTypeString);

            var claims = new List<Claim>
            {
                new Claim(ClaimType.Id, user.Id.ToString(CultureInfo.InvariantCulture)),
                new Claim(ClaimType.Name, user.DisplayName),
                new Claim(ClaimType.UserType, userTypeString),
            };
            if (userType >= UserType.User)
            {
                claims.Add(new Claim(ClaimType.CanSuggest, "1"));
                if (userType >= UserType.TrustedUser)
                {
                    claims.Add(new Claim(ClaimType.CanReview, "1"));
                }
            }

            if (asModerator || user.IsModerator)
            {
                claims.Add(new Claim(ClaimType.IsModerator, "1"));
            }

            var identity = new ClaimsIdentity(claims, "login");

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));
        }

        private string GetOauthReturnUrl()
        {
            return Url.Action("OauthCallback", null, null, _configuration.GetValue<bool>("USE_HTTPS") ? "https" : "http");
        }
    }
}