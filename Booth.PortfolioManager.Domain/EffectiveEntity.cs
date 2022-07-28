using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Booth.Common;

[assembly: InternalsVisibleToAttribute("Booth.PortfolioManager.Domain.Test")]
[assembly: InternalsVisibleToAttribute("Booth.PortfolioManager.Web.Test")]

namespace Booth.PortfolioManager.Domain
{
    public interface IEffectiveEntity : IEntity
    {
        DateRange EffectivePeriod { get; }
        bool IsEffectiveAt(Date date);
        bool IsEffectiveDuring(DateRange dateRange);
    }

    public abstract class EffectiveEntity : IEffectiveEntity
    {
        public Guid Id { get; }

        private DateRange _EffectivePeriod;
        public DateRange EffectivePeriod => _EffectivePeriod;

        public EffectiveEntity(Guid id)
        {
            Id = id;
        }

        protected virtual void Start(Date date)
        {
            if (!EffectivePeriod.FromDate.Equals(Date.MinValue))
                throw new EffectiveDateException("Entity already started");

            _EffectivePeriod = new DateRange(date, Date.MaxValue);
        }

        protected virtual void End(Date date)
        {
            if (!_EffectivePeriod.ToDate.Equals(Date.MaxValue))
                throw new EffectiveDateException("Entity is not current");

            if (!_EffectivePeriod.Contains(date))
                throw new EffectiveDateException("Entity not active on that date");

            _EffectivePeriod = new DateRange(_EffectivePeriod.FromDate, date);
        }

        public bool IsEffectiveAt(Date date)
        {
            return _EffectivePeriod.Contains(date);
        }

        public bool IsEffectiveDuring(DateRange dateRange)
        {
            return _EffectivePeriod.Overlaps(dateRange);
        }
    }
}
