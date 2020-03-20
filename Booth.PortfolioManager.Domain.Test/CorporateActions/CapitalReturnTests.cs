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
    class CapitalReturnTests
    {
        [TestCase]
        public void HasBeenAppliedNoTransactionsAtRecordDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var capitalReturn = new CapitalReturn(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Capital Return", new Date(2020, 02, 01), 100.00m);

            var transactions = mockRepository.Create<IPortfolioTransactionList>();
            transactions.Setup(x => x.ForHolding(stock.Id, capitalReturn.PaymentDate)).Returns(new IPortfolioTransaction[] { });

            var result = capitalReturn.HasBeenApplied(transactions.Object);

            Assert.That(result, Is.False);

            mockRepository.Verify();
        }

        [TestCase]
        public void HasBeenAppliedNoReturnOfCapitalAtRecordDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var capitalReturn = new CapitalReturn(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Capital Return", new Date(2020, 02, 01), 100.00m);

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

            var transactions = mockRepository.Create<IPortfolioTransactionList>();
            transactions.Setup(x => x.ForHolding(stock.Id, capitalReturn.PaymentDate)).Returns(new IPortfolioTransaction[] { transaction });

            var result = capitalReturn.HasBeenApplied(transactions.Object);

            Assert.That(result, Is.False);

            mockRepository.Verify();
        }

        [TestCase]
        public void HasBeenApplied()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var capitalReturn = new CapitalReturn(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Capital Return", new Date(2020, 02, 01), 100.00m);

            var transaction = new ReturnOfCapital()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Return Of Capital",
                RecordDate = new Date(2020, 01, 01),
                Amount = 2.00m,
                CreateCashTransaction = false
            };
            var transactions = mockRepository.Create<IPortfolioTransactionList>();
            transactions.Setup(x => x.ForHolding(stock.Id, capitalReturn.PaymentDate)).Returns(new IPortfolioTransaction[] { transaction });

            var result = capitalReturn.HasBeenApplied(transactions.Object);

            Assert.That(result, Is.True);

            mockRepository.Verify();
        }

        [TestCase]
        public void NoParcelsAtRecordDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var capitalReturn = new CapitalReturn(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Capital Return", new Date(2020, 02, 01), 100.00m);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[capitalReturn.Date]).Returns(new HoldingProperties(0, 0.00m, 0.00m));

            var result = capitalReturn.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            Assert.That(result, Is.Empty);

            mockRepository.Verify();
        }

        [TestCase]
        public void AmountRounded()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(Date.MinValue, 0.30m, RoundingRule.Round, false, DRPMethod.Round);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var capitalReturn = new CapitalReturn(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Capital Return", new Date(2020, 02, 01), 1.20456m);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[capitalReturn.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Settings).Returns(new HoldingSettings(false));

            var result = capitalReturn.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(1));
                if (result.Count >= 1)
                {
                    Assert.That(result[0], Is.TypeOf(typeof(ReturnOfCapital)), "Transaction 1");
                    if (result[0] is ReturnOfCapital returnOfCapital)
                    {
                        Assert.That(returnOfCapital.Date, Is.EqualTo(capitalReturn.PaymentDate), "Transaction 1");
                        Assert.That(returnOfCapital.Stock, Is.EqualTo(capitalReturn.Stock), "Transaction 1");
                        Assert.That(returnOfCapital.Comment, Is.EqualTo("Test Capital Return"), "Transaction 1");
                        Assert.That(returnOfCapital.RecordDate, Is.EqualTo(capitalReturn.Date), "Transaction 1");
                        Assert.That(returnOfCapital.Amount, Is.EqualTo(120.46m), "Transaction 1");
                        Assert.That(returnOfCapital.CreateCashTransaction, Is.EqualTo(true), "Transaction 1");
                    }
                }
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void AmountTruncated()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(Date.MinValue, 0.30m, RoundingRule.Truncate, false, DRPMethod.Round);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var capitalReturn = new CapitalReturn(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Capital Return", new Date(2020, 02, 01), 1.20456m);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[capitalReturn.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Settings).Returns(new HoldingSettings(false));

            var result = capitalReturn.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(1));
                if (result.Count >= 1)
                {
                    Assert.That(result[0], Is.TypeOf(typeof(ReturnOfCapital)), "Transaction 1");
                    if (result[0] is ReturnOfCapital returnOfCapital)
                    {
                        Assert.That(returnOfCapital.Date, Is.EqualTo(capitalReturn.PaymentDate), "Transaction 1");
                        Assert.That(returnOfCapital.Stock, Is.EqualTo(capitalReturn.Stock), "Transaction 1");
                        Assert.That(returnOfCapital.Comment, Is.EqualTo("Test Capital Return"), "Transaction 1");
                        Assert.That(returnOfCapital.RecordDate, Is.EqualTo(capitalReturn.Date), "Transaction 1");
                        Assert.That(returnOfCapital.Amount, Is.EqualTo(120.45m), "Transaction 1");
                        Assert.That(returnOfCapital.CreateCashTransaction, Is.EqualTo(true), "Transaction 1");
                    }
                }
            });

            mockRepository.Verify();
        }
    }
}
