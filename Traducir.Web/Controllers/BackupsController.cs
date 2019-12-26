using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Traducir.Web.ViewModels.Backups;

namespace Traducir.Web.Controllers
{
    public class BackupsController : Controller
    {
        private readonly IConfiguration _configuration;

        public BackupsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Route("backups")]
        public async Task<IActionResult> Index()
        {
            var account = _configuration.GetValue<string>("AZURE_BACKUP_ACCOUNT");
            var container = _configuration.GetValue<string>("AZURE_BACKUP_CONTAINER");
            var suffix = _configuration.GetValue<string>("AZURE_BACKUP_SUFFIX");

            var files = new List<Backup>();
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(new Uri($"https://{account}.blob.core.windows.net/{container}?restype=container&comp=list"));
                response.EnsureSuccessStatusCode();

                var dataStr = await response.Content.ReadAsStringAsync();
                var doc = new XmlDocument();
                doc.LoadXml(dataStr);

                foreach (XmlNode node in doc.SelectNodes("//Name"))
                {
                    if (node.InnerText.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    {
                        files.Add(new Backup
                        {
                            Filename = node.InnerText,
                            Url = $"https://{account}.blob.core.windows.net/{container}/{node.InnerText}",
                        });
                    }
                }

                files = files.OrderByDescending(f => f.Filename).ToList();
            }

            return View(files);
        }
    }
}