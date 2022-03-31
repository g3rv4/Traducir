#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Traducir.Core.TransifexV3
{
    public sealed class TransifexApiClient
    {
        // Transifex supports a page size between 150 and 1000
        private const int TransifexApiPageSize = 1000;

        private const string ContentEncodingText = "text";
        private const string FileTypeDefault = "default";
        private const string UploadStatusFailed = "failed";
        private const string UploadStatusSucceeded = "succeeded";

        private static readonly Uri BaseUrl = new("https://rest.api.transifex.com");

        private readonly string _apiToken;
        private readonly HttpClient _httpClient;

        public TransifexApiClient(string apiToken, HttpClient httpClient)
        {
            _apiToken = apiToken;
            _httpClient = httpClient;
        }

        private static string SerializeJsonWithoutNulls(object value)
        {
            var options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            return JsonSerializer.Serialize(value, options: options);
        }

        public async Task UploadTranslationsForResourceAsync(
            string organizationSlug,
            string projectSlug,
            string resourceSlug,
            string languageSlug,
            ImmutableDictionary<string, ChromeI18N.Message> translationStrings,
            CancellationToken cancellationToken = default)
        {
            // https://transifex.github.io/openapi/index.html#tag/Resource-Translations/paths/~1resource_translations_async_uploads/post
            var requestBody = new
            {
                data = new
                {
                    attributes = new { content = SerializeJsonWithoutNulls(translationStrings), content_encoding = ContentEncodingText, file_type = FileTypeDefault },
                    relationships = new
                    {
                        language = new { data = new { id = $"l:{languageSlug}", type = "languages" } },
                        resource = new { data = new { id = $"o:{organizationSlug}:p:{projectSlug}:r:{resourceSlug}", type = "resources" } }
                    },
                    type = "resource_translations_async_uploads"
                }
            };

            var result = await SendPostRequestAsync<UpdateTranslationsResponse>("resource_translations_async_uploads", requestBody, cancellationToken);
            var uploadStatusId = result.Data.Id;
            await WaitUntilUploadFinishesAsync(
                () => SendGetRequestAsync<GetUploadStatusResponse>($"resource_translations_async_uploads/{HttpUtility.UrlEncode(uploadStatusId)}", cancellationToken),
                cancellationToken);
        }

        public IAsyncEnumerable<StringThatMayHaveBeenTranslated> GetResourceTranslationsCollectionAsync(
            string organizationSlug,
            string projectSlug,
            string resourceSlug,
            string languageSlug,
            CancellationToken cancellationToken = default)
        {
            // https://transifex.github.io/openapi/index.html#tag/Resource-Translations/paths/~1resource_translations/get
            var resourceId = $"o:{organizationSlug}:p:{projectSlug}:r:{resourceSlug}";
            var languageId = $"l:{languageSlug}";
            var queryString = $"?filter[resource]={HttpUtility.UrlEncode(resourceId)}&filter[language]={HttpUtility.UrlEncode(languageId)}&include=resource_string&limit={TransifexApiPageSize}";

            return GetAllPagesOfDataAsync<GetResourceTranslationCollectionResponse, StringThatMayHaveBeenTranslated>(
                "resource_translations" + queryString,
                response =>
                    response.Data.Zip(response.Included, (data, included) =>
                        new StringThatMayHaveBeenTranslated(
                            Key: included.Attributes.Key,
                            SourceStringHash: included.Attributes.StringHash,
                            DeveloperComment: included.Attributes.DeveloperComment,
                            DateTimeCreated: data.Attributes.DateTimeCreated,
                            Strings: data.Attributes.Strings,
                            OriginalStrings: included.Attributes.Strings)),
                cancellationToken);
        }

        private static async Task WaitUntilUploadFinishesAsync(
            Func<Task<GetUploadStatusResponse>> getUploadStatus,
            CancellationToken cancellationToken,
            [CallerMemberName] string callerName = "")
        {
            // https://transifex.github.io/openapi/index.html#section/Asynchronous-Processing
            const int maxDelayInMs = 4_000;
            var nextDelayInMs = 500;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var statusAndErrors = (await getUploadStatus()).Data.Attributes;
                switch (statusAndErrors.Status)
                {
                    case UploadStatusSucceeded:
                        return;
                    case UploadStatusFailed:
                        var transifexErrors = statusAndErrors.Errors.Select(error => new TransifexClientError(statusAndErrors.Status, error.Code, error.Detail, "Upload Error")).ToImmutableList();
                        throw new TransifexException(callerName + " failed", transifexErrors);
                }

                await Task.Delay(nextDelayInMs, cancellationToken);
                if (nextDelayInMs < maxDelayInMs)
                {
                    nextDelayInMs *= 2;
                }
            }
        }

        private async IAsyncEnumerable<TResult> GetAllPagesOfDataAsync<T, TResult>(
            string relativeUrl,
            Func<T, IEnumerable<TResult>> getResultsFromPage,
            [EnumeratorCancellation] CancellationToken cancellationToken,
            [CallerMemberName] string callerName = "")
                where T : IHavePaginationLinks
        {
            // https://transifex.github.io/openapi/index.html#section/Pagination
            string relativeUrlWithCursor = relativeUrl;
            while (true)
            {
                var response = await SendGetRequestAsync<T>(relativeUrlWithCursor, cancellationToken, callerName);
                foreach (var result in getResultsFromPage(response))
                {
                    yield return result;
                }

                if (response.Links.Next is null)
                {
                    yield break;
                }

                relativeUrlWithCursor = response.Links.Next.PathAndQuery.ToString(CultureInfo.InvariantCulture);
            }
        }

        private async Task<T> SendGetRequestAsync<T>(
            string relativeUrl,
            CancellationToken cancellationToken,
            [CallerMemberName] string callerName = "")
        {
            using var request = new HttpRequestMessage
            {
                RequestUri = new Uri(BaseUrl, relativeUrl),
                Method = HttpMethod.Get
            };
            return await SendRequestAsync<T>(request, callerName, cancellationToken);
        }

        private async Task<T> SendPostRequestAsync<T>(
            string relativeUrl,
            object requestBody,
            CancellationToken cancellationToken,
            [CallerMemberName] string callerName = "")
        {
            // https://transifex.github.io/openapi/index.html#section/File-Uploads
            using var request = new HttpRequestMessage
            {
                RequestUri = new Uri(BaseUrl, relativeUrl),
                Method = HttpMethod.Post,

                // Can't use JsonContent.Create(requestBody) because it will automatically chunk the request for big payloads and Transifex has not implemented chunking
                Content = new StringContent(SerializeJsonWithoutNulls(requestBody), null, "application/vnd.api+json")
            };

            // Transifex doesn't support a CharSet on the Content Header, even if we pass null as the CharSet on new StringContent() it will fill it with a default that we need to override here
            request.Content.Headers.ContentType!.CharSet = null;
            return await SendRequestAsync<T>(request, callerName, cancellationToken);
        }

        private async Task<T> SendRequestAsync<T>(
            HttpRequestMessage request,
            string callerNameToIncludeInException,
            CancellationToken cancellationToken)
        {
            // https://transifex.github.io/openapi/index.html#section/Authentication
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiToken);
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
                return result!;
            }
            else
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(cancellationToken: cancellationToken);
                throw new TransifexException(callerNameToIncludeInException + " failed", error!.Errors);
            }
        }

