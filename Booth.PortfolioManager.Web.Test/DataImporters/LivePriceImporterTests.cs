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
    public class LivePriceImporterTests
    {

        [Fact]
        public async Task ImportNoDataReturned()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(2010, 01, 01), false, AssetCategory.InternationalStocks);

            var cancellationToken = new CancellationToken();
            var prices = new DataServices.StockPrice[] { };
            var dataService = mockRepository.Create<ILiveStockPriceService>();
            dataService.Setup(x => x.GetMultiplePrices(new[] { "ABC" }, cancellationToken)).Returns(Task<IEnumerable<DataServices.StockPrice>>.FromResult(prices.AsEnumerable()));

            var stockQuery = mockRepository.Create<IStockQuery>();
            stockQuery.Setup(x => x.All(Date.Today)).Returns(new[] { stock });

            var stockService = mockRepository.Create<IStockService>();

            var logger = mockRepository.Create<ILogger<LivePriceImporter>>(MockBehavior.Loose);

            var importer = new LivePriceImporter(stockQuery.Object, stockService.Object,  dataService.Object, logger.Object);

            await importer.Import(cancellationToken);

            mockRepository.Verify();
        }

        [Fact]
        public async Task ImportDataReturned()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(2010, 01, 01), false, AssetCategory.InternationalStocks);

            var cancellationToken = new CancellationToken();
            var prices = new DataServices.StockPrice[] { new DataServices.StockPrice("ABC", Date.Today, 0.10m) };
            var dataService = mockRepository.Create<ILiveStockPriceService>();
            dataService.Setup(x => x.GetMultiplePrices(new[] { "ABC" }, cancellationToken)).Returns(Task<IEnumerable<DataServices.StockPrice>>.FromResult(prices.AsEnumerable()));

            var stockQuery = mockRepository.Create<IStockQuery>();
            stockQuery.Setup(x => x.All(Date.Today)).Returns(new[] { stock });
            stockQuery.Setup(x => x.Get("ABC", Date.Today)).Returns(stock);

            var stockService = mockRepository.Create<IStockService>();
            stockService.Setup(x => x.UpdateCurrentPrice(stock.Id, 0.10m)).Returns(ServiceResult.Ok()).Verifiable();

            var logger = mockRepository.Create<ILogger<LivePriceImporter>>(MockBehavior.Loose);

            var importer = new LivePriceImporter(stockQuery.Object, stockService.Object, dataService.Object, logger.Object);

            await importer.Import(cancellationToken);

            mockRepository.Verify();
        }


        [Fact]
        public async Task ImportNoLogger()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(2010, 01, 01), false, AssetCategory.InternationalStocks);

            var cancellationToken = new CancellationToken();
            var prices = new DataServices.StockPrice[] { };
            var dataService = mockRepository.Create<ILiveStockPriceService>();
            dataService.Setup(x => x.GetMultiplePrices(new[] { "ABC" }, cancellationToken)).Returns(Task<IEnumerable<DataServices.StockPrice>>.FromResult(prices.AsEnumerable()));

            var stockQuery = mockRepository.Create<IStockQuery>();
            stockQuery.Setup(x => x.All(Date.Today)).Returns(new[] { stock });
            stockQuery.Setup(x => x.Get("ABC", Date.Today)).Returns(stock);

            var stockService = mockRepository.Create<IStockService>();
            stockService.Setup(x => x.UpdateCurrentPrice(stock.Id, 0.10m));

            var importer = new LivePriceImporter(stockQuery.Object, stockService.Object, dataService.Object, null);

            await importer.Import(cancellationToken);

            mockRepository.Verify();
        }
    }
}
