﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

using Xunit;
using Moq;
using FluentAssertions;
using FluentAssertions.Execution;

using Booth.Common;
using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Web.Utilities;
using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Web.Test.Services
{
    public class StockServiceTests : IDisposable
    {
        private CultureInfo _CultureBackup;
        public StockServiceTests()
        {
            _CultureBackup = Thread.CurrentThread.CurrentCulture;

            var testCulture = new CultureInfo("en-AU");
            testCulture.DateTimeFormat.ShortDatePattern = "d/M/yyyy";

            Thread.CurrentThread.CurrentCulture = testCulture;
        }
        public void Dispose()
        {
            Thread.CurrentThread.CurrentCulture = _CultureBackup;
        }

        [Fact]
        public async Task ListStockDuplicateId()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();

            var stockQuery = mockRepository.Create<IStockQuery>();
            var stockRepository = mockRepository.Create<IStockRepository>();
            stockRepository.Setup(x => x.GetAsync(id)).Returns(Task.FromResult<Stock>(new Stock(id)));
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery.Object, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var result = await service.ListStockAsync(id, "XYZ", "XYZ Pty Ltd", new Date(2000, 01, 01), true, AssetCategory.AustralianFixedInterest);

            result.Should().HaveErrorStatus().WithError("A stock with id " + id.ToString() + " already exists");

            mockRepository.Verify();
        }

        [Fact]
        public async Task ListStockDuplicateAsxCode()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();

            var stock = new Stock(Guid.NewGuid());
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();
            stockRepository.Setup(x => x.GetAsync(id)).Returns(Task.FromResult<Stock>(default(Stock)));
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var result = await service.ListStockAsync(id, "XYZ", "XYZ Pty Ltd", new Date(2000, 01, 01), true, AssetCategory.AustralianFixedInterest);

            result.Should().HaveErrorStatus().WithError("A stock already exists with the code XYZ on 1/1/2000");

            mockRepository.Verify();
        }

        [Fact]
        public async Task ListStockDuplicateAsxCodeInThePast()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();

            var stock = new Stock(Guid.NewGuid());
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);
            stock.DeList(new Date(1999, 06, 30));

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();
            stockRepository.Setup(x => x.GetAsync(id)).Returns(Task.FromResult<Stock>(default(Stock)));
            stockRepository.Setup(x => x.AddAsync(It.IsAny<Stock>())).Returns(Task.CompletedTask).Verifiable();
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            stockPriceHistoryRepository.Setup(x => x.AddAsync(It.IsAny<StockPriceHistory>())).Returns(Task.CompletedTask).Verifiable();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var result = await service.ListStockAsync(id, "XYZ", "XYZ Pty Ltd", new Date(2000, 01, 01), true, AssetCategory.AustralianFixedInterest);

            result.Should().HaveOkStatus();

            mockRepository.Verify();
        }

        [Fact]
        public async Task ListStock()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var listingDate = new Date(2000, 01, 01);

            Stock stock = null;

            var stockCache = new EntityCache<Stock>();
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();
            stockRepository.Setup(x => x.GetAsync(id)).Returns(Task.FromResult<Stock>(default(Stock)));
            stockRepository.Setup(x => x.AddAsync(It.IsAny<Stock>())).Returns(Task.CompletedTask).Callback<Stock>(x => stock = x).Verifiable();
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            stockPriceHistoryRepository.Setup(x => x.AddAsync(It.IsAny<StockPriceHistory>())).Returns(Task.CompletedTask).Verifiable();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var result = await service.ListStockAsync(id, "XYZ", "XYZ Pty Ltd", listingDate, true, AssetCategory.AustralianFixedInterest);

            using (new AssertionScope())
            {
                result.Should().HaveOkStatus();

                stock.Should().BeEquivalentTo(new { Id = id, EffectivePeriod = new DateRange(listingDate, Date.MaxValue) });
                stock.Properties[listingDate].Should().BeEquivalentTo(new StockProperties("XYZ", "XYZ Pty Ltd", AssetCategory.AustralianFixedInterest));
            }

            mockRepository.Verify();
        }

        [Fact]
        public async Task DelistStockInvalidId()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();

            var stockCache = new EntityCache<Stock>();
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var result = await service.DelistStockAsync(id, new Date(2000, 01, 01));

            result.Should().HaveNotFoundStatus();

            mockRepository.Verify();
        }


        [Fact]
        public async Task DelistStockNotListed()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);
            var id = Guid.NewGuid();

            var stock = new Stock(id);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var result = await service.DelistStockAsync(id, new Date(2000, 01, 01));

            result.Should().HaveErrorStatus().WithError("Stock is not listed");

            mockRepository.Verify();
        }

        [Fact]
        public async Task DelistStockAlreadyDelisted()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);
            var id = Guid.NewGuid();

            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);
            stock.DeList(new Date(1999, 06, 30));

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var result = await service.DelistStockAsync(id, new Date(2000, 01, 01));

            result.Should().HaveErrorStatus().WithError("Stock has already been delisted");

            mockRepository.Verify();
        }

        [Fact]
        public async Task DeListStock()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);
            var id = Guid.NewGuid();

            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();
            stockRepository.Setup(x => x.UpdateAsync(stock)).Returns(Task.CompletedTask).Verifiable();
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var result = await service.DelistStockAsync(id, new Date(2000, 01, 01));

            using (new AssertionScope())
            {
                result.Should().HaveOkStatus();

                stock.EffectivePeriod.ToDate.Should().Be(new Date(2000, 01, 01));
            }

            mockRepository.Verify();
        }

        [Fact]
        public async Task ChangeStockInvalidId()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();

            var stockCache = new EntityCache<Stock>();
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var result = await service.ChangeStockAsync(id, new Date(2000, 01, 01), "ABC", "ABC Pty Ltd", AssetCategory.AustralianFixedInterest);

            result.Should().HaveNotFoundStatus();

            mockRepository.Verify();
        }

        [Fact]
        public async Task ChangeStockNotListed()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var stock = new Stock(id);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var result = await service.ChangeStockAsync(id, new Date(2000, 01, 01), "ABC", "ABC Pty Ltd", AssetCategory.AustralianFixedInterest);

            result.Should().HaveErrorStatus().WithError("Stock is not listed");

            mockRepository.Verify();
        }

        [Fact]
        public async Task ChangeStockDelisted()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);
            stock.DeList(new Date(1999, 01, 01));

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var result = await service.ChangeStockAsync(id, new Date(2000, 01, 01), "ABC", "ABC Pty Ltd", AssetCategory.AustralianFixedInterest);

            result.Should().HaveErrorStatus().WithError("Stock is delisted");

            mockRepository.Verify();
        }

        [Fact]
        public async Task ChangeStockWithLaterChangeExisting()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);
            stock.ChangeProperties(new Date(2002, 01, 01), "XYZ", "New Name", AssetCategory.AustralianFixedInterest);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var result = await service.ChangeStockAsync(id, new Date(2000, 01, 01), "ABC", "ABC Pty Ltd", AssetCategory.AustralianFixedInterest);

            result.Should().HaveErrorStatus().WithError("A later change has been made on 1/1/2002");

            mockRepository.Verify();
        }

        [Fact]
        public async Task ChangeStockDuplicateAsxCode()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("ABC", "Different stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            stockCache.Add(stock2);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var result = await service.ChangeStockAsync(id, new Date(2000, 01, 01), "ABC", "ABC Pty Ltd", AssetCategory.AustralianFixedInterest);

            result.Should().HaveErrorStatus().WithError("A stock already exists with code ABC on 1/1/2000");

            mockRepository.Verify();
        }

        [Fact]
        public async Task ChangeStockDuplicateAsxCodeInThePast()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("ABC", "Different stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);
            stock2.DeList(new Date(1999, 01, 01));

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            stockCache.Add(stock2);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();
            stockRepository.Setup(x => x.UpdatePropertiesAsync(stock, new Date(2000, 01, 01))).Returns(Task.CompletedTask).Verifiable();
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var result = await service.ChangeStockAsync(id, new Date(2000, 01, 01), "ABC", "ABC Pty Ltd", AssetCategory.AustralianFixedInterest);

            result.Should().HaveOkStatus();

            mockRepository.Verify();
        }

        [Fact]
        public async Task ChangeStock()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);


            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();
            stockRepository.Setup(x => x.UpdatePropertiesAsync(stock, new Date(2000, 01, 01))).Returns(Task.CompletedTask).Verifiable();
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var result = await service.ChangeStockAsync(id, new Date(2000, 01, 01), "ABC", "ABC Pty Ltd", AssetCategory.AustralianProperty);

            using (new AssertionScope())
            {
                result.Should().HaveOkStatus();

                stock.Properties[new Date(2000, 01, 01)].Should().BeEquivalentTo(new { AsxCode = "ABC", Name = "ABC Pty Ltd", Category = AssetCategory.AustralianProperty });
            }

            mockRepository.Verify();
        }

        [Fact]
        public void UpdateCurrentPriceInvalidId()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();

            var stockCache = new EntityCache<Stock>();
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var result = service.UpdateCurrentPrice(id, 10.00m);

            result.Should().HaveNotFoundStatus();

            mockRepository.Verify();
        }

        [Fact]
        public void UpdateCurrentPriceStockNotListed()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var stock = new Stock(id);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var result = service.UpdateCurrentPrice(id, 10.00m);

            result.Should().HaveErrorStatus("Stock is not listed");

            mockRepository.Verify();
        }

        [Fact]
        public void UpdateCurrentPriceStockDelisted()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);
            stock.DeList(new Date(2000, 01, 01));

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var result = service.UpdateCurrentPrice(id, 10.00m);

            result.Should().HaveErrorStatus("Stock is delisted");

            mockRepository.Verify();
        }

        [Fact]
        public void UpdateCurrentPriceNegativePrice()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var result = service.UpdateCurrentPrice(id, -10.00m);
            
            result.Should().HaveErrorStatus("Price is negative");

            mockRepository.Verify();
        }

        [Fact]
        public void UpdateCurrentPrice()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();

            var stockPriceHistory = new StockPriceHistory(id);
            var stockPriceCache = new EntityCache<StockPriceHistory>();
            stockPriceCache.Add(stockPriceHistory);
            var stockPriceRetreiver = new StockPriceRetriever(stockPriceCache);
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            stockPriceHistoryRepository.Setup(x => x.GetAsync(id)).Returns(Task.FromResult(stockPriceHistory));

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetreiver, stockPriceCache);

            var result = service.UpdateCurrentPrice(id, 10.00m);

            using (new AssertionScope())
            {
                result.Should().HaveOkStatus();
                stockPriceRetreiver.GetPrice(stock.Id, Date.Today).Should().Be(10.00m);
            }

            mockRepository.Verify();
        }

        [Fact]
        public async Task UpdateClosingPricesInvalidId()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();

            var stockCache = new EntityCache<Stock>();
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var result = await service.UpdateClosingPricesAsync(id, new StockPrice[] { });

            result.Should().HaveNotFoundStatus();

            mockRepository.Verify();
        }

        [Fact]
        public async Task UpdateClosingPricesStockNotListed()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var stock = new Stock(id);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var result = await service.UpdateClosingPricesAsync(id, new StockPrice[] { });

            result.Should().HaveErrorStatus().WithError("Stock is not listed");

            mockRepository.Verify();
        }

        [Fact]
        public async Task UpdateClosingPricesStockDelisted()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);
            stock.DeList(new Date(1999, 01, 01));

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();

            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var prices = new StockPrice[]
            {
                new StockPrice(new Date(1998, 01, 01), 0.10m),
                new StockPrice(new Date(1998, 12, 01), 0.20m),
                new StockPrice(new Date(1999, 01, 02), 0.30m)
            };
            var result = await service.UpdateClosingPricesAsync(id, prices);

            result.Should().HaveErrorStatus("Stock is no listed on 2/01/1999");

            mockRepository.Verify();
        }

        [Fact]
        public async Task UpdateClosingPricesOnDelistingDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);
            stock.DeList(new Date(1999, 01, 01));

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();

            var stockPriceHistory = new StockPriceHistory(id);
            var stockPriceCache = new EntityCache<StockPriceHistory>();
            stockPriceCache.Add(stockPriceHistory);
            var stockPriceRetreiver = new StockPriceRetriever(stockPriceCache);
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            stockPriceHistoryRepository.Setup(x => x.GetAsync(id)).Returns(Task.FromResult(stockPriceHistory));
            stockPriceHistoryRepository.Setup(x => x.UpdatePricesAsync(stockPriceHistory, new DateRange(new Date(1998, 01, 01), new Date(1999, 01, 01)))).Returns(Task.CompletedTask).Verifiable();

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetreiver, stockPriceCache);

            var prices = new StockPrice[]
            {
                new StockPrice(new Date(1998, 01, 01), 0.10m),
                new StockPrice(new Date(1998, 12, 01), 0.20m),
                new StockPrice(new Date(1999, 01, 01), 0.30m)
            };
            var result = await service.UpdateClosingPricesAsync(id, prices);

            result.Should().HaveOkStatus();

            mockRepository.Verify();
        }

        [Fact]
        public async Task UpdateClosingPricesNegativePrice()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var prices = new StockPrice[]
            {
                new StockPrice(new Date(2000, 01, 01), 0.10m),
                new StockPrice(new Date(2000, 01, 02), -0.20m),
                new StockPrice(new Date(2000, 01, 03), 0.30m)
            };
            var result = await service.UpdateClosingPricesAsync(id, prices);

            result.Should().HaveErrorStatus("Closing price on 2/01/2000 is negative");

            mockRepository.Verify();
        }

        [Fact]
        public async Task UpdateClosingPrices()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();

            var stockPriceHistory = new StockPriceHistory(id);
            var stockPriceCache = new EntityCache<StockPriceHistory>();
            stockPriceCache.Add(stockPriceHistory);
            var stockPriceRetreiver = new StockPriceRetriever(stockPriceCache);
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            stockPriceHistoryRepository.Setup(x => x.GetAsync(id)).Returns(Task.FromResult(stockPriceHistory));
            stockPriceHistoryRepository.Setup(x => x.UpdatePricesAsync(stockPriceHistory, new DateRange(new Date(2000, 01, 01), new Date(2000, 01, 03)))).Returns(Task.CompletedTask).Verifiable();
            
            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetreiver, stockPriceCache);

            var prices = new StockPrice[]
            {
                new StockPrice(new Date(2000, 01, 01), 0.10m),
                new StockPrice(new Date(2000, 01, 02), 0.20m),
                new StockPrice(new Date(2000, 01, 03), 0.30m)
            };
            var result = await service.UpdateClosingPricesAsync(id, prices);

            using (new AssertionScope())
            {
                result.Should().HaveOkStatus();

                stockPriceRetreiver.GetPrice(stock.Id, new Date(2000, 01, 01)).Should().Be(0.10m);
                stockPriceRetreiver.GetPrice(stock.Id, new Date(2000, 01, 02)).Should().Be(0.20m);
                stockPriceRetreiver.GetPrice(stock.Id, new Date(2000, 01, 03)).Should().Be(0.30m);     
            }

            mockRepository.Verify();
        }

        [Fact]
        public async Task ChangeDividendRulesInvalidId()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();

            var stockCache = new EntityCache<Stock>();
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var result = await service.ChangeDividendRulesAsync(id, new Date(2000, 01, 01), 0.30m, RoundingRule.Round, true, DrpMethod.Round);
            
            result.Should().HaveNotFoundStatus();

            mockRepository.Verify();
        }

        [Fact]
        public async Task ChangeDividendRulesStockNotActive()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();

            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);
            stock.DeList(new Date(1999, 06, 30));

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var result = await service.ChangeDividendRulesAsync(id, new Date(2000, 01, 01), 0.30m, RoundingRule.Round, true, DrpMethod.Round);

            result.Should().HaveErrorStatus().WithError("Stock is not active at 1/1/2000");

            mockRepository.Verify();
        }

        [Fact]
        public async Task ChangeDividendRulesTaxRateNegative()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();

            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var result = await service.ChangeDividendRulesAsync(id, new Date(2000, 01, 01), -0.30m, RoundingRule.Round, true, DrpMethod.Round);

            result.Should().HaveErrorStatus().WithError("Company tax rate must be between 0 and 1");

            mockRepository.Verify();
        }

        [Fact]
        public async Task ChangeDividendRulesTaxRateGreaterThan100Percent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();

            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var result = await service.ChangeDividendRulesAsync(id, new Date(2000, 01, 01), 1.30m, RoundingRule.Round, true, DrpMethod.Round);

            result.Should().HaveErrorStatus().WithError("Company tax rate must be between 0 and 1");

            mockRepository.Verify();
        }

        [Fact]
        public async Task ChangeDividendRules()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();

            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IStockRepository>();
            stockRepository.Setup(x => x.UpdateDividendRulesAsync(stock, new Date(2000, 01, 01))).Returns(Task.CompletedTask).Verifiable();
            var stockPriceHistoryRepository = mockRepository.Create<IStockPriceRepository>();
            var stockPriceCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceRetriever = new StockPriceRetriever(stockPriceCache.Object);

            var service = new StockService(stockQuery, stockRepository.Object, stockPriceHistoryRepository.Object, stockPriceRetriever, stockPriceCache.Object);

            var result = await service.ChangeDividendRulesAsync(id, new Date(2000, 01, 01), 0.50m, RoundingRule.Round, true, DrpMethod.Round);

            using (new AssertionScope())
            {
                result.Should().HaveOkStatus();
                var dividendRules = stock.DividendRules[new Date(2000, 01, 01)];
                dividendRules.Should().BeEquivalentTo(new { CompanyTaxRate = 0.50m, DividendRoundingRule = RoundingRule.Round, DrpActive = true, DrpMethod = DrpMethod.Round });
            }

            mockRepository.Verify();
        }
    }

}
