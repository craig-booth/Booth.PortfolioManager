using Booth.PortfolioManager.Web.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Booth.PortfolioManager.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var settings = Configuration.GetSection("Settings").Get<AppSettings>();

            services.AddPortfolioManagerServices(settings)
                .AddDataImportService();

            services.AddMemoryCache();
            services.AddHttpContextAccessor();

            services.AddControllers()
                .AddJsonOptions(options => SerializerSettings.ConfigureOptions(options.JsonSerializerOptions));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {
                    OnPrepareResponse = ctx =>
                    {
                        // Don't cache index.html
                        if (ctx.File.PhysicalPath.EndsWith("index.html"))
                        {
                            ctx.Context.Response.Headers["Cache-Control"] = "no-cache, no-store";
                            ctx.Context.Response.Headers["Pragma"] = "no-cache";
                            ctx.Context.Response.Headers["Expires"] = "-1";
                        }
                    }
            });

            app.UsePortfolioManager();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("/index.html");
            });

        }
    }
}