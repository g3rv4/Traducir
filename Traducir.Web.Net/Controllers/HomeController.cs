using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
using Traducir.Api.Models.Enums;

namespace Traducir.Web.Net.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStringsService _stringsService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISOStringService _soStringsService;
        private readonly IConfiguration _configuration;

        public HomeController(
            IStringsService stringsService,
            IAuthorizationService authorizationService,
            ISOStringService soStringsService,
            IConfiguration configuration)
        {
            _stringsService = stringsService;
            _authorizationService = authorizationService;
            _soStringsService = soStringsService;
            _configuration = configuration;
        }

        public Task<IActionResult> Index() => Filters(null);

        [Route("/filters")]
        public async Task<IActionResult> Filters(QueryViewModel query)
        {
            FilterResultsViewModel filterResults = null;

            if (query == null)
            {
                query = new QueryViewModel();
            }
            else if (query.IsEmpty)
            {
                //let's redirect to root if we get just '/filters'
                return RedirectToAction(nameof(Index));
            }
            else
            {
                filterResults = await GetFilterResultsViewModelFor(query);
                if (filterResults == null)
                {
                    return BadRequest();
                }
            }

            var viewModel = new IndexViewModel
            {
                StringCounts = await _stringsService.GetStringCounts(),
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
            if (User.GetClaim<UserType>(ClaimType.UserType) < UserType.TrustedUser)
            {
                query.IgnoredStatus = IgnoredStatus.AvoidIgnored;
                query.PushStatus = PushStatus.AnyStatus;
            }

            IEnumerable<SOString> strings;

            try
            {
                strings = await _stringsService.Query(query);
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            return new FilterResultsViewModel(
                strings,
                userCanManageIgnoring: (await _authorizationService.AuthorizeAsync(User, TraducirPolicy.CanReview)).Succeeded);
        }

        [HttpPost]
        [Authorize(Policy = TraducirPolicy.CanReview)]
        [Route("/manage-ignore")]
        public async Task<IActionResult> ManageIgnore([FromBody] ManageIgnoreViewModel model)
        {
            var success = await _soStringsService.ManageIgnoreAsync(
                model.StringId,
                model.Ignored,
                User.GetClaim<int>(ClaimType.Id),
                User.GetClaim<UserType>(ClaimType.UserType));

            if (!success)
            {
                return BadRequest();
            }

            return await GetStringSummaryViewModelFor(model.StringId);
        }

        [HttpPost]
        [Authorize(Policy = TraducirPolicy.CanSuggest)]
        [Route("/manage-urgency")]
        public async Task<IActionResult> ManageUrgency([FromBody] ManageUrgencyViewModel model)
        {
            var success = await _soStringsService.ManageUrgencyAsync(
                model.StringId,
                model.IsUrgent,
                User.GetClaim<int>(ClaimType.Id));

            if (!success)
            {
                return BadRequest();
            }

            return await GetStringSummaryViewModelFor(model.StringId);
        }

        [HttpPost]
        [Authorize(Policy = TraducirPolicy.CanSuggest)]
        [Route("/replace-suggestion")]
        public async Task<IActionResult> ReplaceSuggestion([FromBody] ReplaceSuggestionViewModel model)
        {
            var success = await _soStringsService.ReplaceSuggestionAsync(
                model.SuggestionId,
                model.NewSuggestion,
                User.GetClaim<int>(ClaimType.Id));

            if (!success)
            {
                return BadRequest();
            }

            var stringId = await _soStringsService.GetStringIdBySuggestionId(model.SuggestionId);
            return await GetStringSummaryViewModelFor(stringId);
        }

        [HttpPost]
        [Authorize(Policy = TraducirPolicy.CanSuggest)]
        [Route("/delete-suggestion")]
        public async Task<IActionResult> DeleteSuggestion([FromBody] int suggestionId)
        {
            var success = await _soStringsService.DeleteSuggestionAsync(suggestionId, User.GetClaim<int>(ClaimType.Id));
            if (!success)
            {
                return BadRequest();
            }

            var stringId = await _soStringsService.GetStringIdBySuggestionId(suggestionId);
            return await GetStringSummaryViewModelFor(stringId);
        }

        [HttpPost]
        [Authorize(Policy = TraducirPolicy.CanReview)]
        [Route("/review-suggestion")]
        public async Task<IActionResult> Review([FromBody] ReviewViewModel model)
        {
            if (!model.SuggestionId.HasValue || !model.Approve.HasValue)
            {
                return BadRequest();
            }

            var success = await _soStringsService.ReviewSuggestionAsync(
                model.SuggestionId.Value,
                model.Approve.Value,
                User.GetClaim<int>(ClaimType.Id),
                User.GetClaim<UserType>(ClaimType.UserType),
                Request.Host.ToString());
            if (!success)
            {
                return BadRequest();
            }

            var stringId = await _soStringsService.GetStringIdBySuggestionId(model.SuggestionId.Value);
            return await GetStringSummaryViewModelFor(stringId);
        }

        [HttpPost]
        [Authorize(Policy = TraducirPolicy.CanSuggest)]
        [Route("/create-suggestion")]
        public async Task<IActionResult> CreateSuggestion([FromBody] CreateSuggestionViewModel model)
        {
            var result = await _stringsService.CreateSuggestion(model);
            if (result != SuggestionCreationResult.CreationOk)
            {
                return BadRequest(result);
            }

            return await GetStringSummaryViewModelFor(model.StringId);
        }
        private async Task<PartialViewResult> GetStringSummaryViewModelFor(int stringId, bool asChanged = true)
        {
            var str = await _soStringsService.GetStringByIdAsync(stringId);
            var userCanManageIgnoring = (await _authorizationService.AuthorizeAsync(User, TraducirPolicy.CanReview)).Succeeded;
            var summaryViewModel = new StringSummaryViewModel { String = str, RenderAsChanged = asChanged, UserCanManageIgnoring = userCanManageIgnoring };
            return PartialView("StringSummary", summaryViewModel);
        }

        [Route("/string_edit_ui")]
        public async Task<IActionResult> GetStringEditUi(int stringId)
        {
            var str = await _soStringsService.GetStringByIdAsync(stringId);
            if (str == null)
            {
                return NotFound();
            }

            var viewModel = new EditStringViewModel
            {
                SiteDomain = _configuration.GetValue<string>("STACKAPP_SITEDOMAIN"),
                TransifexPath = _configuration.GetValue<string>("TRANSIFEX_LINK_PATH"),
                String = str,
                UserIsLoggedIn = User.GetClaim<string>(ClaimType.Name) != null,
                UserId = User.GetClaim<int>(ClaimType.Id),
                UserCanReview = (await _authorizationService.AuthorizeAsync(User, TraducirPolicy.CanReview)).Succeeded,
                UserCanSuggest = (await _authorizationService.AuthorizeAsync(User, TraducirPolicy.CanSuggest)).Succeeded,
                UserType = User.GetClaim<UserType>(ClaimType.UserType)
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
