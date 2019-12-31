using System;

using NUnit.Framework;

using Booth.Common;
using Booth.PortfolioManager.Domain;

namespace Booth.PortfolioManager.Domain.Test
{
    public class EffectiveEntityTests
    {
        [TestCase]
        public void StartWithValidDate()
        {
            var entity = new EffectiveEntityTestClass(Guid.Empty);

            var startDate = new Date(2019, 12, 01);
            entity.Start(startDate);

            Assert.That(entity.EffectivePeriod.FromDate, Is.EqualTo(startDate));
            Assert.That(entity.EffectivePeriod.ToDate, Is.EqualTo(Date.MaxValue));
        }

        [TestCase]
        public void StartAnAlreadyStartedEntity()
        {
            var entity = new EffectiveEntityTestClass(Guid.Empty);

            var startDate = new Date(2019, 12, 01);
            entity.Start(startDate);

            Assert.That(() => entity.Start(startDate), Throws.TypeOf(typeof(EffectiveDateException)));
        }

        [TestCase]
        public void EndWithValidDate()
        {
            var entity = new EffectiveEntityTestClass(Guid.Empty);

            var startDate = new Date(2019, 12, 01);
            var endDate = new Date(2019, 12, 31);

            entity.Start(startDate);
            entity.End(endDate);

            Assert.That(entity.EffectivePeriod.FromDate, Is.EqualTo(startDate));
            Assert.That(entity.EffectivePeriod.ToDate, Is.EqualTo(endDate));
        }

        [TestCase]
        public void EndAnAlreadyEndedEntity()
        {
            var entity = new EffectiveEntityTestClass(Guid.Empty);

            var startDate = new Date(2019, 12, 01);
            var endDate = new Date(2019, 12, 31);

            entity.Start(startDate);
            entity.End(endDate);

            Assert.That(() => entity.End(endDate), Throws.TypeOf(typeof(EffectiveDateException)));
        }

        [TestCase]
        public void EndAnEntityNotStarted()
        {
            var entity = new EffectiveEntityTestClass(Guid.Empty);

            var startDate = new Date(2019, 12, 01);
            var endDate = new Date(2019, 12, 31);

            Assert.That(() => entity.End(endDate), Throws.TypeOf(typeof(EffectiveDateException)));
        }

        [TestCase]
        public void EndAnEntityBeforeStartDate()
        {
            var entity = new EffectiveEntityTestClass(Guid.Empty);

            var startDate = new Date(2019, 12, 31);
            var endDate = new Date(2019, 12, 01);
            
            entity.Start(startDate);

            Assert.That(() => entity.End(endDate), Throws.TypeOf(typeof(EffectiveDateException)));
        }

        [TestCase]
        public void EffectiveAtReturnsTrueForDateInPeriod()
        {
            var entity = new EffectiveEntityTestClass(Guid.Empty);

            var startDate = new Date(2019, 12, 01);
            var endDate = new Date(2019, 12, 31);

            entity.Start(startDate);
            entity.End(endDate);

            Assert.That(entity.IsEffectiveAt(new Date(2019, 12,15)), Is.True);
        }

        [TestCase]
        public void EffectiveAtReturnsFalseForDateNotInPeriod()
        {
            var entity = new EffectiveEntityTestClass(Guid.Empty);

            var startDate = new Date(2019, 12, 01);
            var endDate = new Date(2019, 12, 31);

            entity.Start(startDate);
            entity.End(endDate);

            Assert.That(entity.IsEffectiveAt(new Date(2019, 11, 15)), Is.False);
        }

        [TestCase]
        public void EffectiveAtReturnsFalseForAnEntityNotStarted()
        {
            var entity = new EffectiveEntityTestClass(Guid.Empty);

            var startDate = new Date(2019, 12, 01);
            var endDate = new Date(2019, 12, 31);

            Assert.That(entity.IsEffectiveAt(new Date(2019, 11, 15)), Is.False);
        }

        [TestCase]
        public void EffectiveDuringForOverlappingRange()
        {
            var entity = new EffectiveEntityTestClass(Guid.Empty);

            var startDate = new Date(2019, 12, 01);
            var endDate = new Date(2019, 12, 31);

            entity.Start(startDate);
            entity.End(endDate);

            var dateRange = new DateRange(new Date(2019, 01, 01), new Date(2019, 12, 31));

            Assert.That(entity.IsEffectiveDuring(dateRange), Is.True);
        }

        [TestCase]
        public void EffectiveDuringForNonOverlappingRange()
        {
            var entity = new EffectiveEntityTestClass(Guid.Empty);

            var startDate = new Date(2019, 12, 01);
            var endDate = new Date(2019, 12, 31);

            var dateRange = new DateRange(new Date(2018, 01, 01), new Date(2018, 12, 31));

            Assert.That(entity.IsEffectiveDuring(dateRange), Is.False);
        }
    }

    // Concrete class for testing abstract EffectiveEntity
    class EffectiveEntityTestClass: EffectiveEntity
    {
        public EffectiveEntityTestClass(Guid id) : base(id) { }

        // Allow testing of protected method
        public new void Start(Date date)
        {
            base.Start(date);
        }

        // Allow testing of protected method
        public new void End(Date date)
        {
            base.End(date);
        }
    }
}
