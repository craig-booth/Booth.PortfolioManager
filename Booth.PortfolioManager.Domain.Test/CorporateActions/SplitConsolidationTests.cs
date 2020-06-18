using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.CorporateActions;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.Domain.Portfolios;
using FluentAssertions;

namespace Booth.PortfolioManager.Domain.Test.CorporateActions
{
    public class SplitConsolidationTests
    {
        [Fact]
        public void HasBeenAppliedNoTransactionsAtRecordDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var splitConsolidation = new SplitConsolidation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test SplitConsolidation", 1, 2);

            var transactions = mockRepository.Create<IPortfolioTransactionList>();
            transactions.Setup(x => x.ForHolding(stock.Id, splitConsolidation.Date)).Returns(new IPortfolioTransaction[] { });

            var result = splitConsolidation.HasBeenApplied(transactions.Object);

            result.Should().BeFalse();

            mockRepository.Verify();
        }

        [Fact]
        public void HasBeenAppliedNoSplitAtRecordDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var splitConsolidation = new SplitConsolidation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test SplitConsolidation", 1, 2);

            var transaction = new IncomeReceived()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Dividend",
                RecordDate = new Date(2020, 01, 01),
                FrankedAmount = 10.00m,
                UnfrankedAmount = 20.00m,
                FrankingCredits = 30.00m,
                Interest = 40.00m,
                TaxDeferred = 0.00m,
                CreateCashTransaction = false,
                DrpCashBalance = 0.00m
            };

            var transactions = mockRepository.Create<IPortfolioTransactionList>();
            transactions.Setup(x => x.ForHolding(stock.Id, splitConsolidation.Date)).Returns(new IPortfolioTransaction[] { transaction });

            var result = splitConsolidation.HasBeenApplied(transactions.Object);

            result.Should().BeFalse();

            mockRepository.Verify();
        }

        [Fact]
        public void HasBeenApplied()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var splitConsolidation = new SplitConsolidation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test SplitConsolidation", 1, 2);

            var transaction = new UnitCountAdjustment()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Unit Count Adjustment",
                OriginalUnits = 1,
                NewUnits = 2
            };
            var transactions = mockRepository.Create<IPortfolioTransactionList>();
            transactions.Setup(x => x.ForHolding(stock.Id, splitConsolidation.Date)).Returns(new IPortfolioTransaction[] { transaction });

            var result = splitConsolidation.HasBeenApplied(transactions.Object);

            result.Should().BeTrue();

            mockRepository.Verify();
        }

        [Fact]
        public void NoParcelsAtRecordDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var splitConsolidation = new SplitConsolidation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test SplitConsolidation", 1, 2);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[splitConsolidation.Date]).Returns(new HoldingProperties(0, 0.00m, 0.00m));

            var result = splitConsolidation.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            result.Should().BeEmpty();

            mockRepository.Verify();
        }

        [Fact]
        public void StockSplit()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(Date.MinValue, 0.30m, RoundingRule.Round, false, DrpMethod.Round);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var splitConsolidation = new SplitConsolidation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test SplitConsolidation", 1, 2);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[splitConsolidation.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Settings).Returns(new HoldingSettings(false));

            var result = splitConsolidation.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            result.Should().SatisfyRespectively(
                first =>
                {
                    first.Should().BeOfType<UnitCountAdjustment>();

                    if (first is UnitCountAdjustment unitCountAdjustment)
                    {
                        unitCountAdjustment.Date.Should().Be(splitConsolidation.Date);
                        unitCountAdjustment.Stock.Should().Be(splitConsolidation.Stock);
                        unitCountAdjustment.Comment.Should().Be("Test SplitConsolidation");
                        unitCountAdjustment.NewUnits.Should().Be(2);
                        unitCountAdjustment.OriginalUnits.Should().Be(1);
                    }
                }
            );

            mockRepository.Verify();
        }

        [Fact]
        public void StockConsolidation()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(Date.MinValue, 0.30m, RoundingRule.Round, false, DrpMethod.Round);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var splitConsolidation = new SplitConsolidation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test SplitConsolidation", 2, 1);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[splitConsolidation.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Settings).Returns(new HoldingSettings(false));

            var result = splitConsolidation.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            result.Should().SatisfyRespectively(
                first =>
                {
                    first.Should().BeOfType<UnitCountAdjustment>();

                    if (first is UnitCountAdjustment unitCountAdjustment)
                    {
                        unitCountAdjustment.Date.Should().Be(splitConsolidation.Date);
                        unitCountAdjustment.Stock.Should().Be(splitConsolidation.Stock);
                        unitCountAdjustment.Comment.Should().Be("Test SplitConsolidation");
                        unitCountAdjustment.NewUnits.Should().Be(1);
                        unitCountAdjustment.OriginalUnits.Should().Be(2);
                    }
                }
            );

            mockRepository.Verify();
        }
    }

}
