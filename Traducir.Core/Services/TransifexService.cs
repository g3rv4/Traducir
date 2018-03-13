using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Traducir.Core.Models.Services;

namespace Traducir.Core.Services
{
    public interface ITransifexService
    {
        Task PullFromTransifex();
    }

    public class TransifexService : ITransifexService
    {
        private static IDbService _dbService { get; set; }
        private string _apikey { get; }
        private string _resourcePath { get; }

        public TransifexService(IConfiguration configuration, IDbService dbService)
        {
            _apikey = configuration.GetValue<string>("TRANSIFEX_APIKEY");
            _resourcePath = configuration.GetValue<string>("TRANSIFEX_RESOURCE_PATH");
            _dbService = dbService;
        }

        private static HttpClient _HttpClient { get; set; }
        private HttpClient GetHttpClient()
        {
            if (_HttpClient == null)
            {
                var baseAddress = new Uri("https://www.transifex.com");
                _HttpClient = new HttpClient() { BaseAddress = baseAddress };
                var byteArray = Encoding.ASCII.GetBytes("api:" + _apikey);
                _HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }

            return _HttpClient;
        }

        public async Task PullFromTransifex()
        {
            var client = GetHttpClient();
            var response = await client.GetAsync(_resourcePath);
            response.EnsureSuccessStatusCode();

            using(var stream = await response.Content.ReadAsStreamAsync())
            using(var reader = new StreamReader(stream))
            {
                var r = Jil.JSON.Deserialize<TransifexString[]>(reader);
            }

            throw new System.NotImplementedException();
        }
    }
}