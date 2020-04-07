using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;

namespace Booth.PortfolioManager.Domain.Test.Portfolios
{
    class ParcelTests
    {

        [TestCase]
        public void Creation()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var transaction = mockRepository.Create<IPortfolioTransaction>();
            var properties = new ParcelProperties(100, 1000.00m, 2000.00m);

            var parcel = new Parcel(id, new Date(2000, 01, 01), new Date(1999, 01, 01), properties, transaction.Object);

            Assert.Multiple(() =>
            {
                Assert.That(parcel.Id, Is.EqualTo(id));
                Assert.That(parcel.EffectivePeriod.FromDate, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(parcel.EffectivePeriod.ToDate, Is.EqualTo(Date.MaxValue));
                Assert.That(parcel.AquisitionDate, Is.EqualTo(new Date(1999, 01, 01)));
                Assert.That(parcel.Properties[new Date(2000, 01, 01)], Is.EqualTo(properties));

                var audit = parcel.Audit.ToList();
                Assert.That(audit, Has.Count.EqualTo(1));
                Assert.That(audit[0], Is.EqualTo(new ParcelAudit(new Date(2000, 01, 01), 100, 2000.00m, 1000.00m, transaction.Object)));
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void ChangeBeforeEffectivePeriod()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var transaction = mockRepository.Create<IPortfolioTransaction>();
            var properties = new ParcelProperties(100, 1000.00m, 2000.00m);

            var parcel = new Parcel(id, new Date(2000, 01, 01), new Date(1999, 01, 01), properties, transaction.Object);

            Assert.That(() => parcel.Change(new Date(1999, 06, 30), 10, 10.00m, 10.00m, transaction.Object), Throws.TypeOf(typeof(EffectiveDateException)));

            mockRepository.Verify();
        }

        [TestCase]
        public void ChangeAfterEffectivePeriod()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var transaction = mockRepository.Create<IPortfolioTransaction>();
            var properties = new ParcelProperties(100, 1000.00m, 2000.00m);

            var parcel = new Parcel(id, new Date(2000, 01, 01), new Date(1999, 01, 01), properties, transaction.Object);
            parcel.Change(new Date(2010, 01, 01), -100, 0.00m, 0.00m, transaction.Object);

            Assert.That(() => parcel.Change(new Date(2011, 06, 30), 10, 10.00m, 10.00m, transaction.Object), Throws.TypeOf(typeof(EffectiveDateException)));


            mockRepository.Verify();
        }

        [TestCase]
        public void ChangeUnitsToLessThanZero()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var transaction = mockRepository.Create<IPortfolioTransaction>();
            var properties = new ParcelProperties(100, 1000.00m, 2000.00m);

            var parcel = new Parcel(id, new Date(2000, 01, 01), new Date(1999, 01, 01), properties, transaction.Object);

            Assert.That(() => parcel.Change(new Date(2010, 06, 30), -200, 0.00m, 0.00m, transaction.Object), Throws.ArgumentException);

            mockRepository.Verify();
        }

        [TestCase]
        public void ChangeAmountToLessThanZero()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var transaction = mockRepository.Create<IPortfolioTransaction>();
            var properties = new ParcelProperties(100, 1000.00m, 2000.00m);

            var parcel = new Parcel(id, new Date(2000, 01, 01), new Date(1999, 01, 01), properties, transaction.Object);

            Assert.That(() => parcel.Change(new Date(2010, 06, 30), 0, -2000.00m, 0.00m, transaction.Object), Throws.ArgumentException);

            mockRepository.Verify();
        }

        [TestCase]
        public void ChangeCostBaseToLessThanZero()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var transaction = mockRepository.Create<IPortfolioTransaction>();
            var properties = new ParcelProperties(100, 1000.00m, 2000.00m);

            var parcel = new Parcel(id, new Date(2000, 01, 01), new Date(1999, 01, 01), properties, transaction.Object);

