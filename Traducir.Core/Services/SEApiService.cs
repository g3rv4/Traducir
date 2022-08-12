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
        private const string FilterId = "SY(8)VHj.3*xaOr3N*GB)B";
        private readonly string _clientId;
        private readonly string _appKey;
        private readonly string _appSecret;

        public SEApiService(IConfiguration configuration)
        {
            _clientId = configuration.GetValue<string>("STACKAPP_CLIENT_ID");
            _appKey = configuration.GetValue<string>("STACKAPP_KEY");
            _appSecret = configuration.GetValue<string>("STACKAPP_SECRET");
        }

        private HttpClientHandler Handler => new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        private HttpClient HttpClient => new HttpClient(Handler)
        {
            BaseAddress = new Uri("https://api.stackexchange.com")
        };

        public string GetInitialOauthUrl(string returnUrl, string state = null)
        {
            return $"https://stackoverflow.com/oauth?client_id={_clientId}&redirect_uri={WebUtility.UrlEncode(returnUrl)}" +
                (state == null ? string.Empty : $"&state={WebUtility.UrlEncode(state)}");
        }

        public async Task<string> GetAccessTokenFromCodeAsync(string code, string returnUrl)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://stackoverflow.com");
                var content = new FormUrlEncodedContent(new[]
                {
                    ("client_id", _clientId),
                    ("client_secret", _appSecret),
                    ("code", code),
                    ("redirect_uri", returnUrl),
                }.Select(e => new KeyValuePair<string, string>(e.Item1, e.Item2)));
                var result = await client.PostAsync("/oauth/access_token", content);
                if (result.StatusCode == HttpStatusCode.Forbidden)
                {
                    var response = await result.Content.ReadAsStringAsync();
                    throw new Exception("Got a 403 when trying to get the token. Body: " + response);
                }

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

            using (var stream = await result.Content.ReadAsStreamAsync())
            using (var reader = new StreamReader(stream))
            {
                return Jil.JSON.Deserialize<PaginatedResponse<NetworkUser>>(reader, Jil.Options.MillisecondsSinceUnixEpochUtc)
                    .Items;
            }
        }

        public async Task<User> GetMyUserAsync(string site, string accessToken)
        {
            var result = await GetFromApi("/2.2/me?site=" + site, accessToken);
            result.EnsureSuccessStatusCode();

            using (var stream = await result.Content.ReadAsStreamAsync())
            using (var reader = new StreamReader(stream))
            {
                return Jil.JSON.Deserialize<PaginatedResponse<User>>(reader, Jil.Options.MillisecondsSinceUnixEpochUtc)
                    .Items.FirstOrDefault();
            }
        }

        private Task<HttpResponseMessage> GetFromApi(string url, string accessToken)
        {
            var glue = url.Contains("?", StringComparison.OrdinalIgnoreCase) ? "&" : "?";
            return HttpClient.GetAsync($"{url}{glue}key={_appKey}&access_token={accessToken}&filter={FilterId}");
        }
    }
}