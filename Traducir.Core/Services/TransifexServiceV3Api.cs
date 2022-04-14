using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Profiling;
using Traducir.Core.Models;
using Traducir.Core.Models.Services;
using Traducir.Core.TransifexV3;

namespace Traducir.Core.Services
{
    public sealed class TransifexServiceV3Api : ITransifexService
    {
        private readonly TransifexApiClient _apiClient;
        private readonly ServiceConfiguration _configuration;
        private readonly ISOStringService _soStringService;

        public TransifexServiceV3Api(TransifexApiClient apiClient, ServiceConfiguration configuration, ISOStringService soStringService)
        {
            _apiClient = apiClient;
            _configuration = configuration;
            _soStringService = soStringService;
        }

        public async Task<ImmutableArray<TransifexString>> GetStringsFromTransifexAsync()
        {
            using (MiniProfiler.Current.Step("Fetching strings from Transifex"))
            {
                var liveTranslations = _apiClient.GetResourceTranslationsCollectionAsync(
                    _configuration.OrganizationSlug,
                    _configuration.ProjectSlug,
                    _configuration.ResourceSlug,
                    _configuration.LanguageSlug);

                var translations = new List<StringThatMayHaveBeenTranslated>();
                await foreach (var translation in liveTranslations)
                {
                    translations.Add(translation);
                }

                return translations
                    .Select(translation => new TransifexString
                    {
                        Key = translation.Key,
                        Source = translation.OriginalStrings.Other,
                        Comment = translation.DeveloperComment ?? string.Empty, // v2 API returned blank string where v2 returns null
                        Translation = translation.Strings?.Other
                    })
                    .ToImmutableArray();
            }
        }

#if RISKY
        public Task<bool> PushStringsToTransifexAsync(ImmutableArray<SOString> strings)
        {
            throw new NotImplementedException("Method not available on a RISKY build");
        }
#else
        public async Task<bool> PushStringsToTransifexAsync(ImmutableArray<SOString> strings)
        {
            using (MiniProfiler.Current.Step("Pushing strings to Transifex"))
            {
                var content = strings.ToImmutableDictionary(
                    s => s.Key,
                    s => new ChromeI18N.Message { Value = s.Translation });

                try
                {
                    await _apiClient.UploadTranslationsForResourceAsync(
                        _configuration.OrganizationSlug,
                        _configuration.ProjectSlug,
                        _configuration.ResourceSlug,
                        _configuration.LanguageSlug,
                        content);
                }
                catch
                {
                    return false;
                }

                await _soStringService.UpdateStringsPushed();
                return true;
            }
        }
#endif

        public sealed class ServiceConfiguration
        {
            public ServiceConfiguration(string organizationSlug, string projectSlug, string resourceSlug, string languageSlug)
            {
                OrganizationSlug = organizationSlug;
                ProjectSlug = projectSlug;
                ResourceSlug = resourceSlug;
                LanguageSlug = languageSlug;
            }

            public string OrganizationSlug { get; }

            public string ProjectSlug { get; }

            public string ResourceSlug { get; }

            public string LanguageSlug { get; }
        }
    }
}