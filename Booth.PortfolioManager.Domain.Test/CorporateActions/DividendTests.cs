using System;
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
    class DividendTests
    {

        [TestCase]
        public void HasBeenAppliedNoTransactionsAtRecordDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var dividend = new Dividend(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Dividend", new Date(2020, 02, 01), 100.00m, 100.0m, 0.00m);

            var transactions = mockRepository.Create<IPortfolioTransactionList>();
            transactions.Setup(x => x.ForHolding(stock.Id, dividend.PaymentDate)).Returns(new IPortfolioTransaction[] { });

            
            var result = dividend.HasBeenApplied(transactions.Object);

            Assert.That(result, Is.False);

            mockRepository.Verify();
        }

        [TestCase]
        public void HasBeenAppliedNoIncomeAtRecordDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var dividend = new Dividend(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Dividend", new Date(2020, 02, 01), 100.00m, 100.0m, 0.00m);

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
            transactions.Setup(x => x.ForHolding(stock.Id, dividend.PaymentDate)).Returns(new IPortfolioTransaction[] { transaction });

            var result = dividend.HasBeenApplied(transactions.Object);

            Assert.That(result, Is.False);

            mockRepository.Verify();
        }

        [TestCase]
        public void HasBeenApplied()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var dividend = new Dividend(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Dividend", new Date(2020, 02, 01), 100.00m, 100.0m, 0.00m);
         
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
            transactions.Setup(x => x.ForHolding(stock.Id, dividend.PaymentDate)).Returns(new IPortfolioTransaction[] { transaction });

            var result = dividend.HasBeenApplied(transactions.Object);

            Assert.That(result, Is.True);

            mockRepository.Verify();
        }

        [TestCase]
        public void NoParcelsAtRecordDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var dividend = new Dividend(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Dividend", new Date(2020, 02, 01), 100.00m, 100.0m, 0.00m);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[dividend.Date]).Returns(new HoldingProperties(0,0.00m, 0.00m));

            var result = dividend.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            Assert.That(result, Is.Empty);

            mockRepository.Verify();
        }

        [TestCase]
        public void AmountsRoundedNoDRP()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(Date.MinValue, 0.30m, RoundingRule.Round, false, DRPMethod.Round);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var dividend = new Dividend(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Dividend", new Date(2020, 02, 01), 1.15065m, 1.00m, 0.00m);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[dividend.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Settings).Returns(new HoldingSettings(false));

            var result = dividend.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(1));
                if (result.Count >= 1)
                {
                    Assert.That(result[0], Is.TypeOf(typeof(IncomeReceived)), "Transaction 1");
                    if (result[0] is IncomeReceived income)
                    {
                        Assert.That(income.Date, Is.EqualTo(dividend.PaymentDate), "Transaction 1");
                        Assert.That(income.Stock, Is.EqualTo(dividend.Stock), "Transaction 1");
                        Assert.That(income.Comment, Is.EqualTo("Test Dividend"), "Transaction 1");
                        Assert.That(income.RecordDate, Is.EqualTo(dividend.Date), "Transaction 1");
                        Assert.That(income.FrankedAmount, Is.EqualTo(115.07m), "Transaction 1");
                        Assert.That(income.UnfrankedAmount, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.FrankingCredits, Is.EqualTo(49.31m), "Transaction 1");
                        Assert.That(income.Interest, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.TaxDeferred, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.CreateCashTransaction, Is.EqualTo(true), "Transaction 1");
                        Assert.That(income.DRPCashBalance, Is.EqualTo(0.00m), "Transaction 1");
                    }
                }
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void AmountsTruncatedNoDRP()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(Date.MinValue, 0.30m, RoundingRule.Truncate, false, DRPMethod.Round);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var dividend = new Dividend(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Dividend", new Date(2020, 02, 01), 1.15065m, 1.00m, 0.00m);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[dividend.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Settings).Returns(new HoldingSettings(false));

            var result = dividend.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(1));
                if (result.Count >= 1)
                {
                    Assert.That(result[0], Is.TypeOf(typeof(IncomeReceived)), "Transaction 1");
                    if (result[0] is IncomeReceived income)
                    {
                        Assert.That(income.Date, Is.EqualTo(dividend.PaymentDate), "Transaction 1");
                        Assert.That(income.Stock, Is.EqualTo(dividend.Stock), "Transaction 1");
                        Assert.That(income.Comment, Is.EqualTo("Test Dividend"), "Transaction 1");
                        Assert.That(income.RecordDate, Is.EqualTo(dividend.Date), "Transaction 1");
                        Assert.That(income.FrankedAmount, Is.EqualTo(115.06m), "Transaction 1");
                        Assert.That(income.UnfrankedAmount, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.FrankingCredits, Is.EqualTo(49.31m), "Transaction 1");
                        Assert.That(income.Interest, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.TaxDeferred, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.CreateCashTransaction, Is.EqualTo(true), "Transaction 1");
                        Assert.That(income.DRPCashBalance, Is.EqualTo(0.00m), "Transaction 1");
                    }
                }
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void DRPActiveButNotParticipating()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(Date.MinValue, 0.30m, RoundingRule.Round, true, DRPMethod.Round);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var dividend = new Dividend(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Dividend", new Date(2020, 02, 01), 1.15065m, 1.00m, 2.30m);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[dividend.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Settings).Returns(new HoldingSettings(false));

            var result = dividend.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(1));
                if (result.Count >= 1)
                {
                    Assert.That(result[0], Is.TypeOf(typeof(IncomeReceived)), "Transaction 1");
                    if (result[0] is IncomeReceived income)
                    {
                        Assert.That(income.Date, Is.EqualTo(dividend.PaymentDate), "Transaction 1");
                        Assert.That(income.Stock, Is.EqualTo(dividend.Stock), "Transaction 1");
                        Assert.That(income.Comment, Is.EqualTo("Test Dividend"), "Transaction 1");
                        Assert.That(income.RecordDate, Is.EqualTo(dividend.Date), "Transaction 1");
                        Assert.That(income.FrankedAmount, Is.EqualTo(115.07m), "Transaction 1");
                        Assert.That(income.UnfrankedAmount, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.FrankingCredits, Is.EqualTo(49.31m), "Transaction 1");
                        Assert.That(income.Interest, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.TaxDeferred, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.CreateCashTransaction, Is.EqualTo(true), "Transaction 1");
                        Assert.That(income.DRPCashBalance, Is.EqualTo(0.00m), "Transaction 1");
                    }
                }
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void DRPNotActiveAndParticipating()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(Date.MinValue, 0.30m, RoundingRule.Round, false, DRPMethod.Round);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var dividend = new Dividend(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Dividend", new Date(2020, 02, 01), 1.15065m, 1.00m, 2.30m);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[dividend.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Settings).Returns(new HoldingSettings(true));

            var result = dividend.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(1));
                if (result.Count >= 1)
                {
                    Assert.That(result[0], Is.TypeOf(typeof(IncomeReceived)), "Transaction 1");
                    if (result[0] is IncomeReceived income)
                    {
                        Assert.That(income.Date, Is.EqualTo(dividend.PaymentDate), "Transaction 1");
                        Assert.That(income.Stock, Is.EqualTo(dividend.Stock), "Transaction 1");
                        Assert.That(income.Comment, Is.EqualTo("Test Dividend"), "Transaction 1");
                        Assert.That(income.RecordDate, Is.EqualTo(dividend.Date), "Transaction 1");
                        Assert.That(income.FrankedAmount, Is.EqualTo(115.07m), "Transaction 1");
                        Assert.That(income.UnfrankedAmount, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.FrankingCredits, Is.EqualTo(49.31m), "Transaction 1");
                        Assert.That(income.Interest, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.TaxDeferred, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.CreateCashTransaction, Is.EqualTo(true), "Transaction 1");
                        Assert.That(income.DRPCashBalance, Is.EqualTo(0.00m), "Transaction 1");
                    }
                }
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void DRPActiveAndParticipatingNoDRPAmount()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(Date.MinValue, 0.30m, RoundingRule.Round, true, DRPMethod.Round);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var dividend = new Dividend(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Dividend", new Date(2020, 02, 01), 1.15065m, 1.00m, 0.00m);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[dividend.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Settings).Returns(new HoldingSettings(true));

            var result = dividend.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(1));
                if (result.Count >= 1)
                {
                    Assert.That(result[0], Is.TypeOf(typeof(IncomeReceived)), "Transaction 1");
                    if (result[0] is IncomeReceived income)
                    {
                        Assert.That(income.Date, Is.EqualTo(dividend.PaymentDate), "Transaction 1");
                        Assert.That(income.Stock, Is.EqualTo(dividend.Stock), "Transaction 1");
                        Assert.That(income.Comment, Is.EqualTo("Test Dividend"), "Transaction 1");
                        Assert.That(income.RecordDate, Is.EqualTo(dividend.Date), "Transaction 1");
                        Assert.That(income.FrankedAmount, Is.EqualTo(115.07m), "Transaction 1");
                        Assert.That(income.UnfrankedAmount, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.FrankingCredits, Is.EqualTo(49.31m), "Transaction 1");
                        Assert.That(income.Interest, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.TaxDeferred, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.CreateCashTransaction, Is.EqualTo(true), "Transaction 1");
                        Assert.That(income.DRPCashBalance, Is.EqualTo(0.00m), "Transaction 1");
                    }
                }
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void DRPRound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(Date.MinValue, 0.30m, RoundingRule.Round, true, DRPMethod.Round);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var dividend = new Dividend(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Dividend", new Date(2020, 02, 01), 1.15065m, 1.00m, 2.30m);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[dividend.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Settings).Returns(new HoldingSettings(true));

            var result = dividend.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(2));
                if (result.Count >= 1)
                {
                    Assert.That(result[0], Is.TypeOf(typeof(IncomeReceived)), "Transaction 1");
                    if (result[0] is IncomeReceived income)
                    {
                        Assert.That(income.Date, Is.EqualTo(dividend.PaymentDate), "Transaction 1");
                        Assert.That(income.Stock, Is.EqualTo(dividend.Stock), "Transaction 1");
                        Assert.That(income.Comment, Is.EqualTo("Test Dividend"), "Transaction 1");
                        Assert.That(income.RecordDate, Is.EqualTo(dividend.Date), "Transaction 1");
                        Assert.That(income.FrankedAmount, Is.EqualTo(115.07m), "Transaction 1");
                        Assert.That(income.UnfrankedAmount, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.FrankingCredits, Is.EqualTo(49.31m), "Transaction 1");
                        Assert.That(income.Interest, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.TaxDeferred, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.CreateCashTransaction, Is.EqualTo(false), "Transaction 1");
                        Assert.That(income.DRPCashBalance, Is.EqualTo(0.00m), "Transaction 1");
                    }
                }

                if (result.Count >= 2)
                {
                    Assert.That(result[1], Is.TypeOf(typeof(OpeningBalance)), "Transaction 2");
                    if (result[1] is OpeningBalance openingBalance)
                    {
                        Assert.That(openingBalance.Date, Is.EqualTo(dividend.PaymentDate), "Transaction 2");
                        Assert.That(openingBalance.Stock, Is.EqualTo(dividend.Stock), "Transaction 2");
                        Assert.That(openingBalance.Comment, Is.EqualTo("DRP $2.30"), "Transaction 2");
                        Assert.That(openingBalance.Units, Is.EqualTo(50), "Transaction 2");
                        Assert.That(openingBalance.CostBase, Is.EqualTo(115.07m), "Transaction 2");
                        Assert.That(openingBalance.AquisitionDate, Is.EqualTo(dividend.PaymentDate), "Transaction 2");
                    }
                }
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void DRPRoundUp()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(Date.MinValue, 0.30m, RoundingRule.Round, true, DRPMethod.RoundUp);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var dividend = new Dividend(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Dividend", new Date(2020, 02, 01), 1.15065m, 1.00m, 2.30m);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[dividend.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Settings).Returns(new HoldingSettings(true));

            var result = dividend.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(2));
                if (result.Count >= 1)
                {
                    Assert.That(result[0], Is.TypeOf(typeof(IncomeReceived)), "Transaction 1");
                    if (result[0] is IncomeReceived income)
                    {
                        Assert.That(income.Date, Is.EqualTo(dividend.PaymentDate), "Transaction 1");
                        Assert.That(income.Stock, Is.EqualTo(dividend.Stock), "Transaction 1");
                        Assert.That(income.Comment, Is.EqualTo("Test Dividend"), "Transaction 1");
                        Assert.That(income.RecordDate, Is.EqualTo(dividend.Date), "Transaction 1");
                        Assert.That(income.FrankedAmount, Is.EqualTo(115.07m), "Transaction 1");
                        Assert.That(income.UnfrankedAmount, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.FrankingCredits, Is.EqualTo(49.31m), "Transaction 1");
                        Assert.That(income.Interest, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.TaxDeferred, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.CreateCashTransaction, Is.EqualTo(false), "Transaction 1");
                        Assert.That(income.DRPCashBalance, Is.EqualTo(0.00m), "Transaction 1");
                    }
                }

                if (result.Count >= 2)
                {
                    Assert.That(result[1], Is.TypeOf(typeof(OpeningBalance)), "Transaction 2");
                    if (result[1] is OpeningBalance openingBalance)
                    {
                        Assert.That(openingBalance.Date, Is.EqualTo(dividend.PaymentDate), "Transaction 2");
                        Assert.That(openingBalance.Stock, Is.EqualTo(dividend.Stock), "Transaction 2");
                        Assert.That(openingBalance.Comment, Is.EqualTo("DRP $2.30"), "Transaction 2");
                        Assert.That(openingBalance.Units, Is.EqualTo(51), "Transaction 2");
                        Assert.That(openingBalance.CostBase, Is.EqualTo(115.07m), "Transaction 2");
                        Assert.That(openingBalance.AquisitionDate, Is.EqualTo(dividend.PaymentDate), "Transaction 2");
                    }
                }
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void DRPRoundDown()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(Date.MinValue, 0.30m, RoundingRule.Round, true, DRPMethod.RoundDown);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var dividend = new Dividend(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Dividend", new Date(2020, 02, 01), 1.15065m, 1.00m, 2.30m);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[dividend.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Settings).Returns(new HoldingSettings(true));

            var result = dividend.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(2));
                if (result.Count >= 1)
                {
                    Assert.That(result[0], Is.TypeOf(typeof(IncomeReceived)), "Transaction 1");
                    if (result[0] is IncomeReceived income)
                    {
                        Assert.That(income.Date, Is.EqualTo(dividend.PaymentDate), "Transaction 1");
                        Assert.That(income.Stock, Is.EqualTo(dividend.Stock), "Transaction 1");
                        Assert.That(income.Comment, Is.EqualTo("Test Dividend"), "Transaction 1");
                        Assert.That(income.RecordDate, Is.EqualTo(dividend.Date), "Transaction 1");
                        Assert.That(income.FrankedAmount, Is.EqualTo(115.07m), "Transaction 1");
                        Assert.That(income.UnfrankedAmount, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.FrankingCredits, Is.EqualTo(49.31m), "Transaction 1");
                        Assert.That(income.Interest, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.TaxDeferred, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.CreateCashTransaction, Is.EqualTo(false), "Transaction 1");
                        Assert.That(income.DRPCashBalance, Is.EqualTo(0.00m), "Transaction 1");
                    }
                }

                if (result.Count >= 2)
                {
                    Assert.That(result[1], Is.TypeOf(typeof(OpeningBalance)), "Transaction 2");
                    if (result[1] is OpeningBalance openingBalance)
                    {
                        Assert.That(openingBalance.Date, Is.EqualTo(dividend.PaymentDate), "Transaction 2");
                        Assert.That(openingBalance.Stock, Is.EqualTo(dividend.Stock), "Transaction 2");
                        Assert.That(openingBalance.Comment, Is.EqualTo("DRP $2.30"), "Transaction 2");
                        Assert.That(openingBalance.Units, Is.EqualTo(50), "Transaction 2");
                        Assert.That(openingBalance.CostBase, Is.EqualTo(115.07m), "Transaction 2");
                        Assert.That(openingBalance.AquisitionDate, Is.EqualTo(dividend.PaymentDate), "Transaction 2");
                    }
                }
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void DRPRetainCashBalance()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(Date.MinValue, 0.30m, RoundingRule.Round, true, DRPMethod.RetainCashBalance);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var dividend = new Dividend(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Dividend", new Date(2020, 02, 01), 1.15065m, 1.00m, 2.30m);

            var drpAccount = mockRepository.Create<ICashAccount>();
            drpAccount.Setup(x => x.Balance(dividend.Date)).Returns(3.20m);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[dividend.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Settings).Returns(new HoldingSettings(true));
            holding.Setup(x => x.DrpAccount).Returns(drpAccount.Object);

            var result = dividend.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(2));
                if (result.Count >= 1)
                {
                    Assert.That(result[0], Is.TypeOf(typeof(IncomeReceived)), "Transaction 1");
                    if (result[0] is IncomeReceived income)
                    {
                        Assert.That(income.Date, Is.EqualTo(dividend.PaymentDate), "Transaction 1");
                        Assert.That(income.Stock, Is.EqualTo(dividend.Stock), "Transaction 1");
                        Assert.That(income.Comment, Is.EqualTo("Test Dividend"), "Transaction 1");
                        Assert.That(income.RecordDate, Is.EqualTo(dividend.Date), "Transaction 1");
                        Assert.That(income.FrankedAmount, Is.EqualTo(115.07m), "Transaction 1");
                        Assert.That(income.UnfrankedAmount, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.FrankingCredits, Is.EqualTo(49.31m), "Transaction 1");
                        Assert.That(income.Interest, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.TaxDeferred, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.CreateCashTransaction, Is.EqualTo(false), "Transaction 1");
                        Assert.That(income.DRPCashBalance, Is.EqualTo(0.97m), "Transaction 1");
                    }
                }

                if (result.Count >= 2)
                {
                    Assert.That(result[1], Is.TypeOf(typeof(OpeningBalance)), "Transaction 2");
                    if (result[1] is OpeningBalance openingBalance)
                    {
                        Assert.That(openingBalance.Date, Is.EqualTo(dividend.PaymentDate), "Transaction 2");
                        Assert.That(openingBalance.Stock, Is.EqualTo(dividend.Stock), "Transaction 2");
                        Assert.That(openingBalance.Comment, Is.EqualTo("DRP $2.30"), "Transaction 2");
                        Assert.That(openingBalance.Units, Is.EqualTo(51), "Transaction 2");
                        Assert.That(openingBalance.CostBase, Is.EqualTo(117.30m), "Transaction 2");
                        Assert.That(openingBalance.AquisitionDate, Is.EqualTo(dividend.PaymentDate), "Transaction 2");
                    }
                }
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void DRPPaymentNotEnoughForAnyUnits()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(Date.MinValue, 0.30m, RoundingRule.Round, true, DRPMethod.RetainCashBalance);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var dividend = new Dividend(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Dividend", new Date(2020, 02, 01), 1.15065m, 1.00m, 200.00m);

            var drpAccount = mockRepository.Create<ICashAccount>();
            drpAccount.Setup(x => x.Balance(dividend.Date)).Returns(3.20m);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[dividend.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Settings).Returns(new HoldingSettings(true));
            holding.Setup(x => x.DrpAccount).Returns(drpAccount.Object);

            var result = dividend.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(1));
                if (result.Count >= 1)
                {
                    Assert.That(result[0], Is.TypeOf(typeof(IncomeReceived)), "Transaction 1");
                    if (result[0] is IncomeReceived income)
                    {
                        Assert.That(income.Date, Is.EqualTo(dividend.PaymentDate), "Transaction 1");
                        Assert.That(income.Stock, Is.EqualTo(dividend.Stock), "Transaction 1");
                        Assert.That(income.Comment, Is.EqualTo("Test Dividend"), "Transaction 1");
                        Assert.That(income.RecordDate, Is.EqualTo(dividend.Date), "Transaction 1");
                        Assert.That(income.FrankedAmount, Is.EqualTo(115.07m), "Transaction 1");
                        Assert.That(income.UnfrankedAmount, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.FrankingCredits, Is.EqualTo(49.31m), "Transaction 1");
                        Assert.That(income.Interest, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.TaxDeferred, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(income.CreateCashTransaction, Is.EqualTo(false), "Transaction 1");
                        Assert.That(income.DRPCashBalance, Is.EqualTo(118.27m), "Transaction 1");
                    }
                }
            });

            mockRepository.Verify();
        }

    }
}
