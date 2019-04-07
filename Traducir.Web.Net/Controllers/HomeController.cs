using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Traducir.Core.Services;
using Traducir.Web.Net.Models;
using Traducir.Web.Net.ViewModels.Home;

namespace Traducir.Web.Net.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISOStringService soStringService;

        public HomeController(ISOStringService soStringService)
        {
            this.soStringService = soStringService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new IndexViewModel
            {
                TotalStringsCount = await soStringService.CountStringsAsync(s => !s.IsIgnored),
                UrgentStringsCount = await soStringService.CountStringsAsync(s => s.IsUrgent && !s.IsIgnored),
                UntranslatedStringsCount = await soStringService.CountStringsAsync(s => !s.HasTranslation && !s.IsIgnored),
                SugestionsAwaitingApprovalCount = await soStringService.CountStringsAsync(s => s.HasSuggestionsWaitingApproval && !s.IsIgnored),
                ApprovedSugestionsAwaitingReviewCount = await soStringService.CountStringsAsync(s => s.HasApprovedSuggestionsWaitingReview && !s.IsIgnored)
            };

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
