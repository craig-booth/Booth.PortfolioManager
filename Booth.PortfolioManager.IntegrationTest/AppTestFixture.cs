using System;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;


using Booth.Common;
using Booth.EventStore;
using Booth.EventStore.Memory;
using Booth.PortfolioManager.Web;
using Booth.PortfolioManager.Web.Utilities;
using Booth.PortfolioManager.Domain.Users;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;

namespace Booth.PortfolioManager.IntegrationTest
{
    static class Ids
    {
        public static Guid BHP = Guid.NewGuid();

        public static Guid UserId = Guid.NewGuid();

        public static Guid PortfolioId = Guid.NewGuid();
    }

    public class AppTestFixture : WebApplicationFactory<Startup>
    {

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var eventStore = new MemoryEventStore();

            AddUsers(eventStore);
            AddStocks(eventStore);
            AddPortfolios(eventStore);

            builder.ConfigureTestServices(x =>
            {
                x.AddSingleton<IEventStore>(eventStore);
                x.RemoveAll<IHostedService>();
            });
        }

        private void AddUsers(IEventStore eventStore)
        {
            var eventStream = eventStore.GetEventStream<User>("Users");

            var standardUser = new User(Ids.UserId);
            standardUser.Create("StandardUser", "secret");
            eventStream.Add(standardUser.Id, "User", standardUser.GetProperties(), standardUser.FetchEvents());

            var standardUser2 = new User(Guid.NewGuid());
            standardUser2.Create("StandardUser2", "secret");
            eventStream.Add(standardUser2.Id, "User", standardUser2.GetProperties(), standardUser2.FetchEvents());

            var administrator = new User(Guid.NewGuid());
            administrator.Create("AdminUser", "secret");
            administrator.AddAdministratorPrivilage();
            eventStream.Add(administrator.Id, "User", administrator.GetProperties(), administrator.FetchEvents());
        }

        private void AddStocks(IEventStore eventStore)
        {
            var eventStream = eventStore.GetEventStream<Stock>("Stocks");

            var stock = new Stock(Ids.BHP);
            stock.List("BHP", "BHP Pty Ltd", new Date(2000, 01, 01), false, Domain.Stocks.AssetCategory.AustralianStocks);
            eventStream.Add(stock.Id, "Stock", stock.FetchEvents());
        }

        private void AddPortfolios(IEventStore eventStore)
        {   
            var repository = new Repository<Stock>(eventStore.GetEventStream<Stock>("Stocks"));

            var stockCache = new EntityCache<Stock>();
            stockCache.PopulateCache(repository);

            var stockResolver = new StockResolver(stockCache);

            var factory = new PortfolioFactory(stockResolver);
            var portfolio = factory.CreatePortfolio(Ids.PortfolioId, "Test", Ids.UserId);

            portfolio.MakeCashTransaction(new Date(2000, 01, 01), BankAccountTransactionType.Deposit, 50000.00m, "", Guid.NewGuid());
            portfolio.AquireShares(Ids.BHP, new Date(2000, 06, 30), 100, 12.00m, 19.95m, true, "", Guid.NewGuid());

            var eventStream = eventStore.GetEventStream<Portfolio>("Portfolios");
            eventStream.Add(portfolio.Id, "Portfolio", portfolio.FetchEvents());
        }
    }
}
