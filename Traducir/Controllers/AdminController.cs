using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Exceptional;
using Traducir.Core.Helpers;
using Traducir.Core.Services;

namespace Traducir.Controllers
{
    public class AdminController : Controller
    {
        private ITransifexService _transifexService { get; }
        private ISOStringService _soStringService { get; }

        public AdminController(ITransifexService transifexService, ISOStringService soStringService)
        {
            _transifexService = transifexService;
            _soStringService = soStringService;
        }

        [Route("app/api/admin/pull")]
        public async Task<IActionResult> PullStrings()
        {
            var strings = await _transifexService.GetStringsFromTransifexAsync();
            await _soStringService.StoreNewStringsAsync(strings);
            return View("Ok");
        }

        [Route("app/api/admin/push")]
        public async Task<IActionResult> PushStrings()
        {
            var stringsToPush = await _soStringService.GetStringsAsync(s => s.Translation.HasValue());
            await _transifexService.PushStringsToTransifexAsync(stringsToPush);
            return View("Ok");
        }

        [Route("app/admin/errors/{path?}/{subPath?}")]
        public async Task Exceptions()=> await ExceptionalMiddleware.HandleRequestAsync(HttpContext);
    }
}