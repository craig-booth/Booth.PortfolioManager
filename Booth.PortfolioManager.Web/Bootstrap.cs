using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;

using Booth.EventStore;
using Booth.EventStore.MongoDB;
using Booth.PortfolioManager.Domain.Users;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Web.Utilities;
using Booth.PortfolioManager.Web.Authentication;
using Booth.PortfolioManager.RestApi.Serialization;

namespace Booth.PortfolioManager.Web
{
    static class PortfolioManagerServiceCollectionExtensions
    {

        public static IServiceCollection AddPortfolioManagerServices(this IServiceCollection services, AppSettings settings)
        {
            services.AddSingleton<AppSettings>(settings);
            services.AddTransient<IConfigureOptions<MvcNewtonsoftJsonOptions>, RestApiMvcJsonOptions>(); 

            IJwtTokenConfigurationProvider jwtTokenConfigProvider;
            if (settings.JwtTokenConfiguration.Key != "")
            {
                var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(settings.JwtTokenConfiguration.Key));
                jwtTokenConfigProvider = new JwtTokenConfigurationProvider(settings.JwtTokenConfiguration.Issuer, settings.JwtTokenConfiguration.Audience, key);
            }
            else
                jwtTokenConfigProvider = new JwtTokenConfigurationProvider(settings.JwtTokenConfiguration.Issuer, settings.JwtTokenConfiguration.Audience, settings.JwtTokenConfiguration.KeyFile);
            services.AddSingleton<IJwtTokenConfigurationProvider>(jwtTokenConfigProvider);

            if (settings.AllowDebugUserAcccess)
                services.AddAnonymousAuthetication();
            else
                services.AddJwtAuthetication(jwtTokenConfigProvider);

            services.AddScoped<IEventStore>(_ => new MongodbEventStore(settings.EventStore, settings.Database));
            services.AddScoped<IEventStream<User>>(x => x.GetRequiredService<IEventStore>().GetEventStream<User>("Users"));
            services.AddScoped<IEventStream<Stock>>(x => x.GetRequiredService<IEventStore>().GetEventStream<Stock>("Stocks"));
            services.AddScoped<IEventStream<StockPriceHistory>>(x => x.GetRequiredService<IEventStore>().GetEventStream<StockPriceHistory>("StockPriceHistory"));

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddSingleton(typeof(IEntityCache<>), typeof(EntityCache<>));
            services.AddScoped(typeof(IEntityFactory<>), typeof(DefaultEntityFactory<>));
            services.AddScoped(typeof(ITrackedEntityFactory<>), typeof(DefaultTrackedEntityFactory<>));

            services.AddScoped<IStockQuery, StockQuery>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IStockService, StockService>();

            return services;
        }

        public static IApplicationBuilder UsePortfolioManager(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                scope.ServiceProvider.InitializeStockCache();
            }

            return app;
        }

        public static IServiceProvider InitializeStockCache(this IServiceProvider serviceProvider)
        {
            var stockRepository = serviceProvider.GetRequiredService<IRepository<Stock>>();
            var stockCache = serviceProvider.GetRequiredService<IEntityCache<Stock>>();
            stockCache.PopulateCache(stockRepository);

            var stockPriceRepository = serviceProvider.GetRequiredService<IRepository<StockPriceHistory>>();
            var stockPriceHistoryCache = serviceProvider.GetRequiredService<IEntityCache<StockPriceHistory>>();
            stockPriceHistoryCache.PopulateCache(stockPriceRepository);

            // Hook up stock prices to stocks
            foreach (var stock in stockCache.All())
            {
                var stockPriceHistory = stockPriceHistoryCache.Get(stock.Id);
                if (stockPriceHistory != null)
                    stock.SetPriceHistory(stockPriceHistory);
            }

            return serviceProvider;
        }
        private static void AddJwtAuthetication(this IServiceCollection services, IJwtTokenConfigurationProvider jwtTokenConfigProvider)
        {
             services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                { 
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = false,
                        IssuerSigningKey = jwtTokenConfigProvider.Key,
                        ValidateIssuer = false,
                        ValidIssuer = jwtTokenConfigProvider.Issuer,
                        ValidateAudience = false,
                        ValidAudience = jwtTokenConfigProvider.Audience,
                        ValidateLifetime = false
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policy.IsPortfolioOwner, policy => policy.Requirements.Add(new PortfolioOwnerRequirement()));
                options.AddPolicy(Policy.CanMantainStocks, policy => policy.RequireRole(Role.Administrator));
            });
        }

        private static void AddAnonymousAuthetication(this IServiceCollection services)
        {
            services.AddAuthentication("AnonymousAuthentication")
                .AddScheme<AuthenticationSchemeOptions, AnonymousAuthenticationHandler>("AnonymousAuthentication", null);

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policy.IsPortfolioOwner, policy => policy.RequireUserName("DebugUser"));
                options.AddPolicy(Policy.CanMantainStocks, policy => policy.RequireUserName("DebugUser"));
            });
        }

    }


    public class RestApiMvcJsonOptions : IConfigureOptions<MvcNewtonsoftJsonOptions>
    {
        public void Configure(MvcNewtonsoftJsonOptions options)
        {
            RestSerializerSettings.Configure(options.SerializerSettings);
        }
    }

}
