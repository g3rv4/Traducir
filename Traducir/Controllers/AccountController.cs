using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Traducir.Core.Models;
using Traducir.Core.Services;

namespace Traducir.Controllers
{
    public class AccountController : Controller
    {
        private ISEApiService _seApiService { get; }
        private IConfiguration _configuration { get; }
        private IUserService _userService { get; }

        public AccountController(IConfiguration configuration,
            ISEApiService seApiService,
            IUserService userService)
        {
            _seApiService = seApiService;
            _configuration = configuration;
            _userService = userService;
        }

        string GetOauthReturnUrl()
        {
            return Url.Action("OauthCallback", null, null, "https");
        }

        [Route("app/login")]
        public IActionResult LogIn(string returnUrl)
        {
            return Redirect(_seApiService.GetInitialOauthUrl(GetOauthReturnUrl(), returnUrl));
        }

        [Route("app/logout")]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync();
            return Content("bye!");
        }

        [Route("app/oauth-callback")]
        public async Task<IActionResult> OauthCallback(string code)
        {
            var siteDomain = _configuration.GetValue<string>("STACKAPP_SITEDOMAIN");

            var accessToken = await _seApiService.GetAccessTokenFromCodeAsync(code, GetOauthReturnUrl());
            var currentUser = await _seApiService.GetMyUserAsync(siteDomain, accessToken);

            if (currentUser == null)
            {
                return Content("Could not retrieve a user account on " + siteDomain);
            }

            if (currentUser.Reputation < 5)
            {
                return Content("You need more reputation to log in");
            }

            await _userService.UpsertUserAsync(new User
            {
                Id = currentUser.UserId,
                    DisplayName = currentUser.DisplayName,
                    IsModerator = currentUser.UserType == "moderator",
                    CreationDate = DateTime.UtcNow,
                    LastSeenDate = DateTime.UtcNow
            });

            var user = await _userService.GetUserAsync(currentUser.UserId);

            var claims = new List<Claim>
            {
                new Claim("Id", user.Id.ToString()),
                new Claim("Name", currentUser.DisplayName)
            };
            if (!user.IsBanned)
            {
                claims.Add(new Claim("CanSuggest", "1"));
                if (user.IsReviewer || user.IsTrusted)
                {
                    claims.Add(new Claim("CanReview", "1"));
                }
            }
            var identity = new ClaimsIdentity(claims, "login");

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            return Redirect("/");
        }
    }
}