using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;

namespace Booth.PortfolioManager.Domain.Test.Portfolios
{
    public class ParcelTests
    {

        [Fact]
        public void Creation()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var transaction = mockRepository.Create<IPortfolioTransaction>();
            var properties = new ParcelProperties(100, 1000.00m, 2000.00m);

            var parcel = new Parcel(id, new Date(2000, 01, 01), new Date(1999, 01, 01), properties, transaction.Object);

            using (new AssertionScope())
            {
                parcel.Should().BeEquivalentTo(new
                {
                    Id = id,
                    EffectivePeriod = new DateRange(new Date(2000, 01, 01), Date.MaxValue),
                    AquisitionDate = new Date(1999, 01, 01)
                });

                parcel.Properties[new Date(2000, 01, 01)].Should().Be(properties);

                parcel.Audit.Should().ContainSingle().Which.Should().BeEquivalentTo(new
                {   
                    Date = new Date(2000, 01, 01),
                    UnitCountChange = 100,
                    CostBaseChange = 2000.00m,
                    AmountChange = 1000.00m,
                    Transaction =  transaction.Object
                });
            }

            mockRepository.Verify();
        }

        [Fact]
        public void ChangeBeforeEffectivePeriod()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var transaction = mockRepository.Create<IPortfolioTransaction>();
            var properties = new ParcelProperties(100, 1000.00m, 2000.00m);

            var parcel = new Parcel(id, new Date(2000, 01, 01), new Date(1999, 01, 01), properties, transaction.Object);

            Action a = () => parcel.Change(new Date(1999, 06, 30), 10, 10.00m, 10.00m, transaction.Object);
            
            a.Should().Throw<EffectiveDateException>();

