using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using Xunit;
using Moq;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.Domain.TradingCalendars;
using Booth.PortfolioManager.DataServices;
using Booth.PortfolioManager.Web.DataImporters;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Web.Utilities;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Web.Test.DataImporters
{
    public class HistoricalPriceImporterTests
    {
        [Fact]
        public async Task ImportNoDataReturned()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(2010, 01, 01), false, AssetCategory.InternationalStocks);

            var priceRetriever = mockRepository.Create<IStockPriceRetriever>();
            priceRetriever.Setup(x => x.GetLatestPrice(stock.Id)).Returns(new Domain.Stocks.StockPrice(new Date(2015, 01, 01), 0.10m));

            var stockQuery = mockRepository.Create<IStockQuery>();
            stockQuery.Setup(x => x.All()).Returns(new[] { stock });

            var stockService = mockRepository.Create<IStockService>();

            var tradingCalendar = new TradingCalendar(TradingCalendarIds.ASX);
            var tradingCalendarRepository = mockRepository.Create<ITradingCalendarRepository>();
            tradingCalendarRepository.Setup(x => x.GetAsync(TradingCalendarIds.ASX)).Returns(Task.FromResult<TradingCalendar>(tradingCalendar)); 

            var cancellationToken = new CancellationToken();
            var prices = new DataServices.StockPrice[] { };
            var dataService = mockRepository.Create<IHistoricalStockPriceService>();
            dataService.Setup(x => x.GetHistoricalPriceData("ABC", new DateRange(new Date(2015, 01, 02), tradingCalendar.PreviousTradingDay(Date.Today.AddDays(-1))), cancellationToken)).Returns(Task<IEnumerable<DataServices.StockPrice>>.FromResult(prices.AsEnumerable()));

            var logger = mockRepository.Create<ILogger<HistoricalPriceImporter>>(MockBehavior.Loose);

            var importer = new HistoricalPriceImporter(stockQuery.Object, stockService.Object, tradingCalendarRepository.Object, dataService.Object, priceRetriever.Object, logger.Object);

            await importer.Import(cancellationToken);

            mockRepository.Verify();
        }

        [Fact]
        public async Task ImportDataReturned()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(2010, 01, 01), false, AssetCategory.InternationalStocks);

            var priceRetriever = mockRepository.Create<IStockPriceRetriever>();
            priceRetriever.Setup(x => x.GetLatestPrice(stock.Id)).Returns(new Domain.Stocks.StockPrice(new Date(2015, 01, 01), 0.10m));

            var stockQuery = mockRepository.Create<IStockQuery>();
            stockQuery.Setup(x => x.All()).Returns(new[] { stock });

            var stockService = mockRepository.Create<IStockService>();
            IEnumerable<Domain.Stocks.StockPrice> savedPrices = null;
            stockService.Setup(x => x.UpdateClosingPricesAsync(stock.Id, It.IsAny<IEnumerable<Domain.Stocks.StockPrice>>())).Returns(Task.FromResult<ServiceResult>(ServiceResult.Ok())).Callback<Guid,IEnumerable<Domain.Stocks.StockPrice>>((a, b) => savedPrices = b).Verifiable();

            var tradingCalendar = new TradingCalendar(TradingCalendarIds.ASX);
            var tradingCalendarRepository = mockRepository.Create<ITradingCalendarRepository>();
            tradingCalendarRepository.Setup(x => x.GetAsync(TradingCalendarIds.ASX)).Returns(Task.FromResult<TradingCalendar>(tradingCalendar));

            var logger = mockRepository.Create<ILogger<HistoricalPriceImporter>>(MockBehavior.Loose);

            var cancellationToken = new CancellationToken();
            var prices = new DataServices.StockPrice[] { new DataServices.StockPrice("ABC", Date.Today.AddDays(-1), 0.10m) };
            var dataService = mockRepository.Create<IHistoricalStockPriceService>();
            dataService.Setup(x => x.GetHistoricalPriceData("ABC", new DateRange(new Date(2015, 01, 02), tradingCalendar.PreviousTradingDay(Date.Today.AddDays(-1))), cancellationToken)).Returns(Task<IEnumerable<DataServices.StockPrice>>.FromResult(prices.AsEnumerable()));

            var importer = new HistoricalPriceImporter(stockQuery.Object, stockService.Object, tradingCalendarRepository.Object, dataService.Object, priceRetriever.Object, logger.Object);

            await importer.Import(cancellationToken);

            savedPrices.Should().Equal(new[]
            {
                new Domain.Stocks.StockPrice(Date.Today.AddDays(-1), 0.10m)
            });

            mockRepository.Verify();
        }


        [Fact]
        public async Task ImportNoLogger()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(2010, 01, 01), false, AssetCategory.InternationalStocks);

            var priceRetriever = mockRepository.Create<IStockPriceRetriever>();
            priceRetriever.Setup(x => x.GetLatestPrice(stock.Id)).Returns(new Domain.Stocks.StockPrice(new Date(2015, 01, 01), 0.10m));

            var stockQuery = mockRepository.Create<IStockQuery>();
            stockQuery.Setup(x => x.All()).Returns(new[] { stock });

            var stockService = mockRepository.Create<IStockService>();
            var prices2 = new Domain.Stocks.StockPrice[] { new Domain.Stocks.StockPrice(Date.Today.AddDays(-1), 0.10m) };
            stockService.Setup(x => x.UpdateClosingPricesAsync(stock.Id, prices2.AsEnumerable()));

            var tradingCalendar = new TradingCalendar(TradingCalendarIds.ASX);
            var tradingCalendarRepository = mockRepository.Create<ITradingCalendarRepository>();
            tradingCalendarRepository.Setup(x => x.GetAsync(TradingCalendarIds.ASX)).Returns(Task.FromResult<TradingCalendar>(tradingCalendar));

            var cancellationToken = new CancellationToken();
            var prices = new DataServices.StockPrice[] { };
            var dataService = mockRepository.Create<IHistoricalStockPriceService>();
            dataService.Setup(x => x.GetHistoricalPriceData("ABC", new DateRange(new Date(2015, 01, 02), tradingCalendar.PreviousTradingDay(Date.Today.AddDays(-1))), cancellationToken)).Returns(Task<IEnumerable<DataServices.StockPrice>>.FromResult(prices.AsEnumerable()));

            var importer = new HistoricalPriceImporter(stockQuery.Object, stockService.Object, tradingCalendarRepository.Object, dataService.Object, priceRetriever.Object, null);

            await importer.Import(cancellationToken);

            mockRepository.Verify();
        } 
    }
}
