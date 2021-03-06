﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Booth.Common;
using Booth.EventStore;

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
        public DateRange EffectivePeriod { get; private set; }

        public EffectiveEntity(Guid id)
        {
            Id = id;
        }

        protected virtual void Start(Date date)
        {
            if (!EffectivePeriod.FromDate.Equals(Date.MinValue))
                throw new EffectiveDateException("Entity already started");

            EffectivePeriod = new DateRange(date, Date.MaxValue);
        }

        protected virtual void End(Date date)
        {
            if (!EffectivePeriod.ToDate.Equals(Date.MaxValue))
                throw new EffectiveDateException("Entity is not current");

            if (!EffectivePeriod.Contains(date))
                throw new EffectiveDateException("Entity not active on that date");

            EffectivePeriod = new DateRange(EffectivePeriod.FromDate, date);
        }

        public bool IsEffectiveAt(Date date)
        {
            return EffectivePeriod.Contains(date);
        }

        public bool IsEffectiveDuring(DateRange dateRange)
        {
            return EffectivePeriod.Overlaps(dateRange);
        }
    }
}
