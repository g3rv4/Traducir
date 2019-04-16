using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Traducir.Api.Services;
using Traducir.Core.Helpers;
using Traducir.Web.Net.Models;
using Traducir.Web.Net.ViewModels.Home;
using Traducir.Api.ViewModels.Strings;
using Traducir.Core.Models.Enums;
using System;
using Traducir.Core.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Traducir.Core.Services;
using Microsoft.Extensions.Configuration;

namespace Traducir.Web.Net.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStringsService stringsService;
        private readonly IAuthorizationService authorizationService;
        private readonly ISOStringService soStringsService;
        private readonly IConfiguration configuration;

        public HomeController(
            IStringsService stringsService,
            IAuthorizationService authorizationService,
            ISOStringService soStringsService,
            IConfiguration configuration)
        {
            this.stringsService = stringsService;
            this.authorizationService = authorizationService;
            this.soStringsService = soStringsService;
            this.configuration = configuration;
        }

        public Task<IActionResult> Index()
        {
            return Filters(null);
        }

        [Route("/filters")]
        public async Task<IActionResult> Filters(QueryViewModel query)
        {
            FilterResultsViewModel filterResults = null;

            if(query == null)
            {
                query = new QueryViewModel();
            }
            else if(query.IsEmpty)
            {
                //let's redirect to root if we get just '/filters'
                return RedirectToAction(nameof(Index));
            }
            else
            {
                filterResults = await GetFilterResultsViewModelFor(query);
                if(filterResults == null)
                {
                    return BadRequest();
                }
            }

            var viewModel = new IndexViewModel
            {
                StringCounts = await stringsService.GetStringCounts(),
                StringsQuery = query,
                FilterResults = filterResults,
                UserCanSeeIgnoredAndPushStatus = User.GetClaim<UserType>(ClaimType.UserType) >= UserType.TrustedUser
            };

            return View("~/Views/Home/Index.cshtml", viewModel);
        }

        [Route("/strings_list")]
        public async Task<IActionResult> StringsList(QueryViewModel query)
        {
            var viewModel = await GetFilterResultsViewModelFor(query);

            return viewModel == null ? (IActionResult)BadRequest() : PartialView("FilterResults", viewModel);
        }

        private async Task<FilterResultsViewModel> GetFilterResultsViewModelFor(QueryViewModel query)
        {
            if(User.GetClaim<UserType>(ClaimType.UserType) < UserType.TrustedUser)
            {
                query.IgnoredStatus = IgnoredStatus.AvoidIgnored;
                query.PushStatus = PushStatus.AnyStatus;
            }

            IEnumerable<SOString> strings;

            try
            {
                strings = await stringsService.Query(query);
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            return new FilterResultsViewModel(
                strings,
                userCanManageIgnoring: (await authorizationService.AuthorizeAsync(User, TraducirPolicy.CanReview)).Succeeded);
        }

        [HttpPut]
        [Authorize(Policy = TraducirPolicy.CanReview)]
        [Route("/manage-ignore")]
        public async Task<IActionResult> ManageIgnore([FromBody] ManageIgnoreViewModel model)
        {
            var success = await soStringsService.ManageIgnoreAsync(
                model.StringId,
                model.Ignored,
                User.GetClaim<int>(ClaimType.Id),
                User.GetClaim<UserType>(ClaimType.UserType));

            if (!success)
            {
                return BadRequest();
            }

            var str = await soStringsService.GetStringByIdAsync(model.StringId);
            var summaryViewModel = new StringSummaryViewModel { String = str, RenderAsChanged = true, UserCanManageIgnoring = true };
            return PartialView("StringSummary", summaryViewModel);
        }

        [Route("/string_edit_ui")]
        public async Task<IActionResult> GetStringEditUi(int stringId)
        {
            var str = await soStringsService.GetStringByIdAsync(stringId);
            if (str == null)
            {
                return NotFound();
            }

            var viewModel = new EditStringViewModel
            {
                SiteDomain = configuration.GetValue<string>("STACKAPP_SITEDOMAIN"),
                TransifexPath = configuration.GetValue<string>("TRANSIFEX_LINK_PATH"),
                String = str,
                UserIsLoggedIn = User.GetClaim<string>(ClaimType.Name) != null,
                UserId = User.GetClaim<int>(ClaimType.Id),
                UserCanReview = (await authorizationService.AuthorizeAsync(User, TraducirPolicy.CanReview)).Succeeded,
                UserCanSuggest = (await authorizationService.AuthorizeAsync(User, TraducirPolicy.CanSuggest)).Succeeded,
                UserTypeIsTrustedUser = User.GetClaim<UserType>(ClaimType.UserType) == UserType.TrustedUser
            };

            return PartialView("EditString", viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
