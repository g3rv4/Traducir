using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SimpleMigrations;
using SimpleMigrations.DatabaseProvider;
using StackExchange.Exceptional;
using Traducir.Core.Services;

namespace Traducir.Api.Controllers
{
    public class AdminController : Controller
    {
        private readonly ITransifexService _transifexService;
        private readonly ISOStringService _soStringService;
        private readonly IConfiguration _configuration;

        public AdminController(ITransifexService transifexService, ISOStringService soStringService, IConfiguration configuration)
        {
            _transifexService = transifexService;
            _soStringService = soStringService;
            _configuration = configuration;
        }

        [Route("app/admin/throw")]
        public static IActionResult Throw()
        {
            throw new InvalidOperationException();
        }

        [Route("app/api/admin/pull")]
        public async Task<IActionResult> PullStrings()
        {
            var strings = await _transifexService.GetStringsFromTransifexAsync().ConfigureAwait(false);
            await _soStringService.StoreNewStringsAsync(strings).ConfigureAwait(false);
            return new EmptyResult();
        }

        [Route("app/api/admin/pull-so-dump")]
        public async Task<IActionResult> PullSODump(string dumpUrl)
        {
            await _soStringService.PullSODump(dumpUrl).ConfigureAwait(false);
            return new EmptyResult();
        }

        [Route("app/api/admin/update-translations-fron-so-dump")]
        public async Task<IActionResult> UpdateTranslationsFromSODump()
        {
            await _soStringService.UpdateTranslationsFromSODump().ConfigureAwait(false);
            return new EmptyResult();
        }

        [Route("app/api/admin/push")]
        public async Task<IActionResult> PushStrings()
        {
            var stringsToPush = await _soStringService.GetStringsAsync(s => s.HasTranslation).ConfigureAwait(false);
            if (stringsToPush.Length > 0)
            {
                await _transifexService.PushStringsToTransifexAsync(stringsToPush).ConfigureAwait(false);
            }

            return new EmptyResult();
        }

        [Route("app/api/admin/migrate")]
        public IActionResult Migrate()
        {
            var migrationsAssembly = typeof(Migrations.Program).Assembly;
            using (var db = new SqlConnection(_configuration.GetValue<string>("CONNECTION_STRING")))
            {
                var databaseProvider = new MssqlDatabaseProvider(db);
                var migrator = new SimpleMigrator(migrationsAssembly, databaseProvider);

                migrator.Load();
                migrator.MigrateToLatest();
            }

            return new EmptyResult();
        }

        [Route("app/admin/errors/{path?}/{subPath?}")]
        public async Task Exceptions() => await ExceptionalMiddleware.HandleRequestAsync(HttpContext).ConfigureAwait(false);
    }
}