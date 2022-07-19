using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;
using Booth.EventStore.MongoDB;
using Booth.PortfolioManager.Domain.Users;
using Booth.PortfolioManager.Domain.TradingCalendars;
using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;


namespace Booth.PortfolioManager.DataMigration
{
    class Program
    {
        static void Main(string[] args)
        {
            var x = new TradingCalendar(Guid.Empty);

            var stockResolver = new StockResolver();
            var factory = new PortfolioFactory(stockResolver);

            var eventStore = new MongodbEventStore("mongodb://192.168.1.93:27017", "PortfolioManager");
            var database = new PortfolioManagerDatabase("mongodb://192.168.1.93:27017", "PortfolioManager2", factory, stockResolver);

            /*
                        var stock = new StapledSecurity(Guid.NewGuid());
                        stockResolver.Add(stock);

                        var childStocks = new[] {
                            new StapledSecurityChild("CH1", "Child 1", false),
                            new StapledSecurityChild("CH2", "Child 2", true)
                        };

                        stock.List("ABC", "Test", new Date(1974, 04, 10), AssetCategory.AustralianStocks, childStocks);

                        stock.SetRelativeNTAs(new Date(1974, 04, 10), new[] { 0.45m, 0.55m });
                        stock.SetRelativeNTAs(new Date(1975, 01, 01), new[] { 0.50m, 0.50m });
                        stock.SetRelativeNTAs(new Date(1976, 01, 01), new[] { 0.60m, 0.40m });

                        stock.ChangeProperties(new Date(2000, 01, 01), "DEF", "New Name", AssetCategory.AustralianProperty);
                        stock.ChangeDividendRules(new Date(2001, 02, 03), 0.45m, RoundingRule.Truncate, true, DrpMethod.RoundDown);

                        stock.CorporateActions.AddCapitalReturn(Guid.NewGuid(), new Date(2002, 06, 01), "test", new Date(2002, 06, 03), 6.00m);
                        stock.CorporateActions.AddSplitConsolidation(Guid.NewGuid(), new Date(2004, 06, 01), "test3", 1, 2);

                        var resultingStocks = new List<Domain.CorporateActions.Transformation.ResultingStock>();
                        resultingStocks.Add(new Domain.CorporateActions.Transformation.ResultingStock(Guid.NewGuid(), 1, 2, 0.67m, new Date(2005, 05, 01)));
                        stock.CorporateActions.AddTransformation(Guid.NewGuid(), new Date(2005, 05, 01), "test4", new Date(2005, 05, 02), 5.24m, true, resultingStocks);

                        stock.CorporateActions.StartCompositeAction(Guid.NewGuid(), new Date(2006, 05, 03), "test5")
                            .AddCapitalReturn("test", new Date(2006, 06, 03), 6.00m)
                            .Finish();

                        stock.DeList(new Date(2020, 10, 01));


                        var portfolio = factory.CreatePortfolio(Guid.NewGuid());
                        portfolio.Create("Testing", Guid.NewGuid());
                        portfolio.MakeCashTransaction(new Date(2000, 01, 01), BankAccountTransactionType.Deposit, 100.00m, "Initial deposit", Guid.NewGuid());
                        portfolio.AquireShares(stock.Id, new Date(2001, 01, 01), 100, 0.02m, 9.95m, true, "test", Guid.NewGuid());
            */

         //   MigrateUsers(eventStore, database);
           // MigrateTradingCalendars(eventStore, database);
           //   MigrateStocks(eventStore, database);
              MigrateStockPriceHistory(eventStore, database);

            // TestPortfolio(portfolio, database);





            //   TestStock(stock, database);

            //   TestTradingCalendar(database);

            Console.WriteLine("Hello World!");
        }

        static void MigrateUsers(MongodbEventStore eventStore, PortfolioManagerDatabase database)
        {
            // Load users from Event Store
            var eventStream = eventStore.GetEventStream<User>("Users");
            var eventRepository = new EventStore.Repository<User>(eventStream);

            var repository = new UserRepository(database);

            foreach (var user in eventRepository.All())
            {                
                var newUser = new User(user.Id);
                newUser.Create2(user.UserName, user.Password);
                if (user.Administator)
                    newUser.AddAdministratorPrivilage();

                repository.Add(newUser);
            }


        }

        static void MigrateTradingCalendars(MongodbEventStore eventStore, PortfolioManagerDatabase database)
        {
            // Load users from Event Store
            var eventStream = eventStore.GetEventStream<TradingCalendar>("TradingCalendar");
            var eventRepository = new EventStore.Repository<TradingCalendar>(eventStream);

            var repository = new TradingCalendarRepository(database);

            var calendar = eventRepository.Get(TradingCalendarIds.ASX);
            
            var newCalendar = new TradingCalendar(calendar.Id);

            foreach (var year in calendar.Years)
                newCalendar.SetNonTradingDays(year, calendar.NonTradingDays(year));

            repository.Add(calendar);
        }

