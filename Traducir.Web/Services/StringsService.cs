using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Traducir.Web.ViewModels.Home;
using Traducir.Core.Helpers;
using Traducir.Core.Models;
using Traducir.Core.Models.Enums;
using Traducir.Core.Services;
using Traducir.Web.Models.Enums;
using Traducir.Web.Models.Home;

namespace Traducir.Web.Services
{
    public interface IStringsService
    {
        Task<ImmutableArray<SOString>> Query(QueryViewModel model);

        Task<StringCountsViewModel> GetStringCounts();

        Task<SuggestionCreationResult> CreateSuggestion(CreateSuggestionViewModel model);
    }

    public class StringsService : IStringsService
    {
        private static readonly Regex VariablesRegex = new Regex(@"\$[^ \$]+\$", RegexOptions.Compiled);
        private static readonly Regex WhitespacesRegex = new Regex(@"^(?<start>\s*).*?(?<end>\s*)$", RegexOptions.Singleline | RegexOptions.Compiled);

        private readonly ISOStringService _soStringService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StringsService(
            ISOStringService soStringService,
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor)
        {
            _soStringService = soStringService;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ImmutableArray<SOString>> Query(QueryViewModel model)
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

            if (model.IgnoredStatus != IgnoredStatus.IncludeIgnored)
            {
                ComposePredicate(s => s.IsIgnored == (model.IgnoredStatus == IgnoredStatus.OnlyIgnored));
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
                catch (ArgumentException ex)
                {
                    throw new InvalidOperationException("Invalid source regex", ex);
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
                catch (ArgumentException ex)
                {
                    throw new InvalidOperationException("Invalid translation regex", ex);
                }

                ComposePredicate(s => s.HasTranslation && regex.IsMatch(s.Translation));
            }

            return await _soStringService.GetStringsAsync(predicate);
        }

        public async Task<StringCountsViewModel> GetStringCounts()
        {
            return new StringCountsViewModel
            {
                TotalStrings = await _soStringService.CountStringsAsync(s => !s.IsIgnored),
                WithoutTranslation = await _soStringService.CountStringsAsync(s => !s.HasTranslation && !s.IsIgnored),
                WithPendingSuggestions = await _soStringService.CountStringsAsync(s => s.HasSuggestions && !s.IsIgnored),
                WaitingApproval = await _soStringService.CountStringsAsync(s => s.HasSuggestionsWaitingApproval && !s.IsIgnored),
                WaitingReview = await _soStringService.CountStringsAsync(s => s.HasApprovedSuggestionsWaitingReview && !s.IsIgnored),
                UrgentStrings = await _soStringService.CountStringsAsync(s => s.IsUrgent && !s.IsIgnored),
            };
        }

        public async Task<SuggestionCreationResult> CreateSuggestion(CreateSuggestionViewModel model)
        {
            var user = _httpContextAccessor.HttpContext.User;

            // Verify that everything is valid before calling the service
            var str = await _soStringService.GetStringByIdAsync(model.StringId);

            // if the string id is invalid
            if (str == null)
            {
                return SuggestionCreationResult.InvalidStringId;
            }

            // empty suggestion
            if (model.Suggestion.IsNullOrEmpty())
            {
                return SuggestionCreationResult.EmptySuggestion;
            }

            var usingRawString = model.RawString &&
                (await _authorizationService.AuthorizeAsync(user, TraducirPolicy.CanReview)).Succeeded;

            // fix whitespaces unless user is reviewer and selected raw string
            if (!usingRawString)
            {
                model.Suggestion = FixWhitespaces(model.Suggestion, str.OriginalString);
            }

            // if the suggestion is the same as the current translation
            if (str.Translation == model.Suggestion)
            {
                return SuggestionCreationResult.SuggestionEqualsOriginal;
            }

            // if there's another suggestion with the same value
            if (str.Suggestions != null && str.Suggestions.Any(sug => sug.Suggestion == model.Suggestion))
            {
                return SuggestionCreationResult.SuggestionAlreadyThere;
            }

            // if there are missing or extra values
            var variablesInOriginal = VariablesRegex.Matches(str.OriginalString).Select(m => m.Value).ToArray();
            var variablesInSuggestion = VariablesRegex.Matches(model.Suggestion).Select(m => m.Value).ToArray();

            if (!usingRawString && variablesInOriginal.Any(v => !variablesInSuggestion.Contains(v)))
            {
                return SuggestionCreationResult.TooFewVariables;
            }

            if (variablesInSuggestion.Any(v => !variablesInOriginal.Contains(v)))
            {
                return SuggestionCreationResult.TooManyVariables;
            }

            var suggestionResult = await _soStringService.CreateSuggestionAsync(
                model.StringId,
                model.Suggestion,
                user.GetClaim<int>(ClaimType.Id),
                user.GetClaim<UserType>(ClaimType.UserType),
                model.Approve);

            return suggestionResult ? SuggestionCreationResult.CreationOk : SuggestionCreationResult.DatabaseError;
        }

        private static string FixWhitespaces(string suggestion, string original)
        {
            var match = WhitespacesRegex.Match(original);
            return match.Groups["start"] + suggestion.Trim() + match.Groups["end"];
        }
    }
}
