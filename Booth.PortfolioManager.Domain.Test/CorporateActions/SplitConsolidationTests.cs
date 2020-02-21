using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.CorporateActions;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Test.CorporateActions
{
    class SplitConsolidationTests
    {
        [TestCase]
        public void HasBeenAppliedNoTransactionsAtRecordDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var splitConsolidation = new SplitConsolidation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test SplitConsolidation", 1, 2);

            var transactions = mockRepository.Create<ITransactionCollection>();
            transactions.Setup(x => x.ForHolding(stock.Id, splitConsolidation.Date)).Returns(new Transaction[] { });

            var result = splitConsolidation.HasBeenApplied(transactions.Object);

            Assert.That(result, Is.False);

            mockRepository.Verify();
        }

        [TestCase]
        public void HasBeenAppliedNoSplitAtRecordDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

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
                DRPCashBalance = 0.00m
            };

            var transactions = mockRepository.Create<ITransactionCollection>();
            transactions.Setup(x => x.ForHolding(stock.Id, splitConsolidation.Date)).Returns(new Transaction[] { transaction });

            var result = splitConsolidation.HasBeenApplied(transactions.Object);

            Assert.That(result, Is.False);

            mockRepository.Verify();
        }

        [TestCase]
        public void HasBeenApplied()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

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
            var transactions = mockRepository.Create<ITransactionCollection>();
            transactions.Setup(x => x.ForHolding(stock.Id, splitConsolidation.Date)).Returns(new Transaction[] { transaction });

            var result = splitConsolidation.HasBeenApplied(transactions.Object);

            Assert.That(result, Is.True);

            mockRepository.Verify();
        }

        [TestCase]
        public void NoParcelsAtRecordDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var splitConsolidation = new SplitConsolidation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test SplitConsolidation", 1, 2);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[splitConsolidation.Date]).Returns(new HoldingProperties(0, 0.00m, 0.00m));

            var result = splitConsolidation.GetTransactionList(holding.Object).ToList();

            Assert.That(result, Is.Empty);

            mockRepository.Verify();
        }

        [TestCase]
        public void StockSplit()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(Date.MinValue, 0.30m, RoundingRule.Round, false, DRPMethod.Round);

            var splitConsolidation = new SplitConsolidation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test SplitConsolidation", 1, 2);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[splitConsolidation.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Settings).Returns(new HoldingSettings(false));

            var result = splitConsolidation.GetTransactionList(holding.Object).ToList();

            Assert.Multiple(() => {
                Assert.That(result, Has.Count.EqualTo(1));
                Assert.That(result[0], Is.TypeOf(typeof(UnitCountAdjustment)));

                var unitCountAdjustment = result[0] as UnitCountAdjustment;
                Assert.That(unitCountAdjustment.Date, Is.EqualTo(splitConsolidation.Date));
                Assert.That(unitCountAdjustment.Stock, Is.EqualTo(splitConsolidation.Stock));
                Assert.That(unitCountAdjustment.Comment, Is.EqualTo("Test SplitConsolidation"));
                Assert.That(unitCountAdjustment.NewUnits, Is.EqualTo(1));
                Assert.That(unitCountAdjustment.OriginalUnits, Is.EqualTo(2));
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void StockConsolidation()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(Date.MinValue, 0.30m, RoundingRule.Round, false, DRPMethod.Round);

            var splitConsolidation = new SplitConsolidation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test SplitConsolidation", 2, 1);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[splitConsolidation.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Settings).Returns(new HoldingSettings(false));

            var result = splitConsolidation.GetTransactionList(holding.Object).ToList();

            Assert.Multiple(() => {
                Assert.That(result, Has.Count.EqualTo(1));
                Assert.That(result[0], Is.TypeOf(typeof(UnitCountAdjustment)));

                var unitCountAdjustment = result[0] as UnitCountAdjustment;
                Assert.That(unitCountAdjustment.Date, Is.EqualTo(splitConsolidation.Date));
                Assert.That(unitCountAdjustment.Stock, Is.EqualTo(splitConsolidation.Stock));
                Assert.That(unitCountAdjustment.Comment, Is.EqualTo("Test SplitConsolidation"));
                Assert.That(unitCountAdjustment.NewUnits, Is.EqualTo(2));
                Assert.That(unitCountAdjustment.OriginalUnits, Is.EqualTo(1));
            });

            mockRepository.Verify();
        }
    }

}
