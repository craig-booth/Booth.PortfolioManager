using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;
using Booth.EventStore;

namespace Booth.PortfolioManager.Domain.Stocks.Events
{

    public class RelativeNTAChangedEvent : Event
    {
        public Date Date { get; set; }
        public decimal[] Percentages { get; set; }

        public RelativeNTAChangedEvent(Guid entityId, int version, Date date, IEnumerable<decimal> percentages)
            : base(entityId, version)
        {
            Date = date;
            Percentages = percentages.ToArray();
        }
    }
}
