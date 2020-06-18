using System;

using Xunit;
using FluentAssertions;

using Booth.Common;

namespace Booth.PortfolioManager.Domain.Test
{
    public class EffectiveEntityTests
    {
        [Fact]
        public void StartWithValidDate()
        {
            var entity = new EffectiveEntityTestClass(Guid.Empty);

            var startDate = new Date(2019, 12, 01);
            entity.Start(startDate);

            entity.EffectivePeriod.Should().Be(new DateRange(startDate, Date.MaxValue));
        }

        [Fact]
        public void StartAnAlreadyStartedEntity()
        {
            var entity = new EffectiveEntityTestClass(Guid.Empty);

            var startDate = new Date(2019, 12, 01);
            entity.Start(startDate);

            Action a = () => entity.Start(startDate);
            
            a.Should().Throw<EffectiveDateException>();
        }

        [Fact]
        public void EndWithValidDate()
        {
            var entity = new EffectiveEntityTestClass(Guid.Empty);

            var startDate = new Date(2019, 12, 01);
            var endDate = new Date(2019, 12, 31);

            entity.Start(startDate);
            entity.End(endDate);

            entity.EffectivePeriod.Should().Be(new DateRange(startDate, endDate));
        }

        [Fact]
        public void EndAnAlreadyEndedEntity()
        {
            var entity = new EffectiveEntityTestClass(Guid.Empty);

            var startDate = new Date(2019, 12, 01);
            var endDate = new Date(2019, 12, 31);

            entity.Start(startDate);
            entity.End(endDate);

            Action a = () => entity.End(endDate);

            a.Should().Throw<EffectiveDateException>();
        }

        [Fact]
        public void EndAnEntityNotStarted()
        {
            var entity = new EffectiveEntityTestClass(Guid.Empty);

            var startDate = new Date(2019, 12, 01);
            var endDate = new Date(2019, 12, 31);

            Action a = () => entity.End(endDate);

            a.Should().Throw<EffectiveDateException>();
        }

        [Fact]
        public void EndAnEntityBeforeStartDate()
        {
            var entity = new EffectiveEntityTestClass(Guid.Empty);

            var startDate = new Date(2019, 12, 31);
            var endDate = new Date(2019, 12, 01);
            
            entity.Start(startDate);

            Action a = () => entity.End(endDate);

            a.Should().Throw<EffectiveDateException>();
        }

        [Fact]
        public void EffectiveAtReturnsTrueForDateInPeriod()
        {
            var entity = new EffectiveEntityTestClass(Guid.Empty);

            var startDate = new Date(2019, 12, 01);
            var endDate = new Date(2019, 12, 31);

            entity.Start(startDate);
            entity.End(endDate);

            entity.IsEffectiveAt(new Date(2019, 12, 15)).Should().BeTrue();
        }

        [Fact]
        public void EffectiveAtReturnsFalseForDateNotInPeriod()
        {
            var entity = new EffectiveEntityTestClass(Guid.Empty);

            var startDate = new Date(2019, 12, 01);
            var endDate = new Date(2019, 12, 31);

            entity.Start(startDate);
            entity.End(endDate);

            entity.IsEffectiveAt(new Date(2019, 11, 15)).Should().BeFalse();
        }

        [Fact]
        public void EffectiveAtReturnsFalseForAnEntityNotStarted()
        {
            var entity = new EffectiveEntityTestClass(Guid.Empty);

            var startDate = new Date(2019, 12, 01);
            var endDate = new Date(2019, 12, 31);

            entity.IsEffectiveAt(new Date(2019, 11, 15)).Should().BeFalse(); 
        }

        [Fact]
        public void EffectiveDuringForOverlappingRange()
        {
            var entity = new EffectiveEntityTestClass(Guid.Empty);

            var startDate = new Date(2019, 12, 01);
            var endDate = new Date(2019, 12, 31);

            entity.Start(startDate);
            entity.End(endDate);

            var dateRange = new DateRange(new Date(2019, 01, 01), new Date(2019, 12, 31));

            entity.IsEffectiveDuring(dateRange).Should().BeTrue();
        }

        [Fact]
        public void EffectiveDuringForNonOverlappingRange()
        {
            var entity = new EffectiveEntityTestClass(Guid.Empty);

            var startDate = new Date(2019, 12, 01);
            var endDate = new Date(2019, 12, 31);

            var dateRange = new DateRange(new Date(2018, 01, 01), new Date(2018, 12, 31));

            entity.IsEffectiveDuring(dateRange).Should().BeFalse();
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
