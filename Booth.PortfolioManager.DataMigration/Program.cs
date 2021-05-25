using System;
using System.Collections.Generic;

using Booth.Common;
using Booth.EventStore.MongoDB;
using Booth.PortfolioManager.Domain.Users;
using Booth.PortfolioManager.Domain.TradingCalendars;
using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.Domain.Stocks;


namespace Booth.PortfolioManager.DataMigration
{
    class Program
    {
        static void Main(string[] args)
        {
            var x = new TradingCalendar(Guid.Empty);

        //    var eventStore = new MongodbEventStore("mongodb://192.168.1.93:27017", "PortfolioManager");
            var database = new PortfolioManagerDatabase("mongodb://192.168.1.93:27017", "PortfolioManager2");

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "Test", new Date(1974, 04, 10), false, AssetCategory.AustralianStocks);
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


            // MigrateUsers(eventStore, database);

            //   MigrateTradingCalendars(eventStore, database);

            //  MigrateStocks(eventStore, database);

        //    TestStock(stock, database);

            TestTradingCalendar(database);

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


                newUser.RemoveAdministratorPrivilage();
                newUser.AddAdministratorPrivilage();
                repository.Update(newUser);
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


            var existingCalendar = repository.Get(calendar.Id);
            existingCalendar.SetNonTradingDays(2020, new[] { new NonTradingDay(new Common.Date(2020, 04, 10), "Birthday" )});
            repository.UpdateYear(existingCalendar, 2020);

            existingCalendar.SetNonTradingDays(2021, new[] { new NonTradingDay(new Common.Date(2021, 04, 10), "Birthday2") });
            repository.UpdateYear(existingCalendar, 2021);

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

                

              //  existingStock.CorporateActions.AddCapitalReturn(Guid.NewGuid(), new Date(1974, 4, 10), "Birthday", new Date(1974, 4, 10), 100.00m);
              //  repository.AddCorporateAction(???);                 
            }


        }

        static void TestStock(Stock stock, PortfolioManagerDatabase database)
        {
            var repository = new StockRepository(database);

            repository.Add(stock);

            repository.Update(stock);

            repository.UpdateProperties(stock, new Date(2000, 01, 01));

            var newStock = repository.Get(stock.Id);
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

    }
}
