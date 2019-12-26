using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Profiling;
using Traducir.Core.Helpers;
using Traducir.Core.Services;
using Traducir.Web.Services;
using Traducir.Web.ViewModels;

namespace Traducir.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment, ILoggerFactory loggerFactory)
        {
#if RISKY
            if (!hostingEnvironment.IsDevelopment() && !configuration.GetValue("ALLOW_RISKY", false))
            {
                throw new Exception("Can't run a risky build here");
            }
#endif
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
            LoggerFactory = loggerFactory;
        }

        public IConfiguration Configuration { get; }

        public IHostingEnvironment HostingEnvironment { get; }

        public ILoggerFactory LoggerFactory { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSingleton(typeof(IDbService), typeof(DbService));
            services.AddSingleton(typeof(ISOStringService), typeof(SOStringService));
            services.AddSingleton(typeof(ISEApiService), typeof(SEApiService));
            services.AddSingleton(typeof(IUserService), typeof(UserService));
            services.AddSingleton(typeof(INotificationService), typeof(NotificationService));
            services.AddSingleton(typeof(IStringsService), typeof(StringsService));

            if (HostingEnvironment.IsDevelopment() && !Configuration.GetValue<bool>("PUSH_TO_TRANSIFEX_ON_DEV"))
            {
                services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddConsole();
                });
                services.AddSingleton(typeof(ILoggerFactory), LoggerFactory);
                services.AddSingleton(typeof(TransifexService), typeof(TransifexService));
                services.AddSingleton(typeof(ITransifexService), typeof(ReadonlyTransifexService));
            }
            else
            {
                services.AddSingleton(typeof(ITransifexService), typeof(TransifexService));
            }

            services.AddMiniProfiler(settings =>
            {
                settings.PopupRenderPosition = RenderPosition.BottomRight;
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o =>
            {
                o.Cookie.Path = "/";
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
                options.AddPolicy(TraducirPolicy.CanSuggest, policy => policy.RequireClaim(ClaimType.CanSuggest));
                options.AddPolicy(TraducirPolicy.CanReview, policy => policy.RequireClaim(ClaimType.CanReview));
                options.AddPolicy(TraducirPolicy.CanManageUsers, policy => policy.RequireClaim(ClaimType.IsModerator));
            });

            services.AddExceptional(settings =>
            {
                settings.UseExceptionalPageOnThrow = true;
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

            services.AddTransient(typeof(LayoutViewModel), typeof(LayoutViewModel));

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services
                .AddMvc(options =>
                {
                    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddViewOptions(options => options.HtmlHelperOptions.ClientValidationEnabled = false);

            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
            });

            services.AddHttpContextAccessor();
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

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseStaticFiles();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseMvc();
        }
    }
}
