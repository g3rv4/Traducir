using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Traducir.Api.Models.Enums;
using Traducir.Api.Services;
using Traducir.Api.ViewModels.Strings;
using Traducir.Core.Helpers;
using Traducir.Core.Models.Enums;
using Traducir.Core.Services;

namespace Traducir.Api.Controllers
{
    public class StringsController : Controller
    {
        private readonly ISOStringService _soStringService;
        private readonly IStringsService _stringsService;

        public StringsController(
            ISOStringService soStringService,
            IStringsService stringsService)
        {
            _soStringService = soStringService;
            _stringsService = stringsService;
        }

        [HttpGet]
        [Route("app/api/strings/stats")]
        public async Task<IActionResult> GetStringStats()
        {
            return Json(await _stringsService.GetStringCounts());
        }

        [HttpGet]
        [Route("app/api/strings/{stringId:INT}")]
        public async Task<IActionResult> GetString(int stringId)
        {
            return Json(await _soStringService.GetStringByIdAsync(stringId));
        }

        [HttpGet]
        [Route("app/api/strings/by-key/{key}")]
        public async Task<IActionResult> GetStringsByKey(string key)
        {
            return Json(await _soStringService.GetStringsAsync(s => s.FamilyKey == key));
        }

        [HttpPost]
        [Route("app/api/strings/query")]
        public async Task<IActionResult> Query([FromBody] QueryViewModel model)
        {
            try
            {
                var result = await _stringsService.Query(model);
                return Json(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("app/api/suggestions-by-user/{userId:INT}")]
        public async Task<IActionResult> GetSuggestionsByUser(int userId, StringSuggestionState? state) =>
            Json(await _soStringService.GetSuggestionsByUser(userId, state));

        [HttpPut]
        [Authorize(Policy = TraducirPolicy.CanSuggest)]
        [Route("app/api/suggestions")]
        public async Task<IActionResult> CreateSuggestion([FromBody] CreateSuggestionViewModel model)
        {
            var result = await _stringsService.CreateSuggestion(model);
            if (result == SuggestionCreationResult.CreationOk)
            {
                return NoContent();
            }

            return BadRequest(result);
        }

        [HttpPut]
        [Authorize(Policy = TraducirPolicy.CanReview)]
        [Route("app/api/review")]
        public async Task<IActionResult> Review([FromBody] ReviewViewModel model)
        {
            if (!model.SuggestionId.HasValue || !model.Approve.HasValue)
            {
                return BadRequest();
            }

            var success = await _soStringService.ReviewSuggestionAsync(
                model.SuggestionId.Value,
                model.Approve.Value,
                User.GetClaim<int>(ClaimType.Id),
                User.GetClaim<UserType>(ClaimType.UserType),
                Request.Host.ToString());
            if (success)
            {
                return NoContent();
            }

            return BadRequest();
        }

        [HttpPut]
        [Authorize(Policy = TraducirPolicy.CanSuggest)]
        [Route("app/api/manage-urgency")]
        public async Task<IActionResult> ManageUrgency([FromBody] ManageUrgencyViewModel model)
        {
            var success = await _soStringService.ManageUrgencyAsync(
                model.StringId,
                model.IsUrgent,
                User.GetClaim<int>(ClaimType.Id));
            if (success)
            {
                return NoContent();
            }

            return BadRequest();
        }

        [HttpPut]
        [Authorize(Policy = TraducirPolicy.CanReview)]
        [Route("app/api/manage-ignore")]
        public async Task<IActionResult> ManageIgnore([FromBody] ManageIgnoreViewModel model)
        {
            var success = await _soStringService.ManageIgnoreAsync(
                model.StringId,
                model.Ignored,
                User.GetClaim<int>(ClaimType.Id),
                User.GetClaim<UserType>(ClaimType.UserType));
            if (success)
            {
                return NoContent();
            }

            return BadRequest();
        }

        [HttpDelete]
        [Authorize(Policy = TraducirPolicy.CanSuggest)]
        [Route("app/api/suggestions/{suggestionId:INT}")]
        public async Task<IActionResult> DeleteSuggestion([FromRoute] int suggestionId)
        {
            var success = await _soStringService.DeleteSuggestionAsync(suggestionId, User.GetClaim<int>(ClaimType.Id));
            if (success)
            {
                return NoContent();
            }

            return BadRequest();
        }

        [HttpPut]
        [Authorize(Policy = TraducirPolicy.CanSuggest)]
        [Route("app/api/suggestions/replace")]
        public async Task<IActionResult> ReplaceSuggestion([FromBody] ReplaceSuggestionViewModel replaceSuggestionViewModel)
        {
            var success = await _soStringService.ReplaceSuggestionAsync(
                replaceSuggestionViewModel.SuggestionId,
                replaceSuggestionViewModel.NewSuggestion,
                User.GetClaim<int>(ClaimType.Id));
            if (success)
            {
                return NoContent();
            }

            return BadRequest();
        }
    }
}