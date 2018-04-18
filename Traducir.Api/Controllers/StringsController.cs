using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Traducir.Core.Helpers;
using Traducir.Core.Models;
using Traducir.Core.Models.Enums;
using Traducir.Core.Services;
using Traducir.ViewModels.Strings;
using Traducir.Api.Models.Enums;

namespace Traducir.Controllers
{
    public class StringsController : Controller
    {
        private ISOStringService _soStringService { get; set; }

        public StringsController(ISOStringService soStringService)
        {
            _soStringService = soStringService;
        }

        [HttpGet]
        [Route("app/api/strings/stats")]
        public async Task<IActionResult> GetStringStats()
        {
            return Json(new
            {
                TotalStrings = (await _soStringService.GetStringsAsync()).Length,
                    WithoutTranslation = (await _soStringService.GetStringsAsync(s => !s.HasTranslation)).Length,
                    WithPendingSuggestions = (await _soStringService.GetStringsAsync(s => s.HasSuggestions)).Length,
                    WaitingApproval = (await _soStringService.GetStringsAsync(s => s.HasSuggestionsWaitingApproval)).Length,
                    WaitingReview = (await _soStringService.GetStringsAsync(s => s.HasApprovedSuggestionsWaitingReview)).Length,
                    UrgentStrings = (await _soStringService.GetStringsAsync(s => s.IsUrgent)).Length,
            });
        }

        [HttpGet]
        [Route("app/api/strings/{stringId:INT}")]
        public async Task<IActionResult> GetString(int stringId)
        {
            return Json(await _soStringService.GetStringByIdAsync(stringId));
        }

        [HttpPost]
        [Route("app/api/strings/query")]
        public async Task<IActionResult> Query([FromBody] QueryViewModel model)
        {
            Func<SOString, bool> predicate = null;

            void composePredicate(Func<SOString, bool> newPredicate)
            {
                if (predicate == null)
                {
                    predicate = newPredicate;
                    return;
                }
                var oldPredicate = predicate;
                predicate = s => oldPredicate(s)&& newPredicate(s);
            }

            if (model.TranslationStatus != QueryViewModel.TranslationStatuses.AnyStatus)
            {
                composePredicate(s => s.HasTranslation == (model.TranslationStatus == QueryViewModel.TranslationStatuses.WithTranslation));
            }
            if (model.PushStatus != QueryViewModel.PushStatuses.AnyStatus)
            {
                composePredicate(s => s.NeedsPush == (model.PushStatus == QueryViewModel.PushStatuses.NeedsPush));
            }
            if (model.UrgencyStatus != QueryViewModel.UrgencyStatuses.AnyStatus)
            {
                composePredicate(s => s.IsUrgent == (model.UrgencyStatus == QueryViewModel.UrgencyStatuses.IsUrgent));
            }
            if (model.SuggestionsStatus != QueryViewModel.SuggestionApprovalStatus.AnyStatus)
            {
                switch (model.SuggestionsStatus)
                {
                    case QueryViewModel.SuggestionApprovalStatus.DoesNotHaveSuggestions:
                        composePredicate(s => !s.HasSuggestions);
                        break;
                    case QueryViewModel.SuggestionApprovalStatus.HasSuggestionsNeedingReview:
                        composePredicate(s =>
                            s.Suggestions != null &&
                            s.Suggestions.Any(sug => sug.State == StringSuggestionState.Created || sug.State == StringSuggestionState.ApprovedByTrustedUser));
                        break;
                    case QueryViewModel.SuggestionApprovalStatus.HasSuggestionsNeedingApproval:
                        composePredicate(s => s.HasSuggestionsWaitingApproval);
                        break;
                    case QueryViewModel.SuggestionApprovalStatus.HasSuggestionsNeedingReviewApprovedByTrustedUser:
                        composePredicate(s => s.HasApprovedSuggestionsWaitingReview);
                        break;
                }
            }
            if (model.Key.HasValue())
            {
                composePredicate(s => s.Key.StartsWith(model.Key));
            }
            if (model.SourceRegex.HasValue())
            {
                Regex regex;
                try
                {
                    regex = new Regex(model.SourceRegex, RegexOptions.Compiled);
                }
                catch (ArgumentException)
                {
                    return BadRequest();
                }
                composePredicate(s => regex.IsMatch(s.OriginalString));
            }
            if (model.TranslationRegex.HasValue())
            {
                Regex regex;
                try
                {
                    regex = new Regex(model.TranslationRegex, RegexOptions.Compiled);
                }
                catch (ArgumentException)
                {
                    return BadRequest();
                }
                composePredicate(s => s.HasTranslation && regex.IsMatch(s.Translation));
            }

            var result = predicate != null ? await _soStringService.GetStringsAsync(predicate):
                await _soStringService.GetStringsAsync();

            return Json(result.Take(2000));
        }

