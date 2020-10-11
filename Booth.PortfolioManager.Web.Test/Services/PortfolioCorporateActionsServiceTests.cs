using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using FluentAssertions;
using Moq;

using Booth.Common;
using Booth.EventStore;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Web.Utilities;
using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Web.Test.Services
{
    public class PortfolioCorporateActionsServiceTests
    {


     /*   private readonly Guid _StockWithoutCorporateActions;
        private readonly Guid _StockWithCorporateActions;

        private readonly Guid _Action1;
        private readonly Guid _Action2;

        private readonly CorporateActionService _Service;

        private readonly List<Event> _Events = new List<Event>(); */
      
        public PortfolioCorporateActionsServiceTests()
        {
     /*       var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockCache = new EntityCache<Stock>();
            var stockQuery = new StockQuery(stockCache);

            _StockWithoutCorporateActions = Guid.NewGuid();
            var stock = new Stock(_StockWithoutCorporateActions);
            stock.List("ABC", "ABC Pty Ltd", new Date(2000, 01, 01), false, AssetCategory.AustralianStocks);
            stockCache.Add(stock);

            _StockWithCorporateActions = Guid.NewGuid();
            var stock2 = new Stock(_StockWithCorporateActions);
            stock2.List("XYZ", "XYZ Pty Ltd", new Date(2000, 01, 01), false, AssetCategory.AustralianStocks);

            _Action1 = Guid.NewGuid();
            stock2.CorporateActions.AddCapitalReturn(_Action1, new Date(2001, 01, 01), "Action 1", new Date(2001, 01, 02), 10.00m);

            _Action2 = Guid.NewGuid();
            stock2.CorporateActions.AddDividend(_Action2, new Date(2001, 01, 01), "Action 2", new Date(2001, 01, 02), 10.00m, 1.00m, 2.45m);

            stockCache.Add(stock2);

            // Remove any existing events
            stock.FetchEvents();
            stock2.FetchEvents();

            var repository = mockRepository.Create<IRepository<Stock>>();
            repository.Setup(x => x.Update(It.IsAny<Stock>())).Callback<Stock>(x => _Events.AddRange(x.FetchEvents())); */
        }
/*
        [Fact]
        public void PortfolioNotFound()
        {
            var service = new PortfolioCorporateActionsService(null);

            var result = service.GetCorporateActions();

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetCorporateActions()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var holdingCollection = mockRepository.Create<IHoldingCollection>();

            var portfolio = mockRepository.Create<IReadOnlyPortfolio>();
            portfolio.Setup(x => x.Holdings).Returns(holdingCollection.Object);

            var service = new PortfolioCorporateActionsService(portfolio.Object);

            var result = service.GetCorporateActions();

            mockRepository.Verify();
        }

        [Fact]
        public void GetCorporateActionsStockNotFound()
        {
            false.Should().BeTrue();
        }

        [Fact]
        public void GetCorporateActionsStockNotOwned()
        {
            false.Should().BeTrue();
        }

        [Fact]
        public void GetCorporateActionsForStock()
        {
            false.Should().BeTrue();
        }

        [Fact]
        public void GetTransactionsForCorporateActionStockNotFound()
        {
            false.Should().BeTrue();
        }

        [Fact]
        public void GetTransactionsForCorporateActionStockNotOwned()
        {
            false.Should().BeTrue();
        }

        [Fact]
        public void GetTransactionsForCorporateActionActionNotFound()
        {
            false.Should().BeTrue();
        }

        [Fact]
        public void GetTransactionsForCorporateAction()
        {
            false.Should().BeTrue();
        } */
    }

}
