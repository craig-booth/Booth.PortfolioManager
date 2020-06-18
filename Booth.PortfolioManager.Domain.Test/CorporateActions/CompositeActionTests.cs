using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using FluentAssertions;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.CorporateActions;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.Domain.Portfolios;


namespace Booth.PortfolioManager.Domain.Test.CorporateActions
{
    public class CompositeActionTests
    {
        [Fact]
        public void HasBeenApplied()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var transactions = mockRepository.Create<IPortfolioTransactionList>();

            var firstAction = mockRepository.Create<ICorporateAction>();
            firstAction.Setup(x => x.HasBeenApplied(transactions.Object)).Returns(true).Verifiable();
            var secondAction = mockRepository.Create<ICorporateAction>();
            var childActions = new ICorporateAction[] { firstAction.Object, secondAction.Object };
            var compositeAction = new CompositeAction(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Composite Action", childActions);            

            var result = compositeAction.HasBeenApplied(transactions.Object);

            result.Should().BeTrue();

            mockRepository.Verify();
        }

        [Fact]
        public void HasBeenAppliedNoChildActions()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var transactions = mockRepository.Create<IPortfolioTransactionList>();

            var childActions = new ICorporateAction[] { };
            var compositeAction = new CompositeAction(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Composite Action", childActions);

            var result = compositeAction.HasBeenApplied(transactions.Object);

            result.Should().BeFalse();

            mockRepository.Verify();
        }


        [Fact]
        public void GetTransactionList()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holding = mockRepository.Create<IReadOnlyHolding>();

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var t1 = mockRepository.Create<IPortfolioTransaction>();
            var t2 = mockRepository.Create<IPortfolioTransaction>();
            var t3 = mockRepository.Create<IPortfolioTransaction>();

            var firstAction = mockRepository.Create<ICorporateAction>();
            firstAction.Setup(x => x.GetTransactionList(holding.Object, stockResolver.Object)).Returns(new IPortfolioTransaction[] { t1.Object }).Verifiable();
            var secondAction = mockRepository.Create<ICorporateAction>();
            secondAction.Setup(x => x.GetTransactionList(holding.Object, stockResolver.Object)).Returns(new IPortfolioTransaction[] { t2.Object, t3.Object }).Verifiable();
            var childActions = new ICorporateAction[] { firstAction.Object, secondAction.Object };
            var compositeAction = new CompositeAction(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Composite Action", childActions);

            var result = compositeAction.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            result.Should().Equal(new IPortfolioTransaction[] { t1.Object, t2.Object, t3.Object });

            mockRepository.Verify();
        }

     
    }
}
