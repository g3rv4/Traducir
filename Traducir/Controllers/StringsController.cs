using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Traducir.Core.Helpers;
using Traducir.Core.Models;
using Traducir.Core.Models.Enums;
using Traducir.Core.Services;
using Traducir.ViewModels.Strings;

namespace Traducir.Controllers
{
    public class StringsController : Controller
    {
        private ISOStringService _soStringService { get; set; }

        public StringsController(ISOStringService soStringService)
        {
            _soStringService = soStringService;
        }

        [HttpPost]
        [Route("app/api/strings/query")]
        public async Task<IActionResult> Query([FromBody] QueryViewModel model)
        {
            Func<SOString, bool> predicate = s => true;
            if (model.WithoutTranslation.HasValue)
            {
                predicate = s => s.Translation.IsNullOrEmpty()== model.WithoutTranslation.Value;
            }
            if (model.WithSuggestionsNeedingApproval.HasValue)
            {
                var oldPredicate = predicate;

                predicate = s => oldPredicate(s)&&
                    s.Suggestions != null &&
                    s.Suggestions.Any(sug => sug.State == StringSuggestionState.Created)== model.WithSuggestionsNeedingApproval.Value;
            }
            if (model.SourceRegex.HasValue())
            {
                var oldPredicate = predicate;

                var regex = new Regex(model.SourceRegex, RegexOptions.Compiled);
                predicate = s => oldPredicate(s)&& regex.IsMatch(s.OriginalString);
            }
            if (model.TranslationRegex.HasValue())
            {
                var oldPredicate = predicate;

                var regex = new Regex(model.TranslationRegex, RegexOptions.Compiled);
                predicate = s => oldPredicate(s)&& s.Translation.HasValue()&& regex.IsMatch(s.Translation);
            }

            return Json(await _soStringService.GetStringAsync(predicate));
        }

        [HttpPut]
        [Route("app/api/suggestions")]
        public async Task<IActionResult> CreateSuggestion([FromBody] CreateSuggestionViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Forbid();
            }

            var userId = User.Claims.Where(c => c.Type == "UserId").Select(c => int.Parse(c.Value)).First();

            var success = await _soStringService.CreateSuggestionAsync(model.StringId, model.Suggestion, userId);
            if(success){
                await _soStringService.RefreshCacheAsync();
                return new EmptyResult();
            }
            return BadRequest();
        }
    }
}