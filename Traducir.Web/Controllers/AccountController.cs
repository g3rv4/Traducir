using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Traducir.Core.Helpers;
using Traducir.Core.Models;
using Traducir.Core.Models.Enums;
using Traducir.Core.Services;

namespace Traducir.Web.Controllers
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

        [Route("login")]
        public IActionResult LogIn(string returnUrl)
        {
            return Redirect(_seApiService.GetInitialOauthUrl(GetOauthReturnUrl(), returnUrl));
        }

        [Route("logout")]
        public async Task<IActionResult> LogOut(string returnUrl = null)
        {
            await HttpContext.SignOutAsync();
            return Redirect(returnUrl ?? "/");
        }

        [Route("oauth-callback")]
        public async Task<IActionResult> OauthCallback(string code, string state = null)
        {
            string returnUrl = state;
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
                IsModerator = currentUser.UserType == "moderator",
                CreationDate = DateTime.UtcNow,
                LastSeenDate = DateTime.UtcNow
            });

            await LoginUser(currentUser.UserId);

            return Redirect(returnUrl ?? "/");
        }

        private async Task LoginUser(int userId)
        {
            var user = await _userService.GetUserAsync(userId);

            var claims = new List<Claim>
            {
                new Claim(ClaimType.Id, user.Id.ToString(CultureInfo.InvariantCulture)),
                new Claim(ClaimType.Name, user.DisplayName),
                new Claim(ClaimType.UserType, user.UserType.ToString()),
            };
            if (user.UserType >= UserType.User)
            {
                claims.Add(new Claim(ClaimType.CanSuggest, "1"));
                if (user.UserType >= UserType.TrustedUser)
                {
                    claims.Add(new Claim(ClaimType.CanReview, "1"));
                }
            }

            if (user.IsModerator)
            {
                claims.Add(new Claim(ClaimType.IsModerator, "1"));
            }

            var identity = new ClaimsIdentity(claims, "login");

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));
        }

        private string GetOauthReturnUrl() =>
            Url.Action("OauthCallback", null, null, _configuration.GetValue<bool>("USE_HTTPS") ? "https" : "http");
    }
}