using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Traducir.Core.Services;
using Traducir.ViewModels.Strings;

namespace Traducir.Controllers
{
    public class StringsController : Controller
    {
        private ISOStringService _soStringService{get;set;}

        public StringsController(ISOStringService soStringService)
        {
            _soStringService=soStringService;
        }

        [Route("app/api/query")]
        public async Task<IActionResult> Query()//[FromBody] QueryViewModel model)
        {
            await _soStringService.RefreshCache();
            return View("Ok");
        }
    }
}