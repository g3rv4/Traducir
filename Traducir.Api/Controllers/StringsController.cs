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
        private Regex _variablesRegex = new Regex(@"\$[^ \$]+\$", RegexOptions.Compiled);

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
                TotalStrings = (await _soStringService.GetStringsAsync(s => true)).Length,
                    WithoutTranslation = (await _soStringService.GetStringsAsync(s => !s.Translation.HasValue())).Length,
                    WithPendingSuggestions = (await _soStringService.GetStringsAsync(s => s.Suggestions != null && s.Suggestions.Any())).Length,
                    WaitingApproval = (await _soStringService.GetStringsAsync(s =>
                        s.Suggestions != null &&
                        s.Suggestions.Any(sug => sug.State == StringSuggestionState.Created)
                    )).Length,
                    WaitingReview = (await _soStringService.GetStringsAsync(s =>
                        s.Suggestions != null &&
                        s.Suggestions.Any(sug => sug.State == StringSuggestionState.ApprovedByTrustedUser)
                    )).Length,
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
            Func<SOString, bool> predicate = s => true;

            void composePredicate(Func<SOString, bool> newPredicate)
            {
                var oldPredicate = predicate;
                predicate = s => oldPredicate(s)&& newPredicate(s);
            }

            if (model.TranslationStatus != QueryViewModel.TranslationStatuses.AnyStatus)
            {
                predicate = s => s.Translation.IsNullOrEmpty()== (model.TranslationStatus == QueryViewModel.TranslationStatuses.WithoutTranslation);
            }
            if (model.PushStatus != QueryViewModel.PushStatuses.AnyStatus)
            {
                composePredicate(s => s.NeedsPush == (model.PushStatus == QueryViewModel.PushStatuses.NeedsPush));
            }
            if (model.SuggestionsStatus != QueryViewModel.SuggestionApprovalStatus.AnyStatus)
            {
                switch (model.SuggestionsStatus)
                {
                    case QueryViewModel.SuggestionApprovalStatus.DoesNotHaveSuggestions:
                        composePredicate(s =>
                            s.Suggestions == null ||
                            s.Suggestions.Length == 0);
                        break;
                    case QueryViewModel.SuggestionApprovalStatus.HasSuggestionsNeedingReview:
                        composePredicate(s =>
                            s.Suggestions != null &&
                            s.Suggestions.Any(sug => sug.State == StringSuggestionState.Created || sug.State == StringSuggestionState.ApprovedByTrustedUser));
                        break;
                    case QueryViewModel.SuggestionApprovalStatus.HasSuggestionsNeedingApproval:
                        composePredicate(s =>
                            s.Suggestions != null &&
                            s.Suggestions.Any(sug => sug.State == StringSuggestionState.Created));
                        break;
                    case QueryViewModel.SuggestionApprovalStatus.HasSuggestionsNeedingReviewApprovedByTrustedUser:
                        composePredicate(s =>
                            s.Suggestions != null &&
                            s.Suggestions.Any(sug => sug.State == StringSuggestionState.ApprovedByTrustedUser));
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
                composePredicate(s => s.Translation.HasValue()&& regex.IsMatch(s.Translation));
            }

            return Json((await _soStringService.GetStringsAsync(predicate)).Take(2000));
        }

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
    }
}