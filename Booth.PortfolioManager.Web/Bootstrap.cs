using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;


using Booth.EventStore;
using Booth.EventStore.MongoDB;
using Booth.PortfolioManager.Domain.Users;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Web.Utilities;

namespace Booth.PortfolioManager.Web
{
    static class PortfolioManagerServiceCollectionExtensions
    {

        public static IServiceCollection AddPortfolioManagerServices(this IServiceCollection services, Configuration settings)
        {
            services.AddSingleton<Configuration>(settings);
            services.AddSingleton<IJwtTokenConfiguration>(settings.JwtTokenConfiguration);

            services.AddScoped<IEventStore>(_ => new MongodbEventStore(settings.EventStore, settings.Database));
            services.AddScoped<IEventStream<User>>(x => x.GetRequiredService<IEventStore>().GetEventStream<User>("Users"));
            services.AddScoped<IEventStream<Stock>>(x => x.GetRequiredService<IEventStore>().GetEventStream<Stock>("Stocks"));
            services.AddScoped<IEventStream<StockPriceHistory>>(x => x.GetRequiredService<IEventStore>().GetEventStream<StockPriceHistory>("StockPriceHistory"));

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IEntityCache<>), typeof(EntityCache<>));
            services.AddScoped(typeof(IEntityFactory<>), typeof(DefaultEntityFactory<>));
            services.AddScoped(typeof(ITrackedEntityFactory<>), typeof(DefaultTrackedEntityFactory<>));

            services.AddScoped<IStockQuery, StockQuery>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IStockService, StockService>();

            return services;
        }

    }
}
