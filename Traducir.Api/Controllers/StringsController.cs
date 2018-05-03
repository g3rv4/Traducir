using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Traducir.Api.Models.Enums;
using Traducir.Api.ViewModels.Strings;
using Traducir.Core.Helpers;
using Traducir.Core.Models;
using Traducir.Core.Models.Enums;
using Traducir.Core.Services;

namespace Traducir.Api.Controllers
{
    public class StringsController : Controller
    {
        private static readonly Regex VariablesRegex = new Regex(@"\$[^ \$]+\$", RegexOptions.Compiled);
        private static readonly Regex WhitespacesRegex = new Regex(@"^(?<start>\s*).*?(?<end>\s*)$", RegexOptions.Singleline | RegexOptions.Compiled);

        private readonly ISOStringService _soStringService;
        private readonly IAuthorizationService _authorizationService;

        public StringsController(
            ISOStringService soStringService,
            IAuthorizationService authorizationService)
        {
            _soStringService = soStringService;
            _authorizationService = authorizationService;
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

            void ComposePredicate(Func<SOString, bool> newPredicate)
            {
                if (predicate == null)
                {
                    predicate = newPredicate;
                    return;
                }

                var oldPredicate = predicate;
                predicate = s => oldPredicate(s) && newPredicate(s);
            }

            if (model.TranslationStatus != TranslationStatus.AnyStatus)
            {
                ComposePredicate(s => s.HasTranslation == (model.TranslationStatus == TranslationStatus.WithTranslation));
            }

            if (model.PushStatus != PushStatus.AnyStatus)
            {
                ComposePredicate(s => s.NeedsPush == (model.PushStatus == PushStatus.NeedsPush));
            }

            if (model.UrgencyStatus != UrgencyStatus.AnyStatus)
            {
                ComposePredicate(s => s.IsUrgent == (model.UrgencyStatus == UrgencyStatus.IsUrgent));
            }

            if (model.SuggestionsStatus != SuggestionApprovalStatus.AnyStatus)
            {
                switch (model.SuggestionsStatus)
                {
                    case SuggestionApprovalStatus.DoesNotHaveSuggestions:
                        ComposePredicate(s => !s.HasSuggestions);
                        break;
                    case SuggestionApprovalStatus.HasSuggestionsNeedingReview:
                        ComposePredicate(s =>
                            s.Suggestions != null &&
                            s.Suggestions.Any(sug => sug.State == StringSuggestionState.Created || sug.State == StringSuggestionState.ApprovedByTrustedUser));
                        break;
                    case SuggestionApprovalStatus.HasSuggestionsNeedingApproval:
                        ComposePredicate(s => s.HasSuggestionsWaitingApproval);
                        break;
                    case SuggestionApprovalStatus.HasSuggestionsNeedingReviewApprovedByTrustedUser:
                        ComposePredicate(s => s.HasApprovedSuggestionsWaitingReview);
                        break;
                }
            }

            if (model.Key.HasValue())
            {
                ComposePredicate(s => s.Key.StartsWith(model.Key, true, CultureInfo.InvariantCulture));
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

                ComposePredicate(s => regex.IsMatch(s.OriginalString));
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

                ComposePredicate(s => s.HasTranslation && regex.IsMatch(s.Translation));
            }

            var result = await _soStringService.GetStringsAsync(predicate);
            return Json(result.Take(2000));
        }

        [HttpGet]
        [Authorize]
        [Route("app/api/suggestions-by-user/{userId:INT}/{filterId:Int?}")]
        public async Task<IActionResult> GetSuggestionsByUser(int userId, int? filterId)
        {
            return Json(await _soStringService.GetSuggestionsByUser(userId, filterId));
        }

        [HttpPut]
        [Authorize(Policy = TraducirPolicy.CanSuggest)]
        [Route("app/api/suggestions")]
        public async Task<IActionResult> CreateSuggestion([FromBody] CreateSuggestionViewModel model)
        {
            // Verify that everything is valid before calling the service
            var str = await _soStringService.GetStringByIdAsync(model.StringId);

            // if the string id is invalid
            if (str == null)
            {
                return BadRequest(SuggestionCreationResult.InvalidStringId);
            }

            // empty suggestion
            if (model.Suggestion.IsNullOrEmpty())
            {
                return BadRequest(SuggestionCreationResult.EmptySuggestion);
            }

            var usingRawString = model.RawString &&
                (await _authorizationService.AuthorizeAsync(User, TraducirPolicy.CanReview)).Succeeded;

            // fix whitespaces unless user is reviewer and selected raw string
            if (!usingRawString)
            {
                model.Suggestion = FixWhitespaces(model.Suggestion, str.OriginalString);
            }

            // if the suggestion is the same as the current translation
            if (str.Translation == model.Suggestion)
            {
                return BadRequest(SuggestionCreationResult.SuggestionEqualsOriginal);
            }

            // if there's another suggestion with the same value
            if (str.Suggestions != null && str.Suggestions.Any(sug => sug.Suggestion == model.Suggestion))
            {
                return BadRequest(SuggestionCreationResult.SuggestionAlreadyThere);
            }

            // if there are missing or extra values
            var variablesInOriginal = VariablesRegex.Matches(str.OriginalString).Select(m => m.Value).ToArray();
            var variablesInSuggestion = VariablesRegex.Matches(model.Suggestion).Select(m => m.Value).ToArray();

            if (!usingRawString && variablesInOriginal.Any(v => !variablesInSuggestion.Contains(v)))
            {
                return BadRequest(SuggestionCreationResult.TooFewVariables);
            }

            if (variablesInSuggestion.Any(v => !variablesInOriginal.Contains(v)))
            {
                return BadRequest(SuggestionCreationResult.TooManyVariables);
            }

            var suggestionResult = await _soStringService.CreateSuggestionAsync(
                model.StringId,
                model.Suggestion,
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
                User.GetClaim<UserType>(ClaimType.UserType));
            if (success)
            {
                return new EmptyResult();
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
                return new EmptyResult();
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
                return new EmptyResult();
            }

            return BadRequest();
        }

        private static string FixWhitespaces(string suggestion, string original)
        {
            var match = WhitespacesRegex.Match(original);
            return match.Groups["start"] + suggestion.Trim() + match.Groups["end"];
        }
    }
}