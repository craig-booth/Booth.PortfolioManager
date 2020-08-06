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
using Microsoft.Extensions.Hosting;

using Booth.EventStore;
using Booth.EventStore.MongoDB;
using Booth.Scheduler;
using Booth.PortfolioManager.RestApi.Serialization;
using Booth.PortfolioManager.Domain.Users;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.TradingCalendars;
using Booth.PortfolioManager.DataServices;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Web.Utilities;
using Booth.PortfolioManager.Web.Authentication;
using Booth.PortfolioManager.Web.DataImporters;


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

            services.AddSingleton<IEventStore>(_ => new MongodbEventStore(settings.EventStore, settings.Database));
            services.AddSingleton<IEventStream<User>>(x => x.GetRequiredService<IEventStore>().GetEventStream<User>("Users"));
            services.AddSingleton<IEventStream<Stock>>(x => x.GetRequiredService<IEventStore>().GetEventStream<Stock>("Stocks"));
            services.AddSingleton<IEventStream<StockPriceHistory>>(x => x.GetRequiredService<IEventStore>().GetEventStream<StockPriceHistory>("StockPriceHistory"));
            services.AddSingleton<IEventStream<TradingCalendar>>(x => x.GetRequiredService<IEventStore>().GetEventStream<TradingCalendar>("TradingCalendar"));

            services.AddSingleton(typeof(IRepository<>), typeof(Repository<>));
            services.AddSingleton(typeof(IEntityCache<>), typeof(EntityCache<>));
            services.AddSingleton(typeof(IEntityFactory<>), typeof(DefaultEntityFactory<>));
            services.AddSingleton(typeof(ITrackedEntityFactory<>), typeof(DefaultTrackedEntityFactory<>));

            services.AddSingleton<ITradingCalendar>(x => x.GetRequiredService<IRepository<TradingCalendar>>().Get(TradingCalendarIds.ASX));

            services.AddSingleton<IStockQuery, StockQuery>();

            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IStockService, StockService>();
            services.AddSingleton<ITradingCalendarService>(x => new TradingCalendarService(x.GetRequiredService<IRepository<TradingCalendar>>(), TradingCalendarIds.ASX));

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
        public static IServiceCollection AddDataImportService(this IServiceCollection services)
        {
            services.AddHttpClient<IHistoricalStockPriceService, AsxDataService>();
            services.AddHttpClient<ILiveStockPriceService, AsxDataService>();
            services.AddHttpClient<ITradingDayService, AsxDataService>();

            services.AddSingleton<HistoricalPriceImporter>();
            services.AddSingleton<LivePriceImporter>();
            services.AddSingleton<TradingDayImporter>();

            services.AddHostedService<DataImportBackgroundService>();

            return services;
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
