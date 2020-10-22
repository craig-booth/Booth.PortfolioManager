using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.Test.Portfolios
{
    public class PortfolioTests
    {
        [Fact]
        public void Create()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockResolver = mockRepository.Create<IStockResolver>();
            var transactionHandlers = mockRepository.Create<IServiceFactory<ITransactionHandler>>();

            var id = Guid.NewGuid();
            var portfolio = new Portfolio(id, stockResolver.Object, transactionHandlers.Object);

            var owner = Guid.NewGuid();
            portfolio.Create("Test", owner);

            portfolio.Should().BeEquivalentTo(new { Id = id, Name = "Test", Owner = owner });

            mockRepository.Verify();
        }

        [Fact]
        public void ChangeDrpParticipationHoldingNotOwned()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var transactionHandlers = mockRepository.Create<IServiceFactory<ITransactionHandler>>();

            var portfolio = new Portfolio(Guid.NewGuid(), stockResolver.Object, transactionHandlers.Object);

            Action a = () => portfolio.ChangeDrpParticipation(stock.Id, true);
            
            a.Should().Throw<ArgumentException>();

            mockRepository.Verify();
        }

        [Fact]
        public void ChangeDrpParticipation()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var transactionHandlers = mockRepository.Create<IServiceFactory<ITransactionHandler>>();
            transactionHandlers.Setup(x => x.GetService<OpeningBalance>()).Returns(new OpeningBalanceHandler());

            var portfolio = new Portfolio(Guid.NewGuid(), stockResolver.Object, transactionHandlers.Object);
            portfolio.AddOpeningBalance(stock.Id, new Date(1999, 01, 01), new Date(1999, 01, 01), 100, 100.00m, "", Guid.Empty);

            portfolio.ChangeDrpParticipation(stock.Id, true);

            portfolio.Holdings[stock.Id].Settings.ParticipateInDrp.Should().BeTrue();

            mockRepository.Verify();
        }

        [Fact]
        public void AddOpeningBalanceNoExistingHoldings()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var handler = mockRepository.Create<ITransactionHandler>();
            OpeningBalance transaction = null;
            handler.Setup(x => x.Apply(It.IsAny<IPortfolioTransaction>(), It.IsAny<IHolding>(), It.IsAny<ICashAccount>()))
                .Callback<IPortfolioTransaction, IHolding, ICashAccount>((t, h, c) => transaction = (OpeningBalance)t)
                .Verifiable();

            var transactionHandlers = mockRepository.Create<IServiceFactory<ITransactionHandler>>();
            transactionHandlers.Setup(x => x.GetService<OpeningBalance>()).Returns(handler.Object);

            var portfolio = new Portfolio(Guid.NewGuid(), stockResolver.Object, transactionHandlers.Object);

            var transactionId = Guid.NewGuid();
            portfolio.AddOpeningBalance(stock.Id, new Date(2000, 01, 01), new Date(1999, 01, 01), 100, 1000.00m, "Comment", transactionId);

            using (new AssertionScope())
            {
                portfolio.Holdings[stock.Id].EffectivePeriod.Should().Be(new DateRange(new Date(2000, 01, 01), Date.MaxValue));

                transaction.Should().BeEquivalentTo(new
                {
                    Id = transaction.Id,
                    Date = new Date(2000, 01, 01),
                    Stock = stock,
                    Comment = "Comment",
                    Units = 100,
                    CostBase = 1000.00m
            });

            }

            mockRepository.Verify();
        }

        [Fact]
        public void AddOpeningBalanceExistingHoldings()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var handler = mockRepository.Create<ITransactionHandler>();
            OpeningBalance transaction = null;
            handler.Setup(x => x.Apply(It.IsAny<IPortfolioTransaction>(), It.IsAny<IHolding>(), It.IsAny<ICashAccount>()))
                .Callback<IPortfolioTransaction, IHolding, ICashAccount>((t, h, c) => transaction = (OpeningBalance)t)
                .Verifiable();

            var transactionHandlers = mockRepository.Create<IServiceFactory<ITransactionHandler>>();
            transactionHandlers.Setup(x => x.GetService<OpeningBalance>()).Returns(handler.Object);

            var portfolio = new Portfolio(Guid.NewGuid(), stockResolver.Object, transactionHandlers.Object);

            var transactionId = Guid.NewGuid();
            portfolio.AddOpeningBalance(stock.Id, new Date(1999, 01, 01), new Date(1998, 01, 01), 10, 100.00m, "Existing", Guid.Empty);
            portfolio.AddOpeningBalance(stock.Id, new Date(2000, 01, 01), new Date(1999, 01, 01), 100, 1000.00m, "Comment", transactionId);

            transaction.Should().BeEquivalentTo(new
            {
                Id = transaction.Id,
                Date = new Date(2000, 01, 01),
                Stock = stock,
                Comment = "Comment",
                Units = 100,
                CostBase = 1000.00m
            });

            mockRepository.Verify();
        }

        [Fact]
        public void AdjustUnitCountNoHoldings()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var transactionHandlers = mockRepository.Create<IServiceFactory<ITransactionHandler>>();

            var portfolio = new Portfolio(Guid.NewGuid(), stockResolver.Object, transactionHandlers.Object);

            var transactionId = Guid.NewGuid();
            Action a = () => portfolio.AdjustUnitCount(stock.Id, new Date(2000, 01, 01), 1, 2, "Comment", transactionId);
            
            a.Should().Throw<NoSharesOwnedException>();

            mockRepository.Verify();
        }

        [Fact]
        public void AdjustUnitCount()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var handler = mockRepository.Create<ITransactionHandler>();
            UnitCountAdjustment transaction = null;
            handler.Setup(x => x.Apply(It.IsAny<IPortfolioTransaction>(), It.IsAny<IHolding>(), It.IsAny<ICashAccount>()))
                .Callback<IPortfolioTransaction, IHolding, ICashAccount>((t, h, c) => transaction = (UnitCountAdjustment)t)
                .Verifiable();

            var transactionHandlers = mockRepository.Create<IServiceFactory<ITransactionHandler>>();
            transactionHandlers.Setup(x => x.GetService<UnitCountAdjustment>()).Returns(handler.Object);
            transactionHandlers.Setup(x => x.GetService<OpeningBalance>()).Returns(new OpeningBalanceHandler());

            var portfolio = new Portfolio(Guid.NewGuid(), stockResolver.Object, transactionHandlers.Object);
            portfolio.AddOpeningBalance(stock.Id, new Date(1999, 01, 01), new Date(1999, 01, 01), 100, 100.00m, "", Guid.Empty);

            var transactionId = Guid.NewGuid();
            portfolio.AdjustUnitCount(stock.Id, new Date(2000, 01, 01), 1, 2, "Comment", transactionId);

            transaction.Should().BeEquivalentTo(new
            {
                Id = transaction.Id,
                Date = new Date(2000, 01, 01),
                Stock = stock,
                Comment = "Comment",
                OriginalUnits = 1,
                NewUnits = 2
            });

            mockRepository.Verify();
        }

        [Fact]
        public void AquireSharesNoExistingHoldings()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var handler = mockRepository.Create<ITransactionHandler>();
            Aquisition transaction = null;
            handler.Setup(x => x.Apply(It.IsAny<IPortfolioTransaction>(), It.IsAny<IHolding>(), It.IsAny<ICashAccount>()))
                .Callback<IPortfolioTransaction, IHolding, ICashAccount>((t, h, c) => transaction = (Aquisition)t)
                .Verifiable();

            var transactionHandlers = mockRepository.Create<IServiceFactory<ITransactionHandler>>();
            transactionHandlers.Setup(x => x.GetService<Aquisition>()).Returns(handler.Object);

            var portfolio = new Portfolio(Guid.NewGuid(), stockResolver.Object, transactionHandlers.Object);

            var transactionId = Guid.NewGuid();
            portfolio.AquireShares(stock.Id, new Date(2000, 01, 01), 100, 10.00m, 19.95m, true, "Comment", transactionId);

            using (new AssertionScope())
            {
                portfolio.Holdings[stock.Id].EffectivePeriod.Should().Be(new DateRange(new Date(2000, 01, 01), Date.MaxValue));            

                transaction.Should().BeEquivalentTo(new
                {
                    Id = transaction.Id,
                    Date = new Date(2000, 01, 01),
                    Stock = stock,
                    Comment = "Comment",
                    Units = 100,
                    AveragePrice = 10.00m,
                    TransactionCosts = 19.95m,
                    CreateCashTransaction = true
                });
            }

            mockRepository.Verify();
        }

        [Fact]
        public void AquireSharesExistingHoldings()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var handler = mockRepository.Create<ITransactionHandler>();
            Aquisition transaction = null;
            handler.Setup(x => x.Apply(It.IsAny<IPortfolioTransaction>(), It.IsAny<IHolding>(), It.IsAny<ICashAccount>()))
                .Callback<IPortfolioTransaction, IHolding, ICashAccount>((t, h, c) => transaction = (Aquisition)t)
                .Verifiable();

            var transactionHandlers = mockRepository.Create<IServiceFactory<ITransactionHandler>>();
            transactionHandlers.Setup(x => x.GetService<Aquisition>()).Returns(handler.Object);
            transactionHandlers.Setup(x => x.GetService<OpeningBalance>()).Returns(new OpeningBalanceHandler());

            var portfolio = new Portfolio(Guid.NewGuid(), stockResolver.Object, transactionHandlers.Object);
            portfolio.AddOpeningBalance(stock.Id, new Date(1999, 01, 01), new Date(1999, 01, 01), 100, 100.00m, "", Guid.Empty);

            var transactionId = Guid.NewGuid();
            portfolio.AquireShares(stock.Id, new Date(2000, 01, 01), 100, 10.00m, 19.95m, true, "Comment", transactionId);

            transaction.Should().BeEquivalentTo(new
            {
                Id = transaction.Id,
                Date = new Date(2000, 01, 01),
                Stock = stock,
                Comment = "Comment",
                Units = 100,
                AveragePrice = 10.00m,
                TransactionCosts = 19.95m,
                CreateCashTransaction = true
            });

            mockRepository.Verify();
        }

        [Fact]
        public void DisposeOfSharesNoHoldings()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var transactionHandlers = mockRepository.Create<IServiceFactory<ITransactionHandler>>();

            var portfolio = new Portfolio(Guid.NewGuid(), stockResolver.Object, transactionHandlers.Object);

            var transactionId = Guid.NewGuid();
            Action a = () => portfolio.DisposeOfShares(stock.Id, new Date(2000, 01, 01), 100, 10.00m, 19.95m, CgtCalculationMethod.MinimizeGain, true, "Comment", transactionId);
            
            a.Should().Throw<NoSharesOwnedException>();

            mockRepository.Verify();
        }

        [Fact]
        public void DisposeOfShares()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var handler = mockRepository.Create<ITransactionHandler>();
            Disposal transaction = null;
            handler.Setup(x => x.Apply(It.IsAny<IPortfolioTransaction>(), It.IsAny<IHolding>(), It.IsAny<ICashAccount>()))
                .Callback<IPortfolioTransaction, IHolding, ICashAccount>((t, h, c) => transaction = (Disposal)t)
                .Verifiable();

            var transactionHandlers = mockRepository.Create<IServiceFactory<ITransactionHandler>>();
            transactionHandlers.Setup(x => x.GetService<Disposal>()).Returns(handler.Object);
            transactionHandlers.Setup(x => x.GetService<OpeningBalance>()).Returns(new OpeningBalanceHandler());

            var portfolio = new Portfolio(Guid.NewGuid(), stockResolver.Object, transactionHandlers.Object);
            portfolio.AddOpeningBalance(stock.Id, new Date(1999, 01, 01), new Date(1999, 01, 01), 100, 100.00m, "", Guid.Empty);

            var transactionId = Guid.NewGuid();
            portfolio.DisposeOfShares(stock.Id, new Date(2000, 01, 01), 100, 10.00m, 19.95m, CgtCalculationMethod.MinimizeGain, true, "Comment", transactionId);

            transaction.Should().BeEquivalentTo(new
            {
                Id = transaction.Id,
                Date = new Date(2000, 01, 01),
                Stock = stock,
                Comment = "Comment",
                Units = 100,
                AveragePrice = 10.00m,
                TransactionCosts = 19.95m,
                CreateCashTransaction = true,
                CgtMethod = CgtCalculationMethod.MinimizeGain
            });

            mockRepository.Verify();
        }

        [Fact]
        public void IncomeReceivedNoHoldings()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var transactionHandlers = mockRepository.Create<IServiceFactory<ITransactionHandler>>();

            var portfolio = new Portfolio(Guid.NewGuid(), stockResolver.Object, transactionHandlers.Object);

            var transactionId = Guid.NewGuid();
            Action a = () => portfolio.IncomeReceived(stock.Id, new Date(2000, 01, 01), new Date(2000, 02, 01), 100.00m, 101.00m, 30.00m, 2.00m, 3.00m, 20.00m, true, "Comment", transactionId);
            
            a.Should().Throw<NoSharesOwnedException>();

            mockRepository.Verify();
        }

        [Fact]
        public void IncomeReceived()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var handler = mockRepository.Create<ITransactionHandler>();
            IncomeReceived transaction = null;
            handler.Setup(x => x.Apply(It.IsAny<IPortfolioTransaction>(), It.IsAny<IHolding>(), It.IsAny<ICashAccount>()))
                .Callback<IPortfolioTransaction, IHolding, ICashAccount>((t, h, c) => transaction = (IncomeReceived)t)
                .Verifiable();

            var transactionHandlers = mockRepository.Create<IServiceFactory<ITransactionHandler>>();
            transactionHandlers.Setup(x => x.GetService<IncomeReceived>()).Returns(handler.Object);
            transactionHandlers.Setup(x => x.GetService<OpeningBalance>()).Returns(new OpeningBalanceHandler());

            var portfolio = new Portfolio(Guid.NewGuid(), stockResolver.Object, transactionHandlers.Object);
            portfolio.AddOpeningBalance(stock.Id, new Date(1999, 01, 01), new Date(1999, 01, 01), 100, 100.00m, "", Guid.Empty);

            var transactionId = Guid.NewGuid();
            portfolio.IncomeReceived(stock.Id, new Date(2000, 01, 01), new Date(2000, 02, 01), 100.00m, 101.00m, 30.00m, 2.00m, 3.00m, 20.00m, true, "Comment", transactionId);

            transaction.Should().BeEquivalentTo(new
            {
                Id = transaction.Id,
                Date = new Date(2000, 02, 01),
                Stock = stock,
                Comment = "Comment",
                RecordDate = new Date(2000, 01, 01),
                FrankedAmount = 100.00m,
                UnfrankedAmount = 101.00m,
                FrankingCredits = 30.00m,
                Interest = 2.00m,
                TaxDeferred = 3.00m,
                DrpCashBalance = 20.00m,
                CreateCashTransaction = true
            });

            mockRepository.Verify();
        }

        [Fact]
        public void MakeCashTransaction()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockResolver = mockRepository.Create<IStockResolver>();

            var handler = mockRepository.Create<ITransactionHandler>();
            CashTransaction transaction = null;
            handler.Setup(x => x.Apply(It.IsAny<IPortfolioTransaction>(), It.IsAny<IHolding>(), It.IsAny<ICashAccount>()))
                .Callback<IPortfolioTransaction, IHolding, ICashAccount>((t, h, c) => transaction = (CashTransaction)t)
                .Verifiable();

            var transactionHandlers = mockRepository.Create<IServiceFactory<ITransactionHandler>>();
            transactionHandlers.Setup(x => x.GetService<CashTransaction>()).Returns(handler.Object);

            var portfolio = new Portfolio(Guid.NewGuid(), stockResolver.Object, transactionHandlers.Object);

            var transactionId = Guid.NewGuid();
            portfolio.MakeCashTransaction(new Date(2000, 01, 01), BankAccountTransactionType.Transfer, 100.00m, "Comment", transactionId);

            transaction.Should().BeEquivalentTo(new
            {
                Id = transaction.Id,
                Date = new Date(2000, 01, 01),
                Comment = "Comment",
                CashTransactionType = BankAccountTransactionType.Transfer,
                Amount = 100.00m
            });

            mockRepository.Verify();
        }

        [Fact]
        public void ReturnOfCapitalReceivedNoHoldings()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var transactionHandlers = mockRepository.Create<IServiceFactory<ITransactionHandler>>();

            var portfolio = new Portfolio(Guid.NewGuid(), stockResolver.Object, transactionHandlers.Object);

            var transactionId = Guid.NewGuid();
            Action a = () => portfolio.ReturnOfCapitalReceived(stock.Id, new Date(2000, 01, 01), new Date(1999, 01, 01), 10.00m, true, "Comment", transactionId);
            
            a.Should().Throw<NoSharesOwnedException>();

            mockRepository.Verify();
        }

        [Fact]
        public void ReturnOfCapitalReceived()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var handler = mockRepository.Create<ITransactionHandler>();
            ReturnOfCapital transaction = null;
            handler.Setup(x => x.Apply(It.IsAny<IPortfolioTransaction>(), It.IsAny<IHolding>(), It.IsAny<ICashAccount>()))
                .Callback<IPortfolioTransaction, IHolding, ICashAccount>((t, h, c) => transaction = (ReturnOfCapital)t)
                .Verifiable();

            var transactionHandlers = mockRepository.Create<IServiceFactory<ITransactionHandler>>();
            transactionHandlers.Setup(x => x.GetService<ReturnOfCapital>()).Returns(handler.Object);
            transactionHandlers.Setup(x => x.GetService<OpeningBalance>()).Returns(new OpeningBalanceHandler());

            var portfolio = new Portfolio(Guid.NewGuid(), stockResolver.Object, transactionHandlers.Object);
            portfolio.AddOpeningBalance(stock.Id, new Date(1999, 01, 01), new Date(1999, 01, 01), 100, 100.00m, "", Guid.Empty);

            var transactionId = Guid.NewGuid();
            portfolio.ReturnOfCapitalReceived(stock.Id, new Date(2000, 01, 01), new Date(1999, 01, 01), 10.00m, true, "Comment", transactionId);

            transaction.Should().BeEquivalentTo(new
            {
                Id = transaction.Id,
                Date = new Date(2000, 01, 01),
                Stock = stock,
                Comment = "Comment",
                RecordDate = new Date(1999, 01, 01),
                Amount = 10.00m,
                CreateCashTransaction = true
            });

            mockRepository.Verify();
        }
    }
}
