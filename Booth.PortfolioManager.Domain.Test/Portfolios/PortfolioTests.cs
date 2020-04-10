using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.Test.Portfolios
{
    class PortfolioTests
    {
        [TestCase]
        public void Create()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockResolver = mockRepository.Create<IStockResolver>();
            var transactionHandlers = mockRepository.Create<IServiceFactory<ITransactionHandler>>();

            var id = Guid.NewGuid();
            var portfolio = new Portfolio(id, stockResolver.Object, transactionHandlers.Object);

            var owner = Guid.NewGuid();
            portfolio.Create("Test", owner);
           
            Assert.Multiple(() =>
            {
                Assert.That(portfolio.Id, Is.EqualTo(id));
                Assert.That(portfolio.Name, Is.EqualTo("Test"));
                Assert.That(portfolio.Owner, Is.EqualTo(owner));
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void ChangeDrpParticipationHoldingNotOwned()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var transactionHandlers = mockRepository.Create<IServiceFactory<ITransactionHandler>>();

            var portfolio = new Portfolio(Guid.NewGuid(), stockResolver.Object, transactionHandlers.Object);

            Assert.That(() => portfolio.ChangeDrpParticipation(stock.Id, true), Throws.ArgumentException);

            mockRepository.Verify();
        }

        [TestCase]
        public void ChangeDrpParticipation()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var transactionHandlers = mockRepository.Create<IServiceFactory<ITransactionHandler>>();
            transactionHandlers.Setup(x => x.GetService<OpeningBalance>()).Returns(new OpeningBalanceHandler());

            var portfolio = new Portfolio(Guid.NewGuid(), stockResolver.Object, transactionHandlers.Object);
            portfolio.AddOpeningBalance(stock.Id, Date.MinValue, Date.MinValue, 100, 100.00m, "", Guid.Empty);

            portfolio.ChangeDrpParticipation(stock.Id, true);

            Assert.That(portfolio.Holdings[stock.Id].Settings.ParticipateInDrp, Is.True);

            mockRepository.Verify();
        }

        [TestCase]
        public void AddOpeningBalanceNoExistingHoldings()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

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

            Assert.Multiple(() =>
            {
                var holding = portfolio.Holdings[stock.Id];
                Assert.That(holding.EffectivePeriod.FromDate, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(holding.EffectivePeriod.ToDate, Is.EqualTo(Date.MaxValue));

                Assert.That(transaction.Id, Is.EqualTo(transaction.Id));
                Assert.That(transaction.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(transaction.Stock, Is.EqualTo(stock));
                Assert.That(transaction.Comment, Is.EqualTo("Comment"));
                Assert.That(transaction.Units, Is.EqualTo(100));
                Assert.That(transaction.CostBase, Is.EqualTo(1000.00m));
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void AddOpeningBalanceExistingHoldings()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

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

            Assert.Multiple(() =>
            {
                Assert.That(transaction.Id, Is.EqualTo(transaction.Id));
                Assert.That(transaction.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(transaction.Stock, Is.EqualTo(stock));
                Assert.That(transaction.Comment, Is.EqualTo("Comment"));
                Assert.That(transaction.Units, Is.EqualTo(100));
                Assert.That(transaction.CostBase, Is.EqualTo(1000.00m));
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void AdjustUnitCountNoHoldings()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var transactionHandlers = mockRepository.Create<IServiceFactory<ITransactionHandler>>();

            var portfolio = new Portfolio(Guid.NewGuid(), stockResolver.Object, transactionHandlers.Object);

            var transactionId = Guid.NewGuid();
            Assert.That(() => portfolio.AdjustUnitCount(stock.Id, new Date(2000, 01, 01), 1, 2, "Comment", transactionId), Throws.TypeOf(typeof(NoSharesOwned)));

            mockRepository.Verify();
        }

        [TestCase]
        public void AdjustUnitCount()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

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
            portfolio.AddOpeningBalance(stock.Id, Date.MinValue, Date.MinValue, 100, 100.00m, "", Guid.Empty);

            var transactionId = Guid.NewGuid();
            portfolio.AdjustUnitCount(stock.Id, new Date(2000, 01, 01), 1, 2, "Comment", transactionId);

            Assert.Multiple(() =>
            {
                Assert.That(transaction.Id, Is.EqualTo(transaction.Id));
                Assert.That(transaction.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(transaction.Stock, Is.EqualTo(stock));
                Assert.That(transaction.Comment, Is.EqualTo("Comment"));
                Assert.That(transaction.OriginalUnits, Is.EqualTo(1));
                Assert.That(transaction.NewUnits, Is.EqualTo(2));
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void AquireSharesNoExistingHoldings()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

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

            Assert.Multiple(() =>
            {
                var holding = portfolio.Holdings[stock.Id];
                Assert.That(holding.EffectivePeriod.FromDate, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(holding.EffectivePeriod.ToDate, Is.EqualTo(Date.MaxValue));

                Assert.That(transaction.Id, Is.EqualTo(transaction.Id));
                Assert.That(transaction.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(transaction.Stock, Is.EqualTo(stock));
                Assert.That(transaction.Comment, Is.EqualTo("Comment"));
                Assert.That(transaction.Units, Is.EqualTo(100));
                Assert.That(transaction.AveragePrice, Is.EqualTo(10.00m));
                Assert.That(transaction.TransactionCosts, Is.EqualTo(19.95m));
                Assert.That(transaction.CreateCashTransaction, Is.EqualTo(true));
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void AquireSharesExistingHoldings()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

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
            portfolio.AddOpeningBalance(stock.Id, Date.MinValue, Date.MinValue, 100, 100.00m, "", Guid.Empty);

            var transactionId = Guid.NewGuid();
            portfolio.AquireShares(stock.Id, new Date(2000, 01, 01), 100, 10.00m, 19.95m, true, "Comment", transactionId);

            Assert.Multiple(() =>
            {
                Assert.That(transaction.Id, Is.EqualTo(transaction.Id));
                Assert.That(transaction.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(transaction.Stock, Is.EqualTo(stock));
                Assert.That(transaction.Comment, Is.EqualTo("Comment"));
                Assert.That(transaction.Units, Is.EqualTo(100));
                Assert.That(transaction.AveragePrice, Is.EqualTo(10.00m));
                Assert.That(transaction.TransactionCosts, Is.EqualTo(19.95m));
                Assert.That(transaction.CreateCashTransaction, Is.EqualTo(true));
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void DisposeOfSharesNoHoldings()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var transactionHandlers = mockRepository.Create<IServiceFactory<ITransactionHandler>>();

            var portfolio = new Portfolio(Guid.NewGuid(), stockResolver.Object, transactionHandlers.Object);

            var transactionId = Guid.NewGuid();
            Assert.That(() => portfolio.DisposeOfShares(stock.Id, new Date(2000, 01, 01), 100, 10.00m, 19.95m, CgtCalculationMethod.MinimizeGain, true, "Comment", transactionId), Throws.TypeOf(typeof(NoSharesOwned)));

            mockRepository.Verify();
        }

        [TestCase]
        public void DisposeOfShares()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

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
            portfolio.AddOpeningBalance(stock.Id, Date.MinValue, Date.MinValue, 100, 100.00m, "", Guid.Empty);

            var transactionId = Guid.NewGuid();
            portfolio.DisposeOfShares(stock.Id, new Date(2000, 01, 01), 100, 10.00m, 19.95m, CgtCalculationMethod.MinimizeGain, true, "Comment", transactionId);

            Assert.Multiple(() =>
            {
                Assert.That(transaction.Id, Is.EqualTo(transaction.Id));
                Assert.That(transaction.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(transaction.Stock, Is.EqualTo(stock));
                Assert.That(transaction.Comment, Is.EqualTo("Comment"));
                Assert.That(transaction.Units, Is.EqualTo(100));
                Assert.That(transaction.AveragePrice, Is.EqualTo(10.00m));
                Assert.That(transaction.TransactionCosts, Is.EqualTo(19.95m));
                Assert.That(transaction.CreateCashTransaction, Is.EqualTo(true));
                Assert.That(transaction.CgtMethod, Is.EqualTo(CgtCalculationMethod.MinimizeGain));
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void IncomeReceivedNoHoldings()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var transactionHandlers = mockRepository.Create<IServiceFactory<ITransactionHandler>>();

            var portfolio = new Portfolio(Guid.NewGuid(), stockResolver.Object, transactionHandlers.Object);

            var transactionId = Guid.NewGuid();
            Assert.That(() => portfolio.IncomeReceived(stock.Id, new Date(2000, 01, 01), new Date(2000, 02, 01), 100.00m, 101.00m, 30.00m, 2.00m, 3.00m, 20.00m, true, "Comment", transactionId), Throws.TypeOf(typeof(NoSharesOwned)));

            mockRepository.Verify();
        }

        [TestCase]
        public void IncomeReceived()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

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
            portfolio.AddOpeningBalance(stock.Id, Date.MinValue, Date.MinValue, 100, 100.00m, "", Guid.Empty);

            var transactionId = Guid.NewGuid();
            portfolio.IncomeReceived(stock.Id, new Date(2000, 01, 01), new Date(2000, 02, 01), 100.00m, 101.00m, 30.00m, 2.00m, 3.00m, 20.00m, true, "Comment", transactionId);

            Assert.Multiple(() =>
            {
                Assert.That(transaction.Id, Is.EqualTo(transaction.Id));
                Assert.That(transaction.Date, Is.EqualTo(new Date(2000, 02, 01)));
                Assert.That(transaction.Stock, Is.EqualTo(stock));
                Assert.That(transaction.Comment, Is.EqualTo("Comment"));
                Assert.That(transaction.RecordDate, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(transaction.FrankedAmount, Is.EqualTo(100.00m));
                Assert.That(transaction.UnfrankedAmount, Is.EqualTo(101.00m));
                Assert.That(transaction.FrankingCredits, Is.EqualTo(30.00m));
                Assert.That(transaction.Interest, Is.EqualTo(2.00m));
                Assert.That(transaction.TaxDeferred, Is.EqualTo(3.00m));
                Assert.That(transaction.DRPCashBalance, Is.EqualTo(20.00m));
                Assert.That(transaction.CreateCashTransaction, Is.EqualTo(true));
            });

            mockRepository.Verify();
        }

        [TestCase]
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

            Assert.Multiple(() =>
            {
                Assert.That(transaction.Id, Is.EqualTo(transaction.Id));
                Assert.That(transaction.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(transaction.Stock, Is.Null);
                Assert.That(transaction.Comment, Is.EqualTo("Comment"));
                Assert.That(transaction.CashTransactionType, Is.EqualTo(BankAccountTransactionType.Transfer));
                Assert.That(transaction.Amount, Is.EqualTo(100.00m));
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void ReturnOfCapitalReceivedNoHoldings()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var transactionHandlers = mockRepository.Create<IServiceFactory<ITransactionHandler>>();

            var portfolio = new Portfolio(Guid.NewGuid(), stockResolver.Object, transactionHandlers.Object);

            var transactionId = Guid.NewGuid();
            Assert.That(() => portfolio.ReturnOfCapitalReceived(stock.Id, new Date(2000, 01, 01), new Date(1999, 01, 01), 10.00m, true, "Comment", transactionId), Throws.TypeOf(typeof(NoSharesOwned)));

            mockRepository.Verify();
        }

        [TestCase]
        public void ReturnOfCapitalReceived()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

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
            portfolio.AddOpeningBalance(stock.Id, Date.MinValue, Date.MinValue, 100, 100.00m, "", Guid.Empty);

            var transactionId = Guid.NewGuid();
            portfolio.ReturnOfCapitalReceived(stock.Id, new Date(2000, 01, 01), new Date(1999, 01, 01), 10.00m, true, "Comment", transactionId);

            Assert.Multiple(() =>
            {
                Assert.That(transaction.Id, Is.EqualTo(transaction.Id));
                Assert.That(transaction.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(transaction.Stock, Is.EqualTo(stock));
                Assert.That(transaction.Comment, Is.EqualTo("Comment"));
                Assert.That(transaction.RecordDate, Is.EqualTo(new Date(1999, 01, 01)));
                Assert.That(transaction.Amount, Is.EqualTo(10.00m));
                Assert.That(transaction.CreateCashTransaction, Is.EqualTo(true));
            });

            mockRepository.Verify();
        }
    }
}
