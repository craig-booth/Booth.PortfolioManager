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

namespace Booth.PortfolioManager.Web.Test.DataImporters
{
    public class HistoricalPriceImporterTests
    {
        [Fact]
        public async Task ImportNoDataReturned()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var priceHistory = mockRepository.Create<IStockPriceHistory>();
            priceHistory.SetupGet(x => x.LatestDate).Returns(new Date(2015, 01, 01));

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(2010, 01, 01), false, AssetCategory.InternationalStocks);
            stock.SetPriceHistory(priceHistory.Object);

            var cancellationToken = new CancellationToken();
            var prices = new DataServices.StockPrice[] { };
            var dataService = mockRepository.Create<IHistoricalStockPriceService>();
            dataService.Setup(x => x.GetHistoricalPriceData("ABC", new DateRange(new Date(2015, 01, 02), Date.Today.AddDays(-1)),  cancellationToken)).Returns(Task<IEnumerable<DataServices.StockPrice>>.FromResult(prices.AsEnumerable()));

            var stockQuery = mockRepository.Create<IStockQuery>();
            stockQuery.Setup(x => x.All()).Returns(new[] { stock });

            var stockService = mockRepository.Create<IStockService>();

            var tradingCalendar = mockRepository.Create<ITradingCalendar>();
            tradingCalendar.Setup(x => x.IsTradingDay(It.IsAny<Date>())).Returns(true);

            var logger = mockRepository.Create<ILogger<HistoricalPriceImporter>>(MockBehavior.Loose);

            var importer = new HistoricalPriceImporter(stockQuery.Object, stockService.Object, tradingCalendar.Object, dataService.Object, logger.Object);

            await importer.Import(cancellationToken);

            mockRepository.Verify();
        }

        [Fact]
        public async Task ImportDataReturned()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var priceHistory = mockRepository.Create<IStockPriceHistory>();
            priceHistory.SetupGet(x => x.LatestDate).Returns(new Date(2015, 01, 01));

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(2010, 01, 01), false, AssetCategory.InternationalStocks);
            stock.SetPriceHistory(priceHistory.Object);

            var cancellationToken = new CancellationToken();
            var prices = new DataServices.StockPrice[] { new DataServices.StockPrice("ABC", Date.Today.AddDays(-1), 0.10m) };
            var dataService = mockRepository.Create<IHistoricalStockPriceService>();
            dataService.Setup(x => x.GetHistoricalPriceData("ABC", new DateRange(new Date(2015, 01, 02), Date.Today.AddDays(-1)), cancellationToken)).Returns(Task<IEnumerable<DataServices.StockPrice>>.FromResult(prices.AsEnumerable()));

            var stockQuery = mockRepository.Create<IStockQuery>();
            stockQuery.Setup(x => x.All()).Returns(new[] { stock });

            var stockService = mockRepository.Create<IStockService>();
            IEnumerable<Domain.Stocks.StockPrice> savedPrices = null;
            stockService.Setup(x => x.UpdateClosingPrices(stock.Id, It.IsAny<IEnumerable<Domain.Stocks.StockPrice>>())).Returns(ServiceResult.Ok()).Callback<Guid,IEnumerable<Domain.Stocks.StockPrice>>((a, b) => savedPrices = b).Verifiable();

            var tradingCalendar = mockRepository.Create<ITradingCalendar>();
            tradingCalendar.Setup(x => x.IsTradingDay(It.IsAny<Date>())).Returns(true);

            var logger = mockRepository.Create<ILogger<HistoricalPriceImporter>>(MockBehavior.Loose);

            var importer = new HistoricalPriceImporter(stockQuery.Object, stockService.Object, tradingCalendar.Object, dataService.Object, logger.Object);

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

            var priceHistory = mockRepository.Create<IStockPriceHistory>();
            priceHistory.SetupGet(x => x.LatestDate).Returns(new Date(2015, 01, 01));

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(2010, 01, 01), false, AssetCategory.InternationalStocks);
            stock.SetPriceHistory(priceHistory.Object);

            var cancellationToken = new CancellationToken();
            var prices = new DataServices.StockPrice[] { };
            var dataService = mockRepository.Create<IHistoricalStockPriceService>();
            dataService.Setup(x => x.GetHistoricalPriceData("ABC", new DateRange(new Date(2015, 01, 02), Date.Today.AddDays(-1)), cancellationToken)).Returns(Task<IEnumerable<DataServices.StockPrice>>.FromResult(prices.AsEnumerable()));

            var stockQuery = mockRepository.Create<IStockQuery>();
            stockQuery.Setup(x => x.All()).Returns(new[] { stock });

            var stockService = mockRepository.Create<IStockService>();
            var prices2 = new Domain.Stocks.StockPrice[] { new Domain.Stocks.StockPrice(Date.Today.AddDays(-1), 0.10m) };
            stockService.Setup(x => x.UpdateClosingPrices(stock.Id, prices2.AsEnumerable()));

            var tradingCalendar = mockRepository.Create<ITradingCalendar>();
            tradingCalendar.Setup(x => x.IsTradingDay(It.IsAny<Date>())).Returns(true);

            var importer = new HistoricalPriceImporter(stockQuery.Object, stockService.Object, tradingCalendar.Object, dataService.Object, null);

            await importer.Import(cancellationToken);

            mockRepository.Verify();
        } 
    }
}