            Assert.That(() => parcel.Change(new Date(2010, 06, 30), 0, 0.00m, -3000.00m, transaction.Object), Throws.ArgumentException);

            mockRepository.Verify();
        }



        [TestCase]
        public void Change()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var transaction = mockRepository.Create<IPortfolioTransaction>();
            var properties = new ParcelProperties(100, 1000.00m, 2000.00m);

            var parcel = new Parcel(id, new Date(2000, 01, 01), new Date(1999, 01, 01), properties, transaction.Object);

            var transaction2 = mockRepository.Create<IPortfolioTransaction>();
            parcel.Change(new Date(2010, 06, 30), 100, 200.00m, 300.00m, transaction2.Object);

            Assert.Multiple(() =>
            {
                Assert.That(parcel.Properties[new Date(2010, 06, 29)], Is.EqualTo(properties));

                Assert.That(parcel.Properties[new Date(2010, 06, 30)], Is.EqualTo(new ParcelProperties(200, 1200.00m, 2300.00m)));

                var audit = parcel.Audit.ToList();
                Assert.That(audit, Has.Count.EqualTo(2));
                Assert.That(audit[0], Is.EqualTo(new ParcelAudit(new Date(2000, 01, 01), 100, 2000.00m, 1000.00m, transaction.Object)));
                Assert.That(audit[1], Is.EqualTo(new ParcelAudit(new Date(2010, 06, 30), 100, 300.00m, 200.00m, transaction2.Object)));
            });

            mockRepository.Verify();
        }

        [TestCase]
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

            Assert.Multiple(() =>
            {
                Assert.That(parcel.Properties[new Date(2010, 06, 29)], Is.EqualTo(properties));

                Assert.That(parcel.Properties[new Date(2010, 06, 30)], Is.EqualTo(new ParcelProperties(400, 1500.00m, 2700.00m)));

                var audit = parcel.Audit.ToList();
                Assert.That(audit, Has.Count.EqualTo(3));
                Assert.That(audit[0], Is.EqualTo(new ParcelAudit(new Date(2000, 01, 01), 100, 2000.00m, 1000.00m, transaction.Object)));
                Assert.That(audit[1], Is.EqualTo(new ParcelAudit(new Date(2010, 06, 30), 100, 300.00m, 200.00m, transaction2.Object)));
                Assert.That(audit[2], Is.EqualTo(new ParcelAudit(new Date(2010, 06, 30), 200, 400.00m, 300.00m, transaction3.Object)));
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void ChangeReduceUnitCountToZero()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var transaction = mockRepository.Create<IPortfolioTransaction>();
            var properties = new ParcelProperties(100, 1000.00m, 2000.00m);

            var parcel = new Parcel(id, new Date(2000, 01, 01), new Date(1999, 01, 01), properties, transaction.Object);

            var transaction2 = mockRepository.Create<IPortfolioTransaction>();
            parcel.Change(new Date(2010, 06, 30), -100, 200.00m, 300.00m, transaction2.Object);

            Assert.Multiple(() =>
            {
                Assert.That(parcel.EffectivePeriod.FromDate, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(parcel.EffectivePeriod.ToDate, Is.EqualTo(new Date(2010, 06, 30)));

                Assert.That(parcel.Properties[new Date(2010, 06, 29)], Is.EqualTo(properties));

                Assert.That(parcel.Properties[new Date(2010, 06, 30)], Is.EqualTo(new ParcelProperties(0, 0.00m, 0.00m)));

                var audit = parcel.Audit.ToList();
                Assert.That(audit, Has.Count.EqualTo(2));
                Assert.That(audit[0], Is.EqualTo(new ParcelAudit(new Date(2000, 01, 01), 100, 2000.00m, 1000.00m, transaction.Object)));
                Assert.That(audit[1], Is.EqualTo(new ParcelAudit(new Date(2010, 06, 30), -100, 300.00m, 200.00m, transaction2.Object)));
            });

            mockRepository.Verify();
        }
    }
}
