using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

using Xunit;
using Moq;
using FluentAssertions;
using FluentAssertions.Execution;

using Booth.Common;
using Booth.EventStore;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Web.Utilities;

namespace Booth.PortfolioManager.Web.Test.Services
{
    public class StockServiceTests 
    {
        [Fact]
        public void ListStockDuplicateId()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();

            var stockQuery = mockRepository.Create<IStockQuery>();
            var stockCache = mockRepository.Create<IEntityCache<Stock>>();
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            stockRepository.Setup(x => x.Get(id)).Returns(new Stock(id));
            var stockPriceHistoryCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery.Object, stockCache.Object, stockRepository.Object, stockPriceHistoryCache.Object, stockPriceHistoryRepository.Object);

            var result = service.ListStock(id, "XYZ", "XYZ Pty Ltd", new Date(2000, 01, 01), true, AssetCategory.AustralianFixedInterest);

            result.Should().HaveErrorStatus().WithError("A stock with id " + id.ToString() + " already exists");

            mockRepository.Verify();
        }

        [Fact]
        public void ListStockDuplicateAsxCode()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();

            var stock = new Stock(Guid.NewGuid());
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            stockRepository.Setup(x => x.Get(id)).Returns(default(Stock));
            var stockPriceHistoryCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache.Object, stockPriceHistoryRepository.Object);

            var result = service.ListStock(id, "XYZ", "XYZ Pty Ltd", new Date(2000, 01, 01), true, AssetCategory.AustralianFixedInterest);

            result.Should().HaveErrorStatus().WithError("A stock already exists with the code XYZ on 1/01/2000");

            mockRepository.Verify();
        }

        [Fact]
        public void ListStockDuplicateAsxCodeInThePast()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();

            var stock = new Stock(Guid.NewGuid());
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);
            stock.DeList(new Date(1999, 06, 30));

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            stockRepository.Setup(x => x.Get(id)).Returns(default(Stock));
            stockRepository.Setup(x => x.Add(It.IsAny<Stock>())).Verifiable();
            var stockPriceHistoryCache = new EntityCache<StockPriceHistory>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();
            stockPriceHistoryRepository.Setup(x => x.Add(It.IsAny<StockPriceHistory>())).Verifiable();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache, stockPriceHistoryRepository.Object);

            var result = service.ListStock(id, "XYZ", "XYZ Pty Ltd", new Date(2000, 01, 01), true, AssetCategory.AustralianFixedInterest);

            result.Should().HaveOkStatus();

            mockRepository.Verify();
        }

        [Fact]
        public void ListStock()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var listingDate = new Date(2000, 01, 01);

            Stock stock = null;

            var stockCache = new EntityCache<Stock>();
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            stockRepository.Setup(x => x.Get(id)).Returns(default(Stock));
            stockRepository.Setup(x => x.Add(It.IsAny<Stock>())).Callback<Stock>(x => stock = x).Verifiable();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();
            var stockPriceHistoryCache = new EntityCache<StockPriceHistory>();
            stockPriceHistoryRepository.Setup(x => x.Add(It.IsAny<StockPriceHistory>())).Verifiable();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache, stockPriceHistoryRepository.Object);

            var result = service.ListStock(id, "XYZ", "XYZ Pty Ltd", listingDate, true, AssetCategory.AustralianFixedInterest);

            using (new AssertionScope())
            {
                result.Should().HaveOkStatus();

                stock.Should().BeEquivalentTo(new { Id = id, EffectivePeriod = new DateRange(listingDate, Date.MaxValue) });
                stock.Properties[listingDate].Should().BeEquivalentTo(new StockProperties("XYZ", "XYZ Pty Ltd", AssetCategory.AustralianFixedInterest));

                stockCache.Get(id).Should().Be(stock);

                stockPriceHistoryCache.Get(id).Should().NotBeNull();
            }

            mockRepository.Verify();
        }

        [Fact]
        public void DelistStockInvalidId()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();

            var stockCache = new EntityCache<Stock>();
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            var stockPriceHistoryCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache.Object, stockPriceHistoryRepository.Object);

            var result = service.DelistStock(id, new Date(2000, 01, 01));

            result.Should().HaveNotFoundStatus();

            mockRepository.Verify();
        }


        [Fact]
        public void DelistStockNotListed()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);
            var id = Guid.NewGuid();

            var stock = new Stock(id);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            var stockPriceHistoryCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache.Object, stockPriceHistoryRepository.Object);

            var result = service.DelistStock(id, new Date(2000, 01, 01));

            result.Should().HaveErrorStatus().WithError("Stock is not listed");

            mockRepository.Verify();
        }

        [Fact]
        public void DelistStockAlreadyDelisted()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);
            var id = Guid.NewGuid();

            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);
            stock.DeList(new Date(1999, 06, 30));

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            var stockPriceHistoryCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache.Object, stockPriceHistoryRepository.Object);

            var result = service.DelistStock(id, new Date(2000, 01, 01));

            result.Should().HaveErrorStatus().WithError("Stock has already been delisted");

            mockRepository.Verify();
        }

        [Fact]
        public void DeListStock()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);
            var id = Guid.NewGuid();

            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            stockRepository.Setup(x => x.Update(stock)).Verifiable();
            var stockPriceHistoryCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache.Object, stockPriceHistoryRepository.Object);

            var result = service.DelistStock(id, new Date(2000, 01, 01));

            using (new AssertionScope())
            {
                result.Should().HaveOkStatus();

                stock.EffectivePeriod.ToDate.Should().Be(new Date(2000, 01, 01));
            }

            mockRepository.Verify();
        }

        [Fact]
        public void ChangeStockInvalidId()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();

            var stockCache = new EntityCache<Stock>();
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            var stockPriceHistoryCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache.Object, stockPriceHistoryRepository.Object);

            var result = service.ChangeStock(id, new Date(2000, 01, 01), "ABC", "ABC Pty Ltd", AssetCategory.AustralianFixedInterest);

            result.Should().HaveNotFoundStatus();

            mockRepository.Verify();
        }

        [Fact]
        public void ChangeStockNotListed()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var stock = new Stock(id);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            var stockPriceHistoryCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache.Object, stockPriceHistoryRepository.Object);

            var result = service.ChangeStock(id, new Date(2000, 01, 01), "ABC", "ABC Pty Ltd", AssetCategory.AustralianFixedInterest);

            result.Should().HaveErrorStatus().WithError("Stock is not listed");

            mockRepository.Verify();
        }

        [Fact]
        public void ChangeStockDelisted()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);
            stock.DeList(new Date(1999, 01, 01));

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            var stockPriceHistoryCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache.Object, stockPriceHistoryRepository.Object);

            var result = service.ChangeStock(id, new Date(2000, 01, 01), "ABC", "ABC Pty Ltd", AssetCategory.AustralianFixedInterest);

            result.Should().HaveErrorStatus().WithError("Stock is delisted");

            mockRepository.Verify();
        }

        [Fact]
        public void ChangeStockWithLaterChangeExisting()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);
            stock.ChangeProperties(new Date(2002, 01, 01), "XYZ", "New Name", AssetCategory.AustralianFixedInterest);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            var stockPriceHistoryCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache.Object, stockPriceHistoryRepository.Object);

            var result = service.ChangeStock(id, new Date(2000, 01, 01), "ABC", "ABC Pty Ltd", AssetCategory.AustralianFixedInterest);

            result.Should().HaveErrorStatus().WithError("A later change has been made on 1/01/2002");

            mockRepository.Verify();
        }

        [Fact]
        public void ChangeStockDuplicateAsxCode()
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
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            var stockPriceHistoryCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache.Object, stockPriceHistoryRepository.Object);

            var result = service.ChangeStock(id, new Date(2000, 01, 01), "ABC", "ABC Pty Ltd", AssetCategory.AustralianFixedInterest);

            result.Should().HaveErrorStatus().WithError("A stock already exists with code ABC on 1/01/2000");

            mockRepository.Verify();
        }

        [Fact]
        public void ChangeStockDuplicateAsxCodeInThePast()
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
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            stockRepository.Setup(x => x.Update(stock)).Verifiable();
            var stockPriceHistoryCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache.Object, stockPriceHistoryRepository.Object);

            var result = service.ChangeStock(id, new Date(2000, 01, 01), "ABC", "ABC Pty Ltd", AssetCategory.AustralianFixedInterest);

            result.Should().HaveOkStatus();

            mockRepository.Verify();
        }

        [Fact]
        public void ChangeStock()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);


            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            stockRepository.Setup(x => x.Update(stock)).Verifiable();
            var stockPriceHistoryCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache.Object, stockPriceHistoryRepository.Object);

            var result = service.ChangeStock(id, new Date(2000, 01, 01), "ABC", "ABC Pty Ltd", AssetCategory.AustralianProperty);

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
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            var stockPriceHistoryCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache.Object, stockPriceHistoryRepository.Object);

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
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            var stockPriceHistoryCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache.Object, stockPriceHistoryRepository.Object);

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
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            var stockPriceHistoryCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache.Object, stockPriceHistoryRepository.Object);

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
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            var stockPriceHistoryCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache.Object, stockPriceHistoryRepository.Object);

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

            var stockPriceHistory = new StockPriceHistory(id);
            stock.SetPriceHistory(stockPriceHistory);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            var stockPriceHistoryCache = new EntityCache<StockPriceHistory>();
            stockPriceHistoryCache.Add(stockPriceHistory);
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache, stockPriceHistoryRepository.Object);

            var result = service.UpdateCurrentPrice(id, 10.00m);

            using (new AssertionScope())
            {
                result.Should().HaveOkStatus();
                stock.GetPrice(Date.Today).Should().Be(10.00m);
            }

            mockRepository.Verify();
        }

        [Fact]
        public void UpdateClosingPricesInvalidId()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();

            var stockCache = new EntityCache<Stock>();
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            var stockPriceHistoryCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache.Object, stockPriceHistoryRepository.Object);

            var result = service.UpdateClosingPrices(id, new StockPrice[] { });

            result.Should().HaveNotFoundStatus();

            mockRepository.Verify();
        }

        [Fact]
        public void UpdateClosingPricesStockNotListed()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var stock = new Stock(id);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            var stockPriceHistoryCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache.Object, stockPriceHistoryRepository.Object);

            var result = service.UpdateClosingPrices(id, new StockPrice[] { });

            result.Should().HaveErrorStatus().WithError("Stock is not listed");

            mockRepository.Verify();
        }

        [Fact]
        public void UpdateClosingPricesStockDelisted()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);
            stock.DeList(new Date(1999, 01, 01));

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            var stockPriceHistoryCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache.Object, stockPriceHistoryRepository.Object);

            var prices = new StockPrice[]
            {
                new StockPrice(new Date(1998, 01, 01), 0.10m),
                new StockPrice(new Date(1998, 12, 01), 0.20m),
                new StockPrice(new Date(1999, 01, 02), 0.30m)
            };
            var result = service.UpdateClosingPrices(id, prices);

            result.Should().HaveErrorStatus("Stock is no listed on 2/01/1999");

            mockRepository.Verify();
        }

        [Fact]
        public void UpdateClosingPricesOnDelistingDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);
            stock.DeList(new Date(1999, 01, 01));

            var stockPriceHistory = new StockPriceHistory(id);
            stock.SetPriceHistory(stockPriceHistory);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            var stockPriceHistoryCache = new EntityCache<StockPriceHistory>();
            stockPriceHistoryCache.Add(stockPriceHistory);
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();
            stockPriceHistoryRepository.Setup(x => x.Update(stockPriceHistory)).Verifiable();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache, stockPriceHistoryRepository.Object);

            var prices = new StockPrice[]
            {
                new StockPrice(new Date(1998, 01, 01), 0.10m),
                new StockPrice(new Date(1998, 12, 01), 0.20m),
                new StockPrice(new Date(1999, 01, 01), 0.30m)
            };
            var result = service.UpdateClosingPrices(id, prices);

            result.Should().HaveOkStatus();

            mockRepository.Verify();
        }

        [Fact]
        public void UpdateClosingPricesNegativePrice()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            var stockPriceHistoryCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache.Object, stockPriceHistoryRepository.Object);

            var prices = new StockPrice[]
            {
                new StockPrice(new Date(2000, 01, 01), 0.10m),
                new StockPrice(new Date(2000, 01, 02), -0.20m),
                new StockPrice(new Date(2000, 01, 03), 0.30m)
            };
            var result = service.UpdateClosingPrices(id, prices);

            result.Should().HaveErrorStatus("Closing price on 2/01/2000 is negative");

            mockRepository.Verify();
        }

        [Fact]
        public void UpdateClosingPrices()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);

            var stockPriceHistory = new StockPriceHistory(id);
            stock.SetPriceHistory(stockPriceHistory);
            
            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            var stockPriceHistoryCache = new EntityCache<StockPriceHistory>();
            stockPriceHistoryCache.Add(stockPriceHistory);
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();
            stockPriceHistoryRepository.Setup(x => x.Update(stockPriceHistory)).Verifiable();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache, stockPriceHistoryRepository.Object);

            var prices = new StockPrice[]
            {
                new StockPrice(new Date(2000, 01, 01), 0.10m),
                new StockPrice(new Date(2000, 01, 02), 0.20m),
                new StockPrice(new Date(2000, 01, 03), 0.30m)
            };
            var result = service.UpdateClosingPrices(id, prices);

            using (new AssertionScope())
            {
                result.Should().HaveOkStatus();

                stock.GetPrice(new Date(2000, 01, 01)).Should().Be(0.10m);
                stock.GetPrice(new Date(2000, 01, 02)).Should().Be(0.20m);
                stock.GetPrice(new Date(2000, 01, 03)).Should().Be(0.30m);     
            }

            mockRepository.Verify();
        }

        [Fact]
        public void ChangeDividendRulesInvalidId()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();

            var stockCache = new EntityCache<Stock>();
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            var stockPriceHistoryCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache.Object, stockPriceHistoryRepository.Object);

            var result = service.ChangeDividendRules(id, new Date(2000, 01, 01), 0.30m, RoundingRule.Round, true, DrpMethod.Round);

            result.Should().HaveNotFoundStatus();

            mockRepository.Verify();
        }

        [Fact]
        public void ChangeDividendRulesStockNotActive()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();

            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);
            stock.DeList(new Date(1999, 06, 30));

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            var stockPriceHistoryCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache.Object, stockPriceHistoryRepository.Object);

            var result = service.ChangeDividendRules(id, new Date(2000, 01, 01), 0.30m, RoundingRule.Round, true, DrpMethod.Round);

            result.Should().HaveErrorStatus().WithError("Stock is not active at 1/01/2000");

            mockRepository.Verify();
        }

        [Fact]
        public void ChangeDividendRulesTaxRateNegative()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();

            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            var stockPriceHistoryCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache.Object, stockPriceHistoryRepository.Object);

            var result = service.ChangeDividendRules(id, new Date(2000, 01, 01), -0.30m, RoundingRule.Round, true, DrpMethod.Round);

            result.Should().HaveErrorStatus().WithError("Company tax rate must be between 0 and 1");

            mockRepository.Verify();
        }

        [Fact]
        public void ChangeDividendRulesTaxRateGreaterThan100Percent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();

            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            var stockPriceHistoryCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache.Object, stockPriceHistoryRepository.Object);

            var result = service.ChangeDividendRules(id, new Date(2000, 01, 01), 1.30m, RoundingRule.Round, true, DrpMethod.Round);

            result.Should().HaveErrorStatus().WithError("Company tax rate must be between 0 and 1");

            mockRepository.Verify();
        }

        [Fact]
        public void ChangeDividendRules()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();

            var stock = new Stock(id);
            stock.List("XYZ", "Existing Stock", new Date(1990, 01, 01), true, AssetCategory.AustralianFixedInterest);

            var stockCache = new EntityCache<Stock>();
            stockCache.Add(stock);
            var stockQuery = new StockQuery(stockCache);
            var stockRepository = mockRepository.Create<IRepository<Stock>>();
            stockRepository.Setup(x => x.Update(stock)).Verifiable();
            var stockPriceHistoryCache = mockRepository.Create<IEntityCache<StockPriceHistory>>();
            var stockPriceHistoryRepository = mockRepository.Create<IRepository<StockPriceHistory>>();

            var service = new StockService(stockQuery, stockCache, stockRepository.Object, stockPriceHistoryCache.Object, stockPriceHistoryRepository.Object);

            var result = service.ChangeDividendRules(id, new Date(2000, 01, 01), 0.50m, RoundingRule.Round, true, DrpMethod.Round);

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
