using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Traducir.Core.Models.Services;
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

        [HttpGet]
        [Route("api/admin/updatedata")]
        public async Task<IActionResult> UpdateData()
        {
            TransifexString[] strings;
            if (Request.Query.ContainsKey("empty"))
            {
                strings = Array.Empty<TransifexString>();
            }
            else
            {
                strings = await _transifexService.GetStringsFromTransifexAsync();
            }
            await _soStringService.StoreNewStrings(strings);
            return View("Ok");
        }
    }
}