using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.EventStore;

namespace Booth.PortfolioManager.Domain.Stocks.Events
{
    public class StockDelistedEvent : Event
    {
        public Date DelistedDate { get; set; }

        public StockDelistedEvent(Guid entityId, int version, Date delistedDate)
            : base(entityId, version)
        {
            DelistedDate = delistedDate;
        }
    }
}
