using System;
using System.Linq;

using Xunit;
using FluentAssertions;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;

namespace Booth.PortfolioManager.Domain.Test.Transactions
{
    public class UnitCountAdjustmentTests
    {
        [Fact]
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

            var holding = mockRepository.Create<IHolding>();
            var cashAccount = mockRepository.Create<ICashAccount>();

            var handler = new UnitCountAdjustmentHandler();

            Action a = () => handler.Apply(transaction, holding.Object, cashAccount.Object);
            
            a.Should().Throw<ArgumentException>();

            mockRepository.Verify();
        }

        [Fact]
        public void NoSharesOwned()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

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
            holding.Setup(x => x.IsEffectiveAt(new Date(2020, 02, 01))).Returns(false);

            var cashAccount = mockRepository.Create<ICashAccount>();

            var handler = new UnitCountAdjustmentHandler();

            Action a = () => handler.Apply(transaction, holding.Object, cashAccount.Object);

            a.Should().Throw<NoSharesOwnedException>();

            mockRepository.Verify();
        }

        [Fact]
        public void SingleParcelOwned()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

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

            var parcelId = Guid.NewGuid();
            var parcel = mockRepository.Create<IParcel>();
            parcel.Setup(x => x.Id).Returns(parcelId);
            parcel.Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(50, 1000.00m, 1500.00m));

            var holding = mockRepository.Create<IHolding>();
            holding.Setup(x => x.IsEffectiveAt(new Date(2020, 02, 01))).Returns(true);
            holding.Setup(x => x.Parcels(new Date(2020, 02, 01))).Returns(new IParcel[] { parcel.Object });
            holding.Setup(x => x.ChangeParcelUnitCount(parcelId, new Date(2020, 02, 01), 75, transaction)).Verifiable();

            var cashAccount = mockRepository.Create<ICashAccount>();

            var handler = new UnitCountAdjustmentHandler();
            handler.Apply(transaction, holding.Object, cashAccount.Object);

            mockRepository.Verify();
        }

        [Fact]
        public void MultipleParcelsOwned()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

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

            var parcel1Id = Guid.NewGuid();
            var parcel1 = mockRepository.Create<IParcel>();
            parcel1.Setup(x => x.Id).Returns(parcel1Id);
            parcel1.Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(100, 1000.00m, 1000.00m));

            var parcel2Id = Guid.NewGuid();
            var parcel2 = mockRepository.Create<IParcel>();
            parcel2.Setup(x => x.Id).Returns(parcel2Id);
            parcel2.Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(50, 500.00m, 500.00m));

            var parcel3Id = Guid.NewGuid();
            var parcel3 = mockRepository.Create<IParcel>();
            parcel3.Setup(x => x.Id).Returns(parcel3Id);
            parcel3.Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(200, 1000.00m, 1000.00m));

            var holding = mockRepository.Create<IHolding>();
            holding.Setup(x => x.IsEffectiveAt(new Date(2020, 02, 01))).Returns(true);
            holding.Setup(x => x.Parcels(new Date(2020, 02, 01))).Returns(new IParcel[] { parcel1.Object, parcel2.Object, parcel3.Object });
            holding.Setup(x => x.ChangeParcelUnitCount(parcel1Id, new Date(2020, 02, 01), 150, transaction)).Verifiable();
            holding.Setup(x => x.ChangeParcelUnitCount(parcel2Id, new Date(2020, 02, 01), 75, transaction)).Verifiable();
            holding.Setup(x => x.ChangeParcelUnitCount(parcel3Id, new Date(2020, 02, 01), 300, transaction)).Verifiable();

            var cashAccount = mockRepository.Create<ICashAccount>();

            var handler = new UnitCountAdjustmentHandler();
            handler.Apply(transaction, holding.Object, cashAccount.Object);

            mockRepository.Verify();
        }

       
    }
}
