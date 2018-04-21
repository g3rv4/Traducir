using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Traducir.Api.ViewModels.Config;

namespace Traducir.Api.Controllers
{
    public class ConfigController : Controller
    {
        private readonly IConfiguration _configuration;

        public ConfigController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Route("app/api/config")]
        public IActionResult GetConfig()
        {
            return Json(new ConfigViewModel
            {
                SiteDomain = _configuration.GetValue<string>("STACKAPP_SITEDOMAIN"),
                FriendlyName = _configuration.GetValue<string>("FRIENDLY_NAME"),
                TransifexPath = _configuration.GetValue<string>("TRANSIFEX_LINK_PATH")
            });
        }
    }
}