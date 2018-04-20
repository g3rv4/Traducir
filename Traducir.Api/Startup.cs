using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Profiling;
using Traducir.Core.Helpers;
using Traducir.Core.Services;

namespace Traducir.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            HostingEnvironment = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSingleton(typeof(IDbService), typeof(DbService));
            services.AddSingleton(typeof(ITransifexService), typeof(TransifexService));
            services.AddSingleton(typeof(ISOStringService), typeof(SOStringService));
            services.AddSingleton(typeof(ISEApiService), typeof(SEApiService));
            services.AddSingleton(typeof(IUserService), typeof(UserService));

            services.AddMiniProfiler(settings =>
            {
                settings.RouteBasePath = "/app/mini-profiler-resources";
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o =>
            {
                o.Cookie.Path = "/app";
                o.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
                o.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policy.CanSuggest, policy => policy.RequireClaim(ClaimType.CanSuggest));
                options.AddPolicy(Policy.CanReview, policy => policy.RequireClaim(ClaimType.CanReview));
                options.AddPolicy(Policy.CanManageUsers, policy => policy.RequireClaim(ClaimType.IsModerator));
            });

            services.AddExceptional(settings =>
            {
                settings.UseExceptionalPageOnThrow = HostingEnvironment.IsDevelopment();
                settings.OnBeforeLog += (sender, args) =>
                {
                    var match = Regex.Match(args.Error.FullUrl, "^(([^/]+)//([^/]+))/", RegexOptions.Compiled);
                    var miniProfilerUrl = match.Groups[1].Value + "/app/mini-profiler-resources/results?id=" + MiniProfiler.Current.Id.ToString();

                    args.Error.CustomData = args.Error.CustomData ?? new Dictionary<string, string>();
                    args.Error.CustomData.Add("MiniProfiler", miniProfilerUrl);
                };
                settings.LogFilters.Cookie[".AspNetCore.Cookies"] = "hidden";
            });

            var keysLocation = Configuration.GetValue<string>("KEYS_LOCATION_FOLDER");
            if (keysLocation != null)
            {
                services.AddDataProtection()
                    .PersistKeysToFileSystem(new DirectoryInfo(keysLocation))
                    .SetApplicationName("Traducir")
                    .SetDefaultKeyLifetime(TimeSpan.FromDays(7));
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAuthentication();
            app.UseMiniProfiler();
            app.UseExceptional();

            app.UseCookiePolicy(new CookiePolicyOptions
            {
                HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always
            });

            app.UseMvc();
        }
    }
}