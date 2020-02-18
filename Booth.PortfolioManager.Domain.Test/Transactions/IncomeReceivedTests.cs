using System;
using System.Linq;

using NUnit.Framework;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;

namespace Booth.PortfolioManager.Domain.Test.Transactions
{
    class IncomeReceivedTests
    {
        [TestCase]
        public void NoSharesOwned()
        {
            Assert.That(false);
            /*
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

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

            var holding = Mock.Of<IHolding>(MockBehavior.Strict);

            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            Mock.Get(holdings).Setup(x => x.Get(stock.Id)).Returns(default(IHolding));

            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);

            var handler = new IncomeReceivedHandler(holdings, cashAccount);

            Assert.That(() => handler.ApplyTransaction(transaction), Throws.Exception.InstanceOf(typeof(NoParcelsForTransaction)));*/
        }

        [TestCase]
        public void NoSharesAtRecordDated()
        {Assert.That(false);
            /*
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

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

            var holding = Mock.Of<IHolding>(MockBehavior.Strict);
            Mock.Get(holding).Setup(x => x.IsEffectiveAt(new Date(2020, 01, 01))).Returns(false);

            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            Mock.Get(holdings).Setup(x => x.Get(stock.Id)).Returns(holding);

            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);

            var handler = new IncomeReceivedHandler(holdings, cashAccount);

            Assert.That(() => handler.ApplyTransaction(transaction), Throws.Exception.InstanceOf(typeof(NoParcelsForTransaction)));*/
        }

        [TestCase]
        public void DoNotCreateCashTransaction()
        {
            Assert.That(false);
            /*var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

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

            var drpAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);
            Mock.Get(drpAccount).Setup(x => x.Balance(new Date(2020, 02, 01))).Returns(0.00m);

            var holding = Mock.Of<IHolding>(MockBehavior.Strict);
            Mock.Get(holding).Setup(x => x.IsEffectiveAt(new Date(2020, 01, 01))).Returns(true);
            Mock.Get(holding).Setup(x => x.DrpAccount).Returns(drpAccount);

            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            Mock.Get(holdings).Setup(x => x.Get(stock.Id)).Returns(holding);

            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);

            var handler = new IncomeReceivedHandler(holdings, cashAccount);

            handler.ApplyTransaction(transaction);*/
        }

        [TestCase]
        public void CreateCashTransaction()
        {
            Assert.That(false);
            /*var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

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
                CreateCashTransaction = true,
                DRPCashBalance = 0.00m
            };

            var drpAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);
            Mock.Get(drpAccount).Setup(x => x.Balance(new Date(2020, 02, 01))).Returns(0.00m);

            var holding = Mock.Of<IHolding>(MockBehavior.Strict);
            Mock.Get(holding).Setup(x => x.IsEffectiveAt(new Date(2020, 01, 01))).Returns(true);
            Mock.Get(holding).Setup(x => x.DrpAccount).Returns(drpAccount);

            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            Mock.Get(holdings).Setup(x => x.Get(stock.Id)).Returns(holding);

            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);
            Mock.Get(cashAccount).Setup(x => x.Transfer(new Date(2020, 02, 01), 70.00m, "Distribution for ABC"));

            var handler = new IncomeReceivedHandler(holdings, cashAccount);

            handler.ApplyTransaction(transaction);*/
        }

        [TestCase]
        public void OutstandingDrpBalance()
        {
            Assert.That(false);
            /*var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

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
                DRPCashBalance = 50.00m
            };

            var drpAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);
            Mock.Get(drpAccount).Setup(x => x.Balance(new Date(2020, 02, 01))).Returns(100.00m);

            var holding = Mock.Of<IHolding>(MockBehavior.Strict);
            Mock.Get(holding).Setup(x => x.IsEffectiveAt(new Date(2020, 01, 01))).Returns(true);
            Mock.Get(holding).Setup(x => x.DrpAccount).Returns(drpAccount);
            Mock.Get(holding).Setup(x => x.AddDrpAccountAmount(new Date(2020, 02, 01), -50.00m));

            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            Mock.Get(holdings).Setup(x => x.Get(stock.Id)).Returns(holding);

            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);

            var handler = new IncomeReceivedHandler(holdings, cashAccount);

            handler.ApplyTransaction(transaction);*/
        }