        static void MigrateStocks(MongodbEventStore eventStore, PortfolioManagerDatabase database)
        {
            // Load users from Event Store
            var eventStream = eventStore.GetEventStream<Stock>("Stocks");
            var eventRepository = new EventStore.Repository<Stock>(eventStream, new StockEntityFactory());

            var repository = new StockRepository(database);

            foreach (var stock in eventRepository.All())
            {
                var existingStock = repository.Get(stock.Id);

                if (existingStock == null)
                    repository.Add(stock);

             //   var id = Guid.NewGuid();
             //   stock.CorporateActions.AddCapitalReturn(id, new Date(1974, 4, 10), "Birthday", new Date(1974, 4, 10), 100.00m);
              //  repository.AddCorporateAction(stock, id);
             //   break;
            }
        }

        static void MigrateStockPriceHistory(MongodbEventStore eventStore, PortfolioManagerDatabase database)
        {
            // Load users from Event Store
            var eventStream = eventStore.GetEventStream<StockPriceHistory>("StockPriceHistory");
            var eventRepository = new EventStore.Repository<StockPriceHistory>(eventStream);

            var repository = new StockPriceRepository(database);

            foreach (var stockPrices in eventRepository.All())
            {
                var existingStockPrices = repository.Get(stockPrices.Id);

                if (existingStockPrices == null)
                    repository.Add(stockPrices);

             /*   stockPrices.UpdateClosingPrice(new Date(1997, 07, 18), 5.00m);
                repository.UpdatePrice(stockPrices, new Date(1997, 07, 18));

                stockPrices.UpdateClosingPrice(new Date(1997, 07, 20), 15.00m);
                repository.UpdatePrice(stockPrices, new Date(1997, 07, 20));

                stockPrices.UpdateClosingPrice(new Date(1997, 07, 17), 6.00m);
                stockPrices.UpdateClosingPrice(new Date(1997, 07, 21), 5.50m);
                repository.UpdatePrices(stockPrices, new DateRange(new Date(1997, 07, 17), new Date(1997, 07, 21)));
                break; */
            }
        }

        static void TestStock(Stock stock, PortfolioManagerDatabase database)
        {
            var repository = new StockRepository(database);

            repository.Test(stock);

            if (stock is StapledSecurity stapledSecurity)
            {
                stapledSecurity.SetRelativeNTAs(new Date(1976, 01, 01), new[] { 0.35m, 0.65m });
                repository.UpdateRelativeNTAs(stapledSecurity, new Date(1975, 01, 01));

                stapledSecurity.SetRelativeNTAs(new Date(1977, 01, 01), new[] { 0.80m, 0.20m });
                repository.UpdateRelativeNTAs(stapledSecurity, new Date(1976, 01, 01));
            }

            /*
            repository.Add(stock);

            repository.Update(stock);

            repository.UpdateProperties(stock, new Date(2000, 01, 01));

            var newStock = repository.Get(stock.Id); */
        }

        static void TestTradingCalendar(PortfolioManagerDatabase database)
        {
            var repository = new TradingCalendarRepository(database);

            var calendar = new TradingCalendar(Guid.NewGuid());
            calendar.SetNonTradingDays(2020, new[] { new NonTradingDay(new Date(2020, 01, 01), "New Years Day") });
            calendar.SetNonTradingDays(2021, new[] { new NonTradingDay(new Date(2021, 04, 10), "Birthday") });

            repository.Add(calendar);

            calendar.SetNonTradingDays(2021, new[] { new NonTradingDay(new Date(2021, 04, 10), "Birthday"), new NonTradingDay(new Date(2021, 12, 10), "Ryan Birthday") });

            repository.UpdateYear(calendar, 2021);
        }

        static void TestPortfolio(Portfolio portfolio, PortfolioManagerDatabase database)
        {
            var repository = new PortfolioRepository(database);

            repository.Add(portfolio);

            repository.Get(portfolio.Id);

        }

    }

    class StockResolver : IStockResolver
    {
        private Dictionary<Guid, Stock> _Stocks = new Dictionary<Guid, Stock>();

        public void Add(Stock stock)
        {
            _Stocks.Add(stock.Id, stock);
        }

        public IReadOnlyStock GetStock(Guid id)
        {
            return _Stocks[id];
        }

    }
}