#pragma warning disable SA1313 // This shouldn't apply to records - a future release of the StyleCop analyzers will fix this
#pragma warning disable CA1812 // Don't complain about these private types not being instantiated - it happens via de/serialization
#pragma warning disable IDE0079 // Don't complain about the pragma below not being required - it IS required, there is an analyzer flaw somewhere
#pragma warning disable CA1801 // Don't complain about these private type parameters not all being used in code - it happens via de/serialization
        private sealed record GetResourceTranslationCollectionResponse(ImmutableList<GetResourceTranslationCollectionData> Data, ImmutableList<GetResourceTranslationCollectionResponseIncluded> Included, PaginationLink Links) : IHavePaginationLinks;

        private sealed record GetResourceTranslationCollectionData(GetResourceTranslationCollectionDataAttributes Attributes, GetResourceTranslationCollectionDataRelationships Relationships);

        private sealed record GetResourceTranslationCollectionDataAttributes([property: JsonPropertyName("datetime_created")] DateTimeOffset DateTimeCreated, TransifexStrings? Strings);

        private sealed record GetResourceTranslationCollectionDataRelationships([property: JsonPropertyName("resource_string")] GetResourceTranslationCollectionDataRelationshipsResourceString ResourceString);

        private sealed record GetResourceTranslationCollectionDataRelationshipsResourceString(GetResourceTranslationCollectionDataRelationshipsResourceStringData Data);

        private sealed record GetResourceTranslationCollectionDataRelationshipsResourceStringData(string Id);

        private sealed record GetResourceTranslationCollectionResponseIncluded(string Id, GetResourceTranslationCollectionResponseIncludedAttributes Attributes);

        private sealed record GetResourceTranslationCollectionResponseIncludedAttributes(
            string Key,
            [property: JsonPropertyName("developer_comment")] string? DeveloperComment,
            [property: JsonPropertyName("string_hash")] string StringHash,
            TransifexStrings Strings);

        private sealed record PaginationLink(Uri? Next, Uri? Previous);

        private sealed record GetUploadStatusResponse(GetUploadStatusData Data);

        private sealed record GetUploadStatusData(GetUploadStatusAttributes Attributes);

        private sealed record GetUploadStatusAttributes(string Status, ImmutableList<GetUploadStatusError> Errors);

        private sealed record GetUploadStatusError(string Code, string Detail);

        private sealed record UpdateTranslationsResponse(UpdateTranslationsResponseData Data);

        private sealed record UpdateTranslationsResponseData(string Id);

        private sealed record ErrorResponse(ImmutableList<TransifexClientError> Errors);

        private interface IHavePaginationLinks
        {
            PaginationLink Links { get; }
        }
    }
}
