using System;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;

using MongoDB.Driver;
using MongoDB.Bson;

using Booth.Common;
using Booth.PortfolioManager.Web;
using Booth.PortfolioManager.Web.Utilities;
using Booth.PortfolioManager.Domain.Users;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.Domain.TradingCalendars;

namespace Booth.PortfolioManager.IntegrationTest.TestFixture
{
    static class Ids
    {
        public static Guid BHP = Guid.NewGuid();

        public static Guid UserId = Guid.NewGuid();

        public static Guid PortfolioId = Guid.NewGuid();
    }

    public class AppTestFixture : WebApplicationFactory<Startup>
    {
        private TestPortfolioAccessor _PortfolioAccessor;
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var userRepository = new InMemoryUserRepository();
            AddUsers(userRepository);

            var calendarRepository = new InMemoryTradingCalendarRepository();
            AddTradingCalanders(calendarRepository);

            var stockRepository = new InMemoryStockRepository();
            AddStocks(stockRepository);

            var stockPriceRepository = new InMemoryStockPriceRepository();

            var portfolioRepository = new InMemoryPortfolioRepository();
            AddPortfolios(portfolioRepository, stockRepository);

            builder.ConfigureTestServices(x =>
            {
                x.AddSingleton<IPortfolioManagerDatabase, TestDatabase>();
                x.AddSingleton<IPortfolioRepository>(portfolioRepository);
                x.AddSingleton<IUserRepository>(userRepository);
                x.AddSingleton<IStockRepository>(stockRepository);
                x.AddSingleton<IStockPriceRepository>(stockPriceRepository);
                x.AddSingleton<ITradingCalendarRepository>(calendarRepository);

                x.RemoveAll<IHostedService>();
                x.AddScoped<IPortfolioAccessor>(_ => _PortfolioAccessor);
            });
        }

        private class TestPortfolioAccessor : IPortfolioAccessor
        {
            private Portfolio _Portfolio;
            public TestPortfolioAccessor(Portfolio portfolio)
            {
                _Portfolio = portfolio;
            }

            public IReadOnlyPortfolio ReadOnlyPortfolio => _Portfolio;

            public IPortfolio Portfolio => _Portfolio;
        }

        private class TestDatabase : IPortfolioManagerDatabase
        {
            public void Configure(IPortfolioFactory portfolioFactory, IStockResolver stockResolver) { }

            public IMongoCollection<BsonDocument> GetCollection(string name) { return null; }

            public IMongoCollection<T> GetCollection<T>(string name) { return null; }

        }

        private void AddUsers(IUserRepository repository)
        {

            var standardUser = new User(Ids.UserId);
            standardUser.Create("StandardUser", "secret");
            repository.Add(standardUser);

            var standardUser2 = new User(Guid.NewGuid());
            standardUser2.Create("StandardUser2", "secret");
            repository.Add(standardUser2);

            var administrator = new User(Guid.NewGuid());
            administrator.Create("AdminUser", "secret");
            administrator.AddAdministratorPrivilage();
            repository.Add(administrator);
        }

        private void AddTradingCalanders(ITradingCalendarRepository repository)
        {
            var calander = new TradingCalendar(TradingCalendarIds.ASX);    
            repository.Add(calander);
        }

        private void AddStocks(IStockRepository repository)
        {
            var stock = new Stock(Ids.BHP);
            stock.List("BHP", "BHP Pty Ltd", new Date(2000, 01, 01), false, AssetCategory.AustralianStocks);
            repository.Add(stock);
        }

        private void AddPortfolios(IPortfolioRepository repository, IStockRepository stockRepository)
        {
            var stockCache = new EntityCache<Stock>();
            stockCache.PopulateCache(stockRepository);

            var stockResolver = new StockResolver(stockCache);

            var factory = new PortfolioFactory(stockResolver);
            var portfolio = factory.CreatePortfolio(Ids.PortfolioId);
            portfolio.Create("Test", Ids.UserId);

            portfolio.MakeCashTransaction(new Date(2000, 01, 01), BankAccountTransactionType.Deposit, 50000.00m, "", Guid.NewGuid());
            portfolio.AquireShares(Ids.BHP, new Date(2000, 06, 30), 100, 12.00m, 19.95m, true, "", Guid.NewGuid());


            repository.Add(portfolio);

            _PortfolioAccessor = new TestPortfolioAccessor(portfolio);
        }
    }
}
