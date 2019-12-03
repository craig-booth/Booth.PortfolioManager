﻿using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

namespace Booth.PortfolioManager.Domain
{
    public interface IEntity
    {
        Guid Id { get; }
    }

    interface IEffectiveEntity
    {
        Guid Id { get; }
        DateRange EffectivePeriod { get; }
    }
    
    public abstract class EffectiveEntity :
        IEntity,
        IEffectiveEntity
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
                throw new Exception("Entity already started");

            EffectivePeriod = new DateRange(date, Date.MaxValue);
        }

        protected virtual void End(Date date)
        {
            if (!EffectivePeriod.ToDate.Equals(Date.MaxValue))
                throw new Exception("Entity is not current");

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
