using System;
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
    public class DividendTests
    {

        [Fact]
        public void HasBeenAppliedNoTransactionsAtRecordDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var dividend = new Dividend(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Dividend", new Date(2020, 02, 01), 100.00m, 100.0m, 0.00m);

            var transactions = mockRepository.Create<IPortfolioTransactionList>();
            transactions.Setup(x => x.ForHolding(stock.Id, dividend.PaymentDate)).Returns(new IPortfolioTransaction[] { });

            
            var result = dividend.HasBeenApplied(transactions.Object);

            result.Should().BeFalse();

            mockRepository.Verify();
        }

        [Fact]
        public void HasBeenAppliedNoIncomeAtRecordDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

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

            result.Should().BeFalse();

            mockRepository.Verify();
        }

        [Fact]
        public void HasBeenApplied()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

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
                DrpCashBalance = 0.00m
            };
            var transactions = mockRepository.Create<IPortfolioTransactionList>();
            transactions.Setup(x => x.ForHolding(stock.Id, dividend.PaymentDate)).Returns(new IPortfolioTransaction[] { transaction });

            var result = dividend.HasBeenApplied(transactions.Object);

            result.Should().BeTrue();

            mockRepository.Verify();
        }

        [Fact]
        public void NoParcelsAtRecordDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var dividend = new Dividend(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Dividend", new Date(2020, 02, 01), 100.00m, 100.0m, 0.00m);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[dividend.Date]).Returns(new HoldingProperties(0,0.00m, 0.00m));

            var result = dividend.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            result.Should().BeEmpty();

            mockRepository.Verify();
        }

        [Fact]
        public void AmountsRoundedNoDrp()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(new Date(1974, 01, 01), 0.30m, RoundingRule.Round, false, DrpMethod.Round);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var dividend = new Dividend(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Dividend", new Date(2020, 02, 01), 1.15065m, 1.00m, 0.00m);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[dividend.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Settings).Returns(new HoldingSettings(false));

            var result = dividend.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            result.Should().SatisfyRespectively(
                first =>
                {
                    first.Should().BeOfType<IncomeReceived>();

                    if (first is IncomeReceived income)
                    {
                        income.Date.Should().Be(dividend.PaymentDate);
                        income.Stock.Should().Be(dividend.Stock);
                        income.Comment.Should().Be("Test Dividend");
                        income.RecordDate.Should().Be(dividend.Date);
                        income.FrankedAmount.Should().Be(115.07m);
                        income.UnfrankedAmount.Should().Be(0.00m);
                        income.FrankingCredits.Should().Be(49.31m);
                        income.Interest.Should().Be(0.00m);
                        income.TaxDeferred.Should().Be(0.00m);
                        income.CreateCashTransaction.Should().BeTrue();
                        income.DrpCashBalance.Should().Be(0.00m);
                    }
                }
            );

            mockRepository.Verify();
        }

        [Fact]
        public void AmountsTruncatedNoDrp()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(new Date(1974, 01, 01), 0.30m, RoundingRule.Truncate, false, DrpMethod.Round);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var dividend = new Dividend(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Dividend", new Date(2020, 02, 01), 1.15065m, 1.00m, 0.00m);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[dividend.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Settings).Returns(new HoldingSettings(false));

            var result = dividend.GetTransactionList(holding.Object, stockResolver.Object).ToList();
       
            result.Should().SatisfyRespectively(
                first =>
                {
                    first.Should().BeOfType<IncomeReceived>();

                    if (first is IncomeReceived income)
                    {
                        income.Date.Should().Be(dividend.PaymentDate);
                        income.Stock.Should().Be(dividend.Stock);
                        income.Comment.Should().Be("Test Dividend");
                        income.RecordDate.Should().Be(dividend.Date);
                        income.FrankedAmount.Should().Be(115.06m);
                        income.UnfrankedAmount.Should().Be(0.00m);
                        income.FrankingCredits.Should().Be(49.31m);
                        income.Interest.Should().Be(0.00m);
                        income.TaxDeferred.Should().Be(0.00m);
                        income.CreateCashTransaction.Should().BeTrue();
                        income.DrpCashBalance.Should().Be(0.00m);
                    }
                }
            );

            mockRepository.Verify();
        }

        [Fact]
        public void DrpActiveButNotParticipating()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(new Date(1974, 01, 01), 0.30m, RoundingRule.Round, true, DrpMethod.Round);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var dividend = new Dividend(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Dividend", new Date(2020, 02, 01), 1.15065m, 1.00m, 2.30m);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[dividend.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Settings).Returns(new HoldingSettings(false));

            var result = dividend.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            result.Should().SatisfyRespectively(
                first =>
                {
                    first.Should().BeOfType<IncomeReceived>();

                    if (first is IncomeReceived income)
                    {
                        income.Date.Should().Be(dividend.PaymentDate);
                        income.Stock.Should().Be(dividend.Stock);
                        income.Comment.Should().Be("Test Dividend");
                        income.RecordDate.Should().Be(dividend.Date);
                        income.FrankedAmount.Should().Be(115.07m);
                        income.UnfrankedAmount.Should().Be(0.00m);
                        income.FrankingCredits.Should().Be(49.31m);
                        income.Interest.Should().Be(0.00m);
                        income.TaxDeferred.Should().Be(0.00m);
                        income.CreateCashTransaction.Should().BeTrue();
                        income.DrpCashBalance.Should().Be(0.00m);
                    }
                }
            );

            mockRepository.Verify();
        }

        [Fact]
        public void DrpNotActiveAndParticipating()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(new Date(1974, 01, 01), 0.30m, RoundingRule.Round, false, DrpMethod.Round);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var dividend = new Dividend(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Dividend", new Date(2020, 02, 01), 1.15065m, 1.00m, 2.30m);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[dividend.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Settings).Returns(new HoldingSettings(true));

            var result = dividend.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            result.Should().SatisfyRespectively(
                first =>
                {
                    first.Should().BeOfType<IncomeReceived>();

                    if (first is IncomeReceived income)
                    {
                        income.Date.Should().Be(dividend.PaymentDate);
                        income.Stock.Should().Be(dividend.Stock);
                        income.Comment.Should().Be("Test Dividend");
                        income.RecordDate.Should().Be(dividend.Date);
                        income.FrankedAmount.Should().Be(115.07m);
                        income.UnfrankedAmount.Should().Be(0.00m);
                        income.FrankingCredits.Should().Be(49.31m);
                        income.Interest.Should().Be(0.00m);
                        income.TaxDeferred.Should().Be(0.00m);
                        income.CreateCashTransaction.Should().BeTrue();
                        income.DrpCashBalance.Should().Be(0.00m);
                    }
                }
            );

            mockRepository.Verify();
        }

        [Fact]
        public void DrpActiveAndParticipatingNoDrpAmount()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(new Date(1974, 01, 01), 0.30m, RoundingRule.Round, true, DrpMethod.Round);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var dividend = new Dividend(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Dividend", new Date(2020, 02, 01), 1.15065m, 1.00m, 0.00m);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[dividend.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Settings).Returns(new HoldingSettings(true));

            var result = dividend.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            result.Should().SatisfyRespectively(
                first =>
                {
                    first.Should().BeOfType<IncomeReceived>();

                    if (first is IncomeReceived income)
                    {
                        income.Date.Should().Be(dividend.PaymentDate);
                        income.Stock.Should().Be(dividend.Stock);
                        income.Comment.Should().Be("Test Dividend");
                        income.RecordDate.Should().Be(dividend.Date);
                        income.FrankedAmount.Should().Be(115.07m);
                        income.UnfrankedAmount.Should().Be(0.00m);
                        income.FrankingCredits.Should().Be(49.31m);
                        income.Interest.Should().Be(0.00m);
                        income.TaxDeferred.Should().Be(0.00m);
                        income.CreateCashTransaction.Should().BeTrue();
                        income.DrpCashBalance.Should().Be(0.00m);
                    }
                }
            );

            mockRepository.Verify();
        }

        [Fact]
        public void DrpRound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(new Date(1974, 01, 01), 0.30m, RoundingRule.Round, true, DrpMethod.Round);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var dividend = new Dividend(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Dividend", new Date(2020, 02, 01), 1.15065m, 1.00m, 2.30m);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[dividend.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Settings).Returns(new HoldingSettings(true));

            var result = dividend.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            result.Should().SatisfyRespectively(
                first =>
                {
                    first.Should().BeOfType<IncomeReceived>();

                    if (first is IncomeReceived income)
                    {
                        income.Date.Should().Be(dividend.PaymentDate);
                        income.Stock.Should().Be(dividend.Stock);
                        income.Comment.Should().Be("Test Dividend");
                        income.RecordDate.Should().Be(dividend.Date);
                        income.FrankedAmount.Should().Be(115.07m);
                        income.UnfrankedAmount.Should().Be(0.00m);
                        income.FrankingCredits.Should().Be(49.31m);
                        income.Interest.Should().Be(0.00m);
                        income.TaxDeferred.Should().Be(0.00m);
                        income.CreateCashTransaction.Should().BeFalse();
                        income.DrpCashBalance.Should().Be(0.00m);
                    }
                },
                second =>
                {
                    second.Should().BeOfType<OpeningBalance>();

                    if (second is OpeningBalance openingBalance)
                    {
                        openingBalance.Date.Should().Be(dividend.PaymentDate);
                        openingBalance.Stock.Should().Be(dividend.Stock);
                        openingBalance.Comment.Should().Be("DRP $2.30");
                        openingBalance.Units.Should().Be(50);
                        openingBalance.CostBase.Should().Be(115.07m);
                        openingBalance.AquisitionDate.Should().Be(dividend.PaymentDate);
                    }
                }
            );

            mockRepository.Verify();
        }

        [Fact]
        public void DrpRoundUp()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(new Date(1974, 01, 01), 0.30m, RoundingRule.Round, true, DrpMethod.RoundUp);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var dividend = new Dividend(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Dividend", new Date(2020, 02, 01), 1.15065m, 1.00m, 2.30m);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[dividend.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Settings).Returns(new HoldingSettings(true));

            var result = dividend.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            result.Should().SatisfyRespectively(
                first =>
                {
                    first.Should().BeOfType<IncomeReceived>();

                    if (first is IncomeReceived income)
                    {
                        income.Date.Should().Be(dividend.PaymentDate);
                        income.Stock.Should().Be(dividend.Stock);
                        income.Comment.Should().Be("Test Dividend");
                        income.RecordDate.Should().Be(dividend.Date);
                        income.FrankedAmount.Should().Be(115.07m);
                        income.UnfrankedAmount.Should().Be(0.00m);
                        income.FrankingCredits.Should().Be(49.31m);
                        income.Interest.Should().Be(0.00m);
                        income.TaxDeferred.Should().Be(0.00m);
                        income.CreateCashTransaction.Should().BeFalse();
                        income.DrpCashBalance.Should().Be(0.00m);
                    }
                },
                second =>
                {
                    second.Should().BeOfType<OpeningBalance>();

                    if (second is OpeningBalance openingBalance)
                    {
                        openingBalance.Date.Should().Be(dividend.PaymentDate);
                        openingBalance.Stock.Should().Be(dividend.Stock);
                        openingBalance.Comment.Should().Be("DRP $2.30");
                        openingBalance.Units.Should().Be(51);
                        openingBalance.CostBase.Should().Be(115.07m);
                        openingBalance.AquisitionDate.Should().Be(dividend.PaymentDate);
                    }
                }
            );

            mockRepository.Verify();
        }

        [Fact]
        public void DrpRoundDown()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(new Date(1974, 01, 01), 0.30m, RoundingRule.Round, true, DrpMethod.RoundDown);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var dividend = new Dividend(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Dividend", new Date(2020, 02, 01), 1.15065m, 1.00m, 2.30m);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[dividend.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Settings).Returns(new HoldingSettings(true));

            var result = dividend.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            result.Should().SatisfyRespectively(
                first =>
                {
                    first.Should().BeOfType<IncomeReceived>();

                    if (first is IncomeReceived income)
                    {
                        income.Date.Should().Be(dividend.PaymentDate);
                        income.Stock.Should().Be(dividend.Stock);
                        income.Comment.Should().Be("Test Dividend");
                        income.RecordDate.Should().Be(dividend.Date);
                        income.FrankedAmount.Should().Be(115.07m);
                        income.UnfrankedAmount.Should().Be(0.00m);
                        income.FrankingCredits.Should().Be(49.31m);
                        income.Interest.Should().Be(0.00m);
                        income.TaxDeferred.Should().Be(0.00m);
                        income.CreateCashTransaction.Should().BeFalse();
                        income.DrpCashBalance.Should().Be(0.00m);
                    }
                },
                second =>
                {
                    second.Should().BeOfType<OpeningBalance>();

                    if (second is OpeningBalance openingBalance)
                    {
                        openingBalance.Date.Should().Be(dividend.PaymentDate);
                        openingBalance.Stock.Should().Be(dividend.Stock);
                        openingBalance.Comment.Should().Be("DRP $2.30");
                        openingBalance.Units.Should().Be(50);
                        openingBalance.CostBase.Should().Be(115.07m);
                        openingBalance.AquisitionDate.Should().Be(dividend.PaymentDate);
                    }
                }
            );

            mockRepository.Verify();
        }

        [Fact]
        public void DrpRetainCashBalance()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(new Date(1974, 01, 01), 0.30m, RoundingRule.Round, true, DrpMethod.RetainCashBalance);

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

            result.Should().SatisfyRespectively(
                first =>
                {
                    first.Should().BeOfType<IncomeReceived>();

                    if (first is IncomeReceived income)
                    {
                        income.Date.Should().Be(dividend.PaymentDate);
                        income.Stock.Should().Be(dividend.Stock);
                        income.Comment.Should().Be("Test Dividend");
                        income.RecordDate.Should().Be(dividend.Date);
                        income.FrankedAmount.Should().Be(115.07m);
                        income.UnfrankedAmount.Should().Be(0.00m);
                        income.FrankingCredits.Should().Be(49.31m);
                        income.Interest.Should().Be(0.00m);
                        income.TaxDeferred.Should().Be(0.00m);
                        income.CreateCashTransaction.Should().BeFalse();
                        income.DrpCashBalance.Should().Be(0.97m);
                    }
                },
                second =>
                {
                    second.Should().BeOfType<OpeningBalance>();

                    if (second is OpeningBalance openingBalance)
                    {
                        openingBalance.Date.Should().Be(dividend.PaymentDate);
                        openingBalance.Stock.Should().Be(dividend.Stock);
                        openingBalance.Comment.Should().Be("DRP $2.30");
                        openingBalance.Units.Should().Be(51);
                        openingBalance.CostBase.Should().Be(117.30m);
                        openingBalance.AquisitionDate.Should().Be(dividend.PaymentDate);
                    }
                }
            );

            mockRepository.Verify();
        }

        [Fact]
        public void DrpPaymentNotEnoughForAnyUnits()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(new Date(1974, 01, 01), 0.30m, RoundingRule.Round, true, DrpMethod.RetainCashBalance);

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

            result.Should().SatisfyRespectively(
                first =>
                {
                    first.Should().BeOfType<IncomeReceived>();

                    if (first is IncomeReceived income)
                    {
                        income.Date.Should().Be(dividend.PaymentDate);
                        income.Stock.Should().Be(dividend.Stock);
                        income.Comment.Should().Be("Test Dividend");
                        income.RecordDate.Should().Be(dividend.Date);
                        income.FrankedAmount.Should().Be(115.07m);
                        income.UnfrankedAmount.Should().Be(0.00m);
                        income.FrankingCredits.Should().Be(49.31m);
                        income.Interest.Should().Be(0.00m);
                        income.TaxDeferred.Should().Be(0.00m);
                        income.CreateCashTransaction.Should().BeFalse();
                        income.DrpCashBalance.Should().Be(118.27m);
                    }
                }
            );

            mockRepository.Verify();
        }

    }
}
