using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SimpleMigrations;
using SimpleMigrations.DatabaseProvider;
using StackExchange.Exceptional;
using Traducir.Core.Helpers;
using Traducir.Core.Services;

namespace Traducir.Controllers
{
    public class AdminController : Controller
    {
        private ITransifexService _transifexService { get; }
        private ISOStringService _soStringService { get; }
        private IConfiguration _configuration { get; }

        public AdminController(ITransifexService transifexService, ISOStringService soStringService, IConfiguration configuration)
        {
            _transifexService = transifexService;
            _soStringService = soStringService;
            _configuration = configuration;
        }

        [Route("app/api/admin/pull")]
        public async Task<IActionResult> PullStrings()
        {
            var strings = await _transifexService.GetStringsFromTransifexAsync();
            await _soStringService.StoreNewStringsAsync(strings);
            return new EmptyResult();
        }

        [Route("app/api/admin/pull-so-dump")]
        public async Task<IActionResult> PullSODump(string dumpUrl)
        {
            await _soStringService.PullSODump(dumpUrl);
            return new EmptyResult();
        }

        [Route("app/api/admin/update-translations-fron-so-dump")]
        public async Task<IActionResult> UpdateTranslationsFromSODump()
        {
            await _soStringService.UpdateTranslationsFromSODump();
            return new EmptyResult();
        }

        [Route("app/api/admin/push")]
        public async Task<IActionResult> PushStrings()
        {
            var stringsToPush = await _soStringService.GetStringsAsync(s => s.HasTranslation);
            if (stringsToPush.Length > 0)
            {
                await _transifexService.PushStringsToTransifexAsync(stringsToPush);
            }
            return new EmptyResult();
        }

        [Route("app/api/admin/migrate")]
        public IActionResult Migrate()
        {
            var migrationsAssembly = typeof(Traducir.Migrations.Program).Assembly;
            using (var db = new SqlConnection(_configuration.GetValue<string>("CONNECTION_STRING")))
            {
                var databaseProvider = new MssqlDatabaseProvider(db);
                var migrator = new SimpleMigrator(migrationsAssembly, databaseProvider);

                migrator.Load();
                migrator.MigrateToLatest();
            }
            return new EmptyResult();
        }

        [Route("app/admin/throw")]
        public IActionResult Throw()
        {
            throw new InvalidOperationException();
        }

        [Route("app/admin/errors/{path?}/{subPath?}")]
        public async Task Exceptions() => await ExceptionalMiddleware.HandleRequestAsync(HttpContext);
    }
}