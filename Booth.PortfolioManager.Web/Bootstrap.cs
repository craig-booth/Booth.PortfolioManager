using System;
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
using MongoDB.Driver;
using Microsoft.Extensions.Caching.Memory;

using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.TradingCalendars;
using Booth.PortfolioManager.RestApi.Serialization;
using Booth.PortfolioManager.DataServices;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Web.Utilities;
using Booth.PortfolioManager.Web.Authentication;
using Booth.PortfolioManager.Web.DataImporters;
using Booth.PortfolioManager.Web.Mappers;
using Booth.PortfolioManager.Web.CachedRepositories;
using System.Net.Security;

namespace Booth.PortfolioManager.Web
{
    static class PortfolioManagerServiceCollectionExtensions
    {

        public static IServiceCollection AddPortfolioManagerServices(this IServiceCollection services, AppSettings settings)
        {
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

            // Caches classes
            services.AddSingleton(typeof(IEntityCache<>), typeof(EntityCache<>));

            // Database
            services.AddSingleton<IMongoClient>(new MongoClient(settings.ConnectionString));
            services.AddScoped<IPortfolioManagerDatabase>(x => new PortfolioManagerDatabase(x.GetRequiredService<IMongoClient>(), settings.Database, x.GetRequiredService<IPortfolioFactory>(), x.GetRequiredService<IStockResolver>()));

            // Repositories
            services.AddScoped<IPortfolioRepository>(x => new CachedPortfolioRepository(new PortfolioRepository(x.GetRequiredService<IPortfolioManagerDatabase>()), x.GetRequiredService<IMemoryCache>()));
            services.AddScoped<IStockRepository>(x => new CachedStockRepository(new StockRepository(x.GetRequiredService<IPortfolioManagerDatabase>()), x.GetRequiredService<IEntityCache<Stock>>()));
            services.AddScoped<IStockPriceRepository>(x => new CachedStockPriceRepository(new StockPriceRepository(x.GetRequiredService<IPortfolioManagerDatabase>()), x.GetRequiredService<IEntityCache<StockPriceHistory>>()));
            services.AddScoped<ITradingCalendarRepository>(x => new CachedTradingCalendarRepository(new TradingCalendarRepository(x.GetRequiredService<IPortfolioManagerDatabase>()), x.GetRequiredService<IMemoryCache>()));
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


            //Mappers
            services.AddScoped<IHoldingMapper, HoldingMapper>();
            services.AddScoped<IStockMapper, StockMapper>();
            services.AddScoped<ICorporateActionMapper, CorporateActionMapper>();
            services.AddScoped<ITransactionMapper, TransactionMapper>();


            // Others
            services.AddScoped<IReadOnlyPortfolio>(x => x.GetRequiredService<IHttpContextPortfolioAccessor>().GetReadOnlyPortfolio().Result);
            services.AddScoped<IPortfolio>(x => x.GetRequiredService<IHttpContextPortfolioAccessor>().GetPortfolio().Result);
            services.AddScoped<IHttpContextPortfolioAccessor, HttpContextPortfolioAccessor>();
            services.AddScoped<IPortfolioFactory, PortfolioFactory>();
            services.AddScoped<IStockResolver, StockResolver>();
            services.AddScoped<IStockQuery, StockQuery>();
            services.AddScoped<IStockPriceRetriever, StockPriceRetriever>();
            services.AddScoped<IPortfolioReturnCalculator, IrrReturnCalculator>();

            return services;
        }

        public static IApplicationBuilder UsePortfolioManager(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var task = InitializeStockCache(scope.ServiceProvider);
                task.Wait();
            }

            return app;
        }
        public static IServiceCollection AddDataImportService(this IServiceCollection services)
        {
            services.AddHttpClient<IHistoricalStockPriceService, YahooDataService>();
            services.AddHttpClient<ILiveStockPriceService, AsxDataService>();
            services.AddHttpClient<ITradingDayService, AsxDataService>();

            services.AddScoped<HistoricalPriceImporter>();
            services.AddScoped<LivePriceImporter>();
            services.AddScoped<TradingDayImporter>();

            services.AddHostedService<DataImportBackgroundService>();

            return services;
        }

        private static async Task InitializeStockCache(IServiceProvider serviceProvider)
        {
            // Load all entities from the repositories
            var stockRepository = serviceProvider.GetRequiredService<IStockRepository>();
            var stockCache = serviceProvider.GetRequiredService<IEntityCache<Stock>>();
            stockCache.Clear();
            var allStocks = await stockRepository.AllAsync().ToListAsync();

            var stockPriceRepository = serviceProvider.GetRequiredService<IStockPriceRepository>();
            var stockPriceHistoryCache = serviceProvider.GetRequiredService<IEntityCache<StockPriceHistory>>();
            stockPriceHistoryCache.Clear();
            var allPrices = await stockPriceRepository.AllAsync().ToListAsync();
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
                options.AddPolicy(Policy.CanCreatePortfolio, policy =>  policy.RequireAuthenticatedUser());
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
