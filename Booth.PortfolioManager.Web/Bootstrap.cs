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

using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.Domain.Users;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.TradingCalendars;
using Booth.PortfolioManager.RestApi.Serialization;
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
            services.AddSingleton(typeof(IEntityCache<>), typeof(EntityCache<>));


            // Repositories
            services.AddScoped<IPortfolioManagerDatabase>(x => new PortfolioManagerDatabase(settings.ConnectionString, settings.Database, x.GetRequiredService<IPortfolioFactory>(), x.GetRequiredService<IStockResolver>()));
            services.AddScoped<IPortfolioRepository, PortfolioRepository>();
            services.AddScoped<IStockRepository, StockRepository>();
            services.AddScoped<IStockPriceRepository, StockPriceRepository>();
            services.AddScoped<ITradingCalendarRepository, TradingCalendarRepository>();
            services.AddScoped<IUserRepository, UserRepository>();


            // Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IStockService, StockService>();
            services.AddScoped<ITradingCalendarService, TradingCalendarService>();
            services.AddScoped<ICashAccountService, CashAccountService>();
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
            services.AddScoped<ICorporateActionService, CorporateActionService>();


            // Others
            services.AddScoped<IReadOnlyPortfolio>(x => x.GetRequiredService<IPortfolioAccessor>().ReadOnlyPortfolio);
            services.AddScoped<IPortfolio>(x => x.GetRequiredService<IPortfolioAccessor>().Portfolio);
            services.AddScoped<IPortfolioAccessor, PortfolioAccessor>();
            services.AddSingleton<IPortfolioFactory, PortfolioFactory>();
            services.AddSingleton<IPortfolioCache, PortfolioCache>();
            services.AddSingleton<IStockResolver, StockResolver>();
            services.AddSingleton<IStockQuery, StockQuery>();

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

            services.AddScoped<HistoricalPriceImporter>();
            services.AddScoped<LivePriceImporter>();
            services.AddScoped<TradingDayImporter>();

            services.AddHostedService<DataImportBackgroundService>();

            return services;
        }

        public static IServiceProvider InitializeStockCache(this IServiceProvider serviceProvider)
        {
            var stockRepository = serviceProvider.GetRequiredService<IStockRepository>();
            var stockCache = serviceProvider.GetRequiredService<IEntityCache<Stock>>();
            stockCache.PopulateCache(stockRepository);

            var stockPriceRepository = serviceProvider.GetRequiredService<IStockPriceRepository>();
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
