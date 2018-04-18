using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StackExchange.Profiling;
using Traducir.Core.Models;
using Traducir.Core.Models.Services;

namespace Traducir.Core.Services
{
    public interface ITransifexService
    {
        Task<ImmutableArray<TransifexString>> GetStringsFromTransifexAsync();

        Task<bool> PushStringsToTransifexAsync(ImmutableArray<SOString> strings);
    }

    public class TransifexService : ITransifexService
    {
        private string _apikey { get; }
        private string _resourcePath { get; }
        private ISOStringService _soStringService { get; }

        public TransifexService(IConfiguration configuration, ISOStringService soStringService)
        {
            _apikey = configuration.GetValue<string>("TRANSIFEX_APIKEY");
            _resourcePath = configuration.GetValue<string>("TRANSIFEX_RESOURCE_PATH");
            _soStringService = soStringService;
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

        public async Task<ImmutableArray<TransifexString>> GetStringsFromTransifexAsync()
        {
            using (MiniProfiler.Current.Step("Fetching strings from Transifex"))
            {
                var client = GetHttpClient();
                var response = await client.GetAsync(_resourcePath);
                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(stream))
                {
                    return Jil.JSON.Deserialize<TransifexString[]>(reader).ToImmutableArray();
                }
            }
        }

        public async Task<bool> PushStringsToTransifexAsync(ImmutableArray<SOString> strings)
        {
            using (MiniProfiler.Current.Step("Pushing strings to Transifex"))
            {
                ByteArrayContent byteContent;
                using (MiniProfiler.Current.Step("Serializing the payload"))
                {
                    var content = Jil.JSON.Serialize(strings.Select(s => new TransifexStringToPush
                    {
                        Key = s.Key,
                        Translation = s.Translation
                    }));

                    var buffer = System.Text.Encoding.UTF8.GetBytes(content);
                    byteContent = new ByteArrayContent(buffer);
                    byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                }

                bool success;
                using (MiniProfiler.Current.Step("Posting to Transifex"))
                {
                    var client = GetHttpClient();
                    var response = await client.PutAsync(_resourcePath, byteContent);
                    success = response.IsSuccessStatusCode;
                }

                if (success)
                {
                    await _soStringService.UpdateStringsPushed();
                }

                return success;
            }
        }
    }
}