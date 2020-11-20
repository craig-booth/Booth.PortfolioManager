using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Booth.EventStore;
using Booth.EventStore.MongoDB;
using Booth.PortfolioManager.RestApi.Serialization;
using Booth.PortfolioManager.Domain.Users;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.TradingCalendars;
using Booth.PortfolioManager.DataServices;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Web.Utilities;
using Booth.PortfolioManager.Web.Authentication;
using Booth.PortfolioManager.Web.DataImporters;
using Booth.Common;
using System.Reflection;
using System.Drawing;

namespace Booth.PortfolioManager.Web
{
    static class PortfolioManagerServiceCollectionExtensions
    {

        public static IServiceCollection AddPortfolioManagerServices(this IServiceCollection services, AppSettings settings)
        {
            services.AddSingleton<AppSettings>(settings);
            services.AddTransient<IConfigureOptions<MvcNewtonsoftJsonOptions>, RestApiMvcJsonOptions>();

            IJwtTokenConfigurationProvider jwtTokenConfigProvider;
            if ((settings.JwtTokenConfiguration.Key != null) && (settings.JwtTokenConfiguration.Key != ""))
            {
                var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(settings.JwtTokenConfiguration.Key));
                jwtTokenConfigProvider = new JwtTokenConfigurationProvider(settings.JwtTokenConfiguration.Issuer, settings.JwtTokenConfiguration.Audience, key);
            }
            else
                jwtTokenConfigProvider = new JwtTokenConfigurationProvider(settings.JwtTokenConfiguration.Issuer, settings.JwtTokenConfiguration.Audience, settings.JwtTokenConfiguration.KeyFile);
            services.AddSingleton<IJwtTokenConfigurationProvider>(jwtTokenConfigProvider);

            services.AddScoped<IAuthorizationHandler, PortfolioOwnerAuthorizationHandler>();

            if (settings.AllowDebugUserAcccess)
                services.AddAnonymousAuthetication();
            else
                services.AddJwtAuthetication(jwtTokenConfigProvider);

            // Generic classes
            services.AddSingleton(typeof(IRepository<>), typeof(Repository<>));
            services.AddSingleton(typeof(IEntityCache<>), typeof(EntityCache<>));

            // Event Store
            services.AddSingleton<IEventStore>(_ => new MongodbEventStore(settings.EventStore, settings.Database));
            services.AddSingleton<IEventStream<User>>(x => x.GetRequiredService<IEventStore>().GetEventStream<User>("Users"));
            services.AddSingleton<IEventStream<Stock>>(x => x.GetRequiredService<IEventStore>().GetEventStream<Stock>("Stocks"));
            services.AddSingleton<IEventStream<StockPriceHistory>>(x => x.GetRequiredService<IEventStore>().GetEventStream<StockPriceHistory>("StockPriceHistory"));
            services.AddSingleton<IEventStream<TradingCalendar>>(x => x.GetRequiredService<IEventStore>().GetEventStream<TradingCalendar>("TradingCalendar"));
            services.AddSingleton<IEventStream<Portfolio>>(x => x.GetRequiredService<IEventStore>().GetEventStream<Portfolio>("Portfolios"));

            // Entity Factories
            services.AddSingleton<ITrackedEntityFactory<Stock>, StockEntityFactory>();
            services.AddSingleton<ITrackedEntityFactory<Portfolio>, PortfolioEntityFactory>();

            // Services
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IStockService, StockService>();
            services.AddSingleton<ITradingCalendarService>(x => new TradingCalendarService(x.GetRequiredService<IRepository<TradingCalendar>>(), TradingCalendarIds.ASX));
            services.AddScoped<ICashAccountService, CashAccountService>();
        //    services.AddScoped<ICorporateActionService, CorporateActionService>();
            services.AddScoped<IPortfolioCapitalGainsService, PortfolioCapitalGainsService>();
            services.AddScoped<IPortfolioCgtLiabilityService, PortfolioCgtLiabilityService>();
            services.AddScoped<IPortfolioCorporateActionsService, PortfolioCorporateActionsService>();
            services.AddScoped<IPortfolioIncomeService, PortfolioIncomeService>();
            services.AddScoped<IPortfolioPerformanceService, PortfolioPerformanceService>();
            services.AddScoped<IPortfolioPropertiesService, PortfolioPropertiesService>();
            services.AddScoped<IPortfolioHoldingService, PortfolioHoldingService>();
            services.AddScoped<IPortfolioService, PortfolioService>();
            services.AddScoped<IPortfolioSummaryService, PortfolioSummaryService>();
            services.AddScoped<IPortfolioTransactionService, PortfolioTransactionService>();
            services.AddScoped<IPortfolioValueService, PortfolioValueService>();


            // Others
            services.AddScoped<IReadOnlyPortfolio>(x => x.GetRequiredService<IPortfolioAccessor>().ReadOnlyPortfolio);
            services.AddScoped<IPortfolio>(x => x.GetRequiredService<IPortfolioAccessor>().Portfolio);
            services.AddScoped<IPortfolioAccessor, PortfolioAccessor>();
            services.AddSingleton<IPortfolioFactory, PortfolioFactory>();
            services.AddSingleton<IPortfolioCache, PortfolioCache>();
            services.AddSingleton<IStockResolver, StockResolver>();
            services.AddSingleton<IStockQuery, StockQuery>();
            services.AddSingleton<ITradingCalendar>(x => x.GetRequiredService<ITradingCalendarService>().TradingCalendar);

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


    class RestApiMvcJsonOptions : IConfigureOptions<MvcNewtonsoftJsonOptions>
    {
        public void Configure(MvcNewtonsoftJsonOptions options)
        {
            RestSerializerSettings.Configure(options.SerializerSettings);
        }
    }

}
