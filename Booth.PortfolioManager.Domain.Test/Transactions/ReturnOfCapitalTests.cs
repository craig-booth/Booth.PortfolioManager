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
    class ReturnOfCapitalTests
    {

        [TestCase]
        public void NoSharesOwned()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var transaction = new ReturnOfCapital()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Dividend",
                RecordDate = new Date(2020, 01, 01),
                Amount = 2.00m,
                CreateCashTransaction = false
            };

            var holding = Mock.Of<IHolding>(MockBehavior.Strict);

            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            Mock.Get(holdings).Setup(x => x[stock.Id]).Returns(default(IHolding));

            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);

            var handler = new ReturnOfCapitalHandler(holdings, cashAccount);

            Assert.That(() => handler.ApplyTransaction(transaction), Throws.Exception.InstanceOf(typeof(NoParcelsForTransaction)));
        }

        [TestCase]
        public void SingleParcelOwned()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var transaction = new ReturnOfCapital()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Dividend",
                RecordDate = new Date(2020, 01, 01),
                Amount = 2.00m,
                CreateCashTransaction = true
            };

            var parcel = Mock.Of<IParcel>(MockBehavior.Strict);
            Mock.Get(parcel).Setup(x => x.Properties[new Date(2020, 01, 01)]).Returns(new ParcelProperties(50, 1000.00m, 1500.00m));
            Mock.Get(parcel).Setup(x => x.Change(new Date(2020, 01, 01), 0, 0.00m, 100.00m, transaction)).Verifiable();

            var holding = Mock.Of<IHolding>(MockBehavior.Strict);
            Mock.Get(holding).Setup(x => x.IsEffectiveAt(new Date(2020, 01, 01))).Returns(true);
            Mock.Get(holding).Setup(x => x[new Date(2020, 01, 01)]).Returns(new IParcel[] { parcel });
    
            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            Mock.Get(holdings).Setup(x => x[stock.Id]).Returns(holding);

            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);
            Mock.Get(cashAccount).Setup(x => x.Transfer(new Date(2020, 02, 01), 100.00m, "Return of capital for ABC")).Verifiable(); 

            var handler = new ReturnOfCapitalHandler(holdings, cashAccount);
            handler.ApplyTransaction(transaction);

            Mock.Verify(new Mock[] { Mock.Get(parcel), Mock.Get(holding), Mock.Get(holdings), Mock.Get(cashAccount) });
        }

        [TestCase]
        public void MultipleParcelsOwned()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var transaction = new ReturnOfCapital()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Dividend",
                RecordDate = new Date(2020, 01, 01),
                Amount = 2.00m,
                CreateCashTransaction = true
            };

            var parcel1 = Mock.Of<IParcel>(MockBehavior.Strict);
            Mock.Get(parcel1).Setup(x => x.Properties[new Date(2020, 01, 01)]).Returns(new ParcelProperties(100, 1000.00m, 1000.00m));
            Mock.Get(parcel1).Setup(x => x.Change(new Date(2020, 01, 01), 0, 0.00m, 200.00m, transaction)).Verifiable();
            var parcel2 = Mock.Of<IParcel>(MockBehavior.Strict);
            Mock.Get(parcel2).Setup(x => x.Properties[new Date(2020, 01, 01)]).Returns(new ParcelProperties(50, 500.00m, 500.00m));
            Mock.Get(parcel2).Setup(x => x.Change(new Date(2020, 01, 01), 0, 0.00m, 100.00m, transaction)).Verifiable();
            var parcel3 = Mock.Of<IParcel>(MockBehavior.Strict);
            Mock.Get(parcel3).Setup(x => x.Properties[new Date(2020, 01, 01)]).Returns(new ParcelProperties(200, 1000.00m, 1000.00m));
            Mock.Get(parcel3).Setup(x => x.Change(new Date(2020, 01, 01), 0, 0.00m, 400.00m, transaction)).Verifiable();

            var holding = Mock.Of<IHolding>(MockBehavior.Strict);
            Mock.Get(holding).Setup(x => x.IsEffectiveAt(new Date(2020, 01, 01))).Returns(true);
            Mock.Get(holding).Setup(x => x[new Date(2020, 01, 01)]).Returns(new IParcel[] { parcel1, parcel2, parcel3 });

            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            Mock.Get(holdings).Setup(x => x[stock.Id]).Returns(holding);

            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);
            Mock.Get(cashAccount).Setup(x => x.Transfer(new Date(2020, 02, 01), 700.00m, "Return of capital for ABC")).Verifiable();

            var handler = new ReturnOfCapitalHandler(holdings, cashAccount);
            handler.ApplyTransaction(transaction);

            Mock.Verify(new Mock[] { Mock.Get(parcel1), Mock.Get(parcel2), Mock.Get(parcel3), Mock.Get(holding), Mock.Get(holdings), Mock.Get(cashAccount) });
        }


        [TestCase]
        public void NoCashTransaction()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var transaction = new ReturnOfCapital()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Dividend",
                RecordDate = new Date(2020, 01, 01),
                Amount = 2.00m,
                CreateCashTransaction = false
            };

            var parcel = Mock.Of<IParcel>(MockBehavior.Strict);
            Mock.Get(parcel).Setup(x => x.Properties[new Date(2020, 01, 01)]).Returns(new ParcelProperties(50, 1000.00m, 1500.00m));
            Mock.Get(parcel).Setup(x => x.Change(new Date(2020, 01, 01), 0, 0.00m, 100.00m, transaction)).Verifiable();

            var holding = Mock.Of<IHolding>(MockBehavior.Strict);
            Mock.Get(holding).Setup(x => x.IsEffectiveAt(new Date(2020, 01, 01))).Returns(true);
            Mock.Get(holding).Setup(x => x[new Date(2020, 01, 01)]).Returns(new IParcel[] { parcel });

            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            Mock.Get(holdings).Setup(x => x[stock.Id]).Returns(holding);

            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);
  
            var handler = new ReturnOfCapitalHandler(holdings, cashAccount);
            handler.ApplyTransaction(transaction);

            Mock.Verify(new Mock[] { Mock.Get(parcel), Mock.Get(holding), Mock.Get(holdings), Mock.Get(cashAccount) });
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

            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);

            var handler = new ReturnOfCapitalHandler(holdings, cashAccount);

            Assert.That(() => handler.ApplyTransaction(transaction), Throws.ArgumentException); 
        }
    }
}
