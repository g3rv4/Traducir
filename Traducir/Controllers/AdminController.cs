using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Traducir.Core.Services;

namespace Traducir.Controllers
{
    public class AdminController : Controller
    {
        private ITransifexService _transifexService { get; }

        public AdminController(ITransifexService transifexService)
        {
            _transifexService = transifexService;
        }

        [HttpGet]
        [Route("api/admin/updatedata")]
        public async Task<IActionResult> UpdateData()
        {
            await _transifexService.PullFromTransifex();
            return Content("Ok");
        }
    }
}