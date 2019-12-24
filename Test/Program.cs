using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.EventStore.MongoDB;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("TST", "Test", false, AssetCategory.AustralianStocks);
            stock1.ChangeProperties(new Date(2019, 06, 01), "TST", "New Test", AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("TST2", "Test2", false, AssetCategory.AustralianStocks);


            var stockResolver = new StockResolver();
            stockResolver.Stocks.Add(stock1);
            stockResolver.Stocks.Add(stock2);

            var events = stock1.FetchEvents();


            var eventStore = new MongodbEventStore("mongodb://52.62.34.156:27017", "Test");

            var eventStream = eventStore.GetEventStream("Stocks");

            eventStream.Add(stock1.Id, "stock", events);


            var portfolio = new Portfolio(Guid.NewGuid(), stockResolver);


            Console.WriteLine("Hello World!");
        }
    }

    class StockResolver : IStockResolver
    {
        public List<Stock> Stocks = new List<Stock>();
        public Stock GetStock(Guid id)
        {
            return Stocks.FirstOrDefault(x => x.Id == id);
        }

    }
}
