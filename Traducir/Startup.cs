using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Traducir.Core.Services;

namespace Traducir
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

            services.AddMiniProfiler();
            services.AddExceptional(settings =>
            {
                settings.UseExceptionalPageOnThrow = HostingEnvironment.IsDevelopment();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiniProfiler();
            app.UseExceptional();

            app.UseMvc();
        }
    }
}