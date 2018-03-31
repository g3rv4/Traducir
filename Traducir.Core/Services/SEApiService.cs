using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Configuration;
using Traducir.Core.Models.Services;

namespace Traducir.Core.Services
{
    public interface ISEApiService
    {
        string GetInitialOauthUrl(string returnUrl, string state = null);

        Task<string> GetAccessTokenFromCodeAsync(string code, string returnUrl);

        Task<NetworkUser[]> GetMyAssociatedUsersAsync(string accessToken);

        Task<User> GetMyUserAsync(string site, string accessToken);
    }

    public class SEApiService : ISEApiService
    {
        private const string FilterId = "!)iuLOYsiL7DYw9VYzWRER";

        private string ClientId { get; }
        private string AppKey { get; }
        private string AppSecret { get; }

        HttpClientHandler _handler => new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        HttpClient _httpClient => new HttpClient(_handler)
        {
            BaseAddress = new Uri("https://api.stackexchange.com")
        };

        public SEApiService(IConfiguration _configuration)
        {
            ClientId = _configuration.GetValue<string>("STACKAPP_CLIENT_ID");
            AppKey = _configuration.GetValue<string>("STACKAPP_KEY");
            AppSecret = _configuration.GetValue<string>("STACKAPP_SECRET");
        }

        Task<HttpResponseMessage> GetFromApi(string url, string accessToken)
        {
            var glue = url.Contains("?")? "&" : "?";
            return _httpClient.GetAsync($"{url}{glue}key={AppKey}&access_token={accessToken}&filter={FilterId}");
        }

        public string GetInitialOauthUrl(string returnUrl, string state = null)
        {
            return $"https://stackoverflow.com/oauth?client_id={ClientId}&redirect_uri={WebUtility.UrlEncode(returnUrl)}" +
                (state == null ? "" : $"&state={WebUtility.UrlEncode(state)}");
        }

        public async Task<string> GetAccessTokenFromCodeAsync(string code, string returnUrl)
        {
            using(var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://stackoverflow.com");
                var content = new FormUrlEncodedContent(new []
                {
                    ("client_id", ClientId),
                    ("client_secret", AppSecret),
                    ("code", code),
                    ("redirect_uri", returnUrl),
                }.Select(e => new KeyValuePair<string, string>(e.Item1, e.Item2)));
                var result = await client.PostAsync("/oauth/access_token", content);
                result.EnsureSuccessStatusCode();

                string resultContent = await result.Content.ReadAsStringAsync();
                var parsedData = HttpUtility.ParseQueryString(resultContent);
                return parsedData["access_token"];
            }
        }

        public async Task<NetworkUser[]> GetMyAssociatedUsersAsync(string accessToken)
        {
            var result = await GetFromApi("/2.2/me/associated", accessToken);
            result.EnsureSuccessStatusCode();

            using(var stream = await result.Content.ReadAsStreamAsync())
            using(var reader = new StreamReader(stream))
            {
                return Jil.JSON.Deserialize<PaginatedResponse<NetworkUser>>(reader, Jil.Options.MillisecondsSinceUnixEpochUtc)
                    .Items;
            }
        }

        public async Task<User> GetMyUserAsync(string site, string accessToken)
        {
            var result = await GetFromApi("/2.2/me?site=" + site, accessToken);
            result.EnsureSuccessStatusCode();

            using(var stream = await result.Content.ReadAsStreamAsync())
            using(var reader = new StreamReader(stream))
            {
                return Jil.JSON.Deserialize<PaginatedResponse<User>>(reader, Jil.Options.MillisecondsSinceUnixEpochUtc)
                    .Items.FirstOrDefault();
            }
        }
    }
}