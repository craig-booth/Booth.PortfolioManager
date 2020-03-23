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
    class UnitCountAdjustmentTests
    {
        [TestCase]
        public void NoSharesOwned()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var transaction = new UnitCountAdjustment()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Stock Split",
                OriginalUnits = 2,
                NewUnits = 3
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var holding = mockRepository.Create<IHolding>();

            var holdings = mockRepository.Create<IHoldingCollection>();
            holdings.Setup(x => x[stock.Id]).Returns(default(IHolding));

            var handler = new UnitCountAdjustmentHandler(holdings.Object);

            Assert.That(() => handler.ApplyTransaction(transaction), Throws.TypeOf(typeof(NoParcelsForTransaction)));

            mockRepository.Verify();
        }

        [TestCase]
        public void SingleParcelOwned()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var transaction = new UnitCountAdjustment()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Stock Split",
                OriginalUnits = 2,
                NewUnits = 3
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var parcel = mockRepository.Create<IParcel>();
            parcel.Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(50, 1000.00m, 1500.00m));
            parcel.Setup(x => x.Change(new Date(2020, 02, 01), 75, 0.00m, 0.00m, transaction)).Verifiable();

            var holding = mockRepository.Create<IHolding>();
            holding.Setup(x => x.IsEffectiveAt(new Date(2020, 02, 01))).Returns(true);
            holding.Setup(x => x[new Date(2020, 02, 01)]).Returns(new IParcel[] { parcel.Object });

            var holdings = mockRepository.Create<IHoldingCollection>();
            holdings.Setup(x => x[stock.Id]).Returns(holding.Object);


            var handler = new UnitCountAdjustmentHandler(holdings.Object);
            handler.ApplyTransaction(transaction);

            mockRepository.Verify();
        }

        [TestCase]
        public void MultipleParcelsOwned()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var transaction = new UnitCountAdjustment()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Stock Split",
                OriginalUnits = 2,
                NewUnits = 3
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var parcel1 = mockRepository.Create<IParcel>();
            parcel1.Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(100, 1000.00m, 1000.00m));
            parcel1.Setup(x => x.Change(new Date(2020, 02, 01), 150, 0.00m, 0.00m, transaction)).Verifiable();
            var parcel2 = mockRepository.Create<IParcel>();
            parcel2.Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(50, 500.00m, 500.00m));
            parcel2.Setup(x => x.Change(new Date(2020, 02, 01), 75, 0.00m, 0.00m, transaction)).Verifiable();
            var parcel3 = mockRepository.Create<IParcel>();
            parcel3.Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(200, 1000.00m, 1000.00m));
            parcel3.Setup(x => x.Change(new Date(2020, 02, 01), 300, 0.00m, 0.00m, transaction)).Verifiable();

            var holding = mockRepository.Create<IHolding>();
            holding.Setup(x => x.IsEffectiveAt(new Date(2020, 02, 01))).Returns(true);
            holding.Setup(x => x[new Date(2020, 02, 01)]).Returns(new IParcel[] { parcel1.Object, parcel2.Object, parcel3.Object });

            var holdings = mockRepository.Create<IHoldingCollection>();
            holdings.Setup(x => x[stock.Id]).Returns(holding.Object);

            var handler = new UnitCountAdjustmentHandler(holdings.Object);
            handler.ApplyTransaction(transaction);

            mockRepository.Verify();
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

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var holdings = mockRepository.Create<IHoldingCollection>();

            var handler = new UnitCountAdjustmentHandler(holdings.Object);

            Assert.That(() => handler.ApplyTransaction(transaction), Throws.ArgumentException);

            mockRepository.Verify();
        }
    }
}