            mockRepository.Verify();
        }

        [Fact]
        public void ChangeAfterEffectivePeriod()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var transaction = mockRepository.Create<IPortfolioTransaction>();
            var properties = new ParcelProperties(100, 1000.00m, 2000.00m);

            var parcel = new Parcel(id, new Date(2000, 01, 01), new Date(1999, 01, 01), properties, transaction.Object);
            parcel.Change(new Date(2010, 01, 01), -100, 0.00m, 0.00m, transaction.Object);

            Action a = () => parcel.Change(new Date(2011, 06, 30), 10, 10.00m, 10.00m, transaction.Object);

            a.Should().Throw<EffectiveDateException>();

            mockRepository.Verify();
        }

        [Fact]
        public void ChangeUnitsToLessThanZero()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var transaction = mockRepository.Create<IPortfolioTransaction>();
            var properties = new ParcelProperties(100, 1000.00m, 2000.00m);

            var parcel = new Parcel(id, new Date(2000, 01, 01), new Date(1999, 01, 01), properties, transaction.Object);

            Action a = () => parcel.Change(new Date(2010, 06, 30), -200, 0.00m, 0.00m, transaction.Object);

            a.Should().Throw<ArgumentException>();

            mockRepository.Verify();
        }

        [Fact]
        public void ChangeAmountToLessThanZero()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var transaction = mockRepository.Create<IPortfolioTransaction>();
            var properties = new ParcelProperties(100, 1000.00m, 2000.00m);

            var parcel = new Parcel(id, new Date(2000, 01, 01), new Date(1999, 01, 01), properties, transaction.Object);

            Action a = () => parcel.Change(new Date(2010, 06, 30), 0, -2000.00m, 0.00m, transaction.Object);

            a.Should().Throw<ArgumentException>();

            mockRepository.Verify();
        }

        [Fact]
        public void ChangeCostBaseToLessThanZero()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var transaction = mockRepository.Create<IPortfolioTransaction>();
            var properties = new ParcelProperties(100, 1000.00m, 2000.00m);

            var parcel = new Parcel(id, new Date(2000, 01, 01), new Date(1999, 01, 01), properties, transaction.Object);

            Action a = () => parcel.Change(new Date(2010, 06, 30), 0, 0.00m, -3000.00m, transaction.Object);

            a.Should().Throw<ArgumentException>();

            mockRepository.Verify();
        }



        [Fact]
        public void Change()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var transaction = mockRepository.Create<IPortfolioTransaction>();
            var properties = new ParcelProperties(100, 1000.00m, 2000.00m);

            var parcel = new Parcel(id, new Date(2000, 01, 01), new Date(1999, 01, 01), properties, transaction.Object);

            var transaction2 = mockRepository.Create<IPortfolioTransaction>();
            parcel.Change(new Date(2010, 06, 30), 100, 200.00m, 300.00m, transaction2.Object);

            using (new AssertionScope())
            {
                parcel.Properties[new Date(2010, 06, 29)].Should().Be(properties);

                parcel.Properties[new Date(2010, 06, 30)].Should().Be(new ParcelProperties(200, 1200.00m, 2300.00m));

                parcel.Audit.Should().BeEquivalentTo(new[]
                {
                    new ParcelAudit(new Date(2000, 01, 01), 100, 2000.00m, 1000.00m, transaction.Object),
                    new ParcelAudit(new Date(2010, 06, 30), 100, 300.00m, 200.00m, transaction2.Object)
                });
            }

            mockRepository.Verify();
        }

        [Fact]
        public void ChangeTwiceOnSameDay()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var transaction = mockRepository.Create<IPortfolioTransaction>();
            var properties = new ParcelProperties(100, 1000.00m, 2000.00m);

            var parcel = new Parcel(id, new Date(2000, 01, 01), new Date(1999, 01, 01), properties, transaction.Object);

            var transaction2 = mockRepository.Create<IPortfolioTransaction>();
            parcel.Change(new Date(2010, 06, 30), 100, 200.00m, 300.00m, transaction2.Object);

            var transaction3 = mockRepository.Create<IPortfolioTransaction>();
            parcel.Change(new Date(2010, 06, 30), 200, 300.00m, 400.00m, transaction3.Object);

            using (new AssertionScope())
            {
                parcel.Properties[new Date(2010, 06, 29)].Should().Be(properties);

                parcel.Properties[new Date(2010, 06, 30)].Should().Be(new ParcelProperties(400, 1500.00m, 2700.00m));

                parcel.Audit.Should().BeEquivalentTo(new[]
                {
                    new ParcelAudit(new Date(2000, 01, 01), 100, 2000.00m, 1000.00m, transaction.Object),
                    new ParcelAudit(new Date(2010, 06, 30), 100, 300.00m, 200.00m, transaction2.Object),
                    new ParcelAudit(new Date(2010, 06, 30), 200, 400.00m, 300.00m, transaction3.Object)
                });
            }

            mockRepository.Verify();
        }

        [Fact]
        public void ChangeReduceUnitCountToZero()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var transaction = mockRepository.Create<IPortfolioTransaction>();
            var properties = new ParcelProperties(100, 1000.00m, 2000.00m);

            var parcel = new Parcel(id, new Date(2000, 01, 01), new Date(1999, 01, 01), properties, transaction.Object);

            var transaction2 = mockRepository.Create<IPortfolioTransaction>();
            parcel.Change(new Date(2010, 06, 30), -100, 200.00m, 300.00m, transaction2.Object);

            using (new AssertionScope())
            {
                parcel.EffectivePeriod.Should().Be(new DateRange(new Date(2000, 01, 01), new Date(2010, 06, 30)));

                parcel.Properties[new Date(2010, 06, 29)].Should().Be(properties);

                parcel.Properties[new Date(2010, 06, 30)].Should().Be(new ParcelProperties(0, 0.00m, 0.00m));

                parcel.Audit.Should().BeEquivalentTo(new[]
                {
                    new ParcelAudit(new Date(2000, 01, 01), 100, 2000.00m, 1000.00m, transaction.Object),
                    new ParcelAudit(new Date(2010, 06, 30), -100, 300.00m, 200.00m, transaction2.Object)
                });
            }

            mockRepository.Verify();
        }
    }
}
