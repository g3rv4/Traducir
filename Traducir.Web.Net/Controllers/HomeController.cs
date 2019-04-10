using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Traducir.Api.Services;
using Traducir.Core.Helpers;
using Traducir.Web.Net.Models;
using Traducir.Web.Net.ViewModels.Home;
using Traducir.Api.ViewModels.Strings;
using Traducir.Core.Models.Enums;
using System;
using Traducir.Core.Models;
using System.Collections.Generic;

namespace Traducir.Web.Net.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStringsService stringsService;

        public HomeController(IStringsService stringsService)
        {
            this.stringsService = stringsService;
        }

        public Task<IActionResult> Index()
        {
            return Filters(null);
        }

        [Route("/filters")]
        public async Task<IActionResult> Filters(QueryViewModel query)
        {
            FilterResultsViewModel filterResults = null;

            if(query == null)
            {
                query = new QueryViewModel();
            }
            else if(query.IsEmpty)
            {
                //let's redirect to root if we get just '/filters'
                return RedirectToAction(nameof(Index));
            }
            else
            {
                filterResults = new FilterResultsViewModel
                {
                    UserType = User.GetClaim<UserType>(ClaimType.UserType),
                    Strings = await stringsService.Query(query)
                };
            }

            var viewModel = new IndexViewModel
            {
                StringCounts = await stringsService.GetStringCounts(),
                StringsQuery = query,
                FilterResults = filterResults
            };

            return View("~/Views/Home/Index.cshtml", viewModel);
        }

        [Route("/strings_list")]
        public async Task<IActionResult> StringsList(QueryViewModel query)
        {
            IEnumerable<SOString> strings;

            try
            {
                strings = await stringsService.Query(query);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            var filterResults = new FilterResultsViewModel
            {
                UserType = User.GetClaim<UserType>(ClaimType.UserType),
                Strings = strings
            };

            return PartialView("FilterResults", filterResults);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