        [TestCase]
        public void TaxDeferredAmount()
        {
         Assert.That(false);
            /*   var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

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
                TaxDeferred = 50.00m,
                CreateCashTransaction = false,
                DRPCashBalance = 0.00m
            };

            var drpAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);
            Mock.Get(drpAccount).Setup(x => x.Balance(new Date(2020, 02, 01))).Returns(0.00m);

            var parcel1 = Mock.Of<IParcel>(MockBehavior.Strict);
            Mock.Get(parcel1).Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(100, 1000.00m, 1000.00m));
            Mock.Get(parcel1).Setup(x => x.Change(new Date(2020, 02, 01), 0, 0.00m, 100.00m, transaction));
            var parcel2 = Mock.Of<IParcel>(MockBehavior.Strict);
            Mock.Get(parcel2).Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(50, 500.00m, 500.00m));
            Mock.Get(parcel1).Setup(x => x.Change(new Date(2020, 02, 01), 0, 0.00m, 100.00m, transaction));
            var parcel3 = Mock.Of<IParcel>(MockBehavior.Strict);
            Mock.Get(parcel2).Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(200, 1000.00m, 1000.00m));
            Mock.Get(parcel1).Setup(x => x.Change(new Date(2020, 02, 01), 0, 0.00m, 100.00m, transaction));

            var holding = Mock.Of<IHolding>(MockBehavior.Strict);
            Mock.Get(holding).Setup(x => x.IsEffectiveAt(new Date(2020, 01, 01))).Returns(true);
            Mock.Get(holding).Setup(x => x.DrpAccount).Returns(drpAccount);
            Mock.Get(holding).Setup(x => x.Parcels(new Date(2020, 01, 01))).Returns(new IParcel[] { parcel1, parcel2, parcel3 });

            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            Mock.Get(holdings).Setup(x => x.Get(stock.Id)).Returns(holding);

            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);

            var handler = new IncomeReceivedHandler(holdings, cashAccount);

            handler.ApplyTransaction(transaction);*/
        }

        [TestCase]
        public void TaxDeferredAmount2()
        {
           Assert.That(false);
            /* var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

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
                TaxDeferred = 50.00m,
                CreateCashTransaction = false,
                DRPCashBalance = 0.00m
            };

            var holdings = new HoldingCollection();

            var holding = holdings.Add(stock, new Date(2010, 01, 01));
            var parcel1 = holding.AddParcel(new Date(2010, 01, 01), new Date(2010, 01, 01), 100, 1000.00m, 1000.00m, transaction);
            var parcel2 = holding.AddParcel(new Date(2010, 01, 01), new Date(2010, 01, 01), 50, 500.00m, 500.00m, transaction);
            var parcel3 = holding.AddParcel(new Date(2010, 01, 01), new Date(2010, 01, 01), 200, 1000.00m, 1000.00m, transaction);

            var cashAccount = new CashAccount();

            var handler = new IncomeReceivedHandler(holdings, cashAccount);

            handler.ApplyTransaction(transaction);

            Assert.Multiple(() =>
            {
                Assert.That(cashAccount.Transactions.Count, Is.EqualTo(0));

                var properties1 = parcel1.Properties[new Date(2020, 01, 01)];
                Assert.That(properties1.Units, Is.EqualTo(100));
                Assert.That(properties1.Amount, Is.EqualTo(1000.00m));
                Assert.That(properties1.CostBase, Is.EqualTo(1000.00m));

                var properties2 = parcel2.Properties[new Date(2020, 01, 01)];
                Assert.That(properties2.Units, Is.EqualTo(100));
                Assert.That(properties2.Amount, Is.EqualTo(1000.00m));
                Assert.That(properties2.CostBase, Is.EqualTo(1000.00m));

                var properties3 = parcel1.Properties[new Date(2020, 01, 01)];
                Assert.That(properties3.Units, Is.EqualTo(100));
                Assert.That(properties3.Amount, Is.EqualTo(1000.00m));
                Assert.That(properties3.CostBase, Is.EqualTo(1000.00m));
            });*/
        }

        [TestCase]
        public void IncorrectTransactionType()
        {
            var transaction = new CashTransaction()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 01, 01),
                Comment = "Test Deposit",
                CashTransactionType = BankAccountTransactionType.Deposit,
                Amount = 100.00m
            };

            var holdings = new HoldingCollection();
            var cashAccount = new CashAccount();

            var handler = new IncomeReceivedHandler(holdings, cashAccount);

            Assert.That(() => handler.ApplyTransaction(transaction), Throws.ArgumentException);
        }

    }
}
