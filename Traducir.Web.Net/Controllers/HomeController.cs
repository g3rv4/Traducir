using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Traducir.Api.Services;
using Traducir.Core.Services;
using Traducir.Web.Net.Models;
using Traducir.Web.Net.ViewModels.Home;

namespace Traducir.Web.Net.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStringsService stringsService;

        public HomeController(IStringsService stringsService)
        {
            this.stringsService = stringsService;
        }

        public async Task<IActionResult> Index()
        {
            return await Filters();
        }

        [Route("/filters")]
        public async Task<IActionResult> Filters()
        {
            var viewModel = new IndexViewModel
            {
                StringCounts = await stringsService.GetStringCounts()
            };

            return View("~/Views/Home/Index.cshtml", viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