        private Regex _variablesRegex = new Regex(@"\$[^ \$]+\$", RegexOptions.Compiled);

        [HttpPut]
        [Authorize(Policy = "CanSuggest")]
        [Route("app/api/suggestions")]
        public async Task<IActionResult> CreateSuggestion([FromBody] CreateSuggestionViewModel model)
        {
            //Verify that everything is valid before calling the service
            var str = await _soStringService.GetStringByIdAsync(model.StringId);
            
            // if the string id is invalid
            if (str == null)
            {
                return BadRequest(SuggestionCreationResult.InvalidStringId);
            }

            // if the suggestion is the same as the current translation
            if (str.Translation == model.Suggestion)
            {
                return BadRequest(SuggestionCreationResult.SuggestionEqualsOriginal);
            }

            // empty suggestion
            if (model.Suggestion == null || model.Suggestion.Length == 0)
            {
                return BadRequest(SuggestionCreationResult.EmptySuggestion);
            }

            // if there's another suggestion with the same value
            if (str.Suggestions != null && str.Suggestions.Any(sug => sug.Suggestion == model.Suggestion))
            {
                return BadRequest(SuggestionCreationResult.SuggestionAlreadyThere);
            }

            // if there are missing or extra values
            var variablesInOriginal = _variablesRegex.Matches(str.OriginalString).Cast<Match>().Select(m => m.Value).ToArray();
            var variablesInSuggestion = _variablesRegex.Matches(model.Suggestion).Cast<Match>().Select(m => m.Value).ToArray();
            if (variablesInOriginal.Any(v => !variablesInSuggestion.Contains(v)) ||
                variablesInSuggestion.Any(v => !variablesInOriginal.Contains(v)))
            {
                return BadRequest(SuggestionCreationResult.QuantityOfVariableValuesNotEqual);
            }

            var suggestionResult = await _soStringService.CreateSuggestionAsync(model.StringId, model.Suggestion,
                User.GetClaim<int>(ClaimType.Id),
                User.GetClaim<UserType>(ClaimType.UserType),
                model.Approve);
            if (suggestionResult)
            {
                return new EmptyResult();
            }
            return BadRequest(SuggestionCreationResult.DatabaseError);
        }

        [HttpPut]
        [Authorize(Policy = "CanReview")]
        [Route("app/api/review")]
        public async Task<IActionResult> Review([FromBody] ReviewViewModel model)
        {
            if (!model.SuggestionId.HasValue || !model.Approve.HasValue)
            {
                return BadRequest();
            }
            var success = await _soStringService.ReviewSuggestionAsync(model.SuggestionId.Value, model.Approve.Value,
                User.GetClaim<int>(ClaimType.Id),
                User.GetClaim<UserType>(ClaimType.UserType));
            if (success)
            {
                return new EmptyResult();
            }
            return BadRequest();
        }

        [HttpPut]
        [Authorize(Policy = "CanSuggest")]
        [Route("app/api/manage-urgency")]
        public async Task<IActionResult> ManageUrgency([FromBody] ManageUrgencyViewModel model)
        {
            var success = await _soStringService.ManageUrgencyAsync(model.StringId, model.IsUrgent,
                User.GetClaim<int>(ClaimType.Id));
            if (success)
            {
                return new EmptyResult();
            }
            return BadRequest();
        }

        [HttpDelete]
        [Route("app/api/delete-suggestion")]
        public async Task<IActionResult> DeleteSuggestion([FromQuery] DeleteSuggestionViewModel model)
        {
            //Have to validate everything before continuing
            var str = await _soStringService.GetStringByIdAsync(model.StringId);

            //validate that a string exists
            if (str == null)
            {
                return BadRequest();
            }
            //Get the suggestion and validate it and his user
            var suggestion = str.Suggestions.Single(x => x.Id == model.SuggestionId);
            if (suggestion == null)
            {
                return BadRequest();
            }
            if(suggestion.CreatedById != model.UserId)
            {
                return BadRequest();
            }
            //Validate that the user is logged and is the one that is asking this
            if (User.GetClaim<int>(ClaimType.Id) != model.UserId)
            {
                return BadRequest();
            }
            var success = await _soStringService.DeleteSuggestionAsync(suggestion);
            if (success)
            {
                return new EmptyResult();
            }
            return BadRequest();
        }

    }
}