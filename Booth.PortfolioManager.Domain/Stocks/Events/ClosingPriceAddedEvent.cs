using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;
using Booth.EventStore;

namespace Booth.PortfolioManager.Domain.Stocks.Events
{
    public class ClosingPricesAddedEvent : Event
    {
        public ClosingPrice[] ClosingPrices { get; set; }

        public class ClosingPrice
        {
            public Date Date { get; set; }
            public decimal Price { get; set; }

            public ClosingPrice(Date date, decimal price)
            {
                Date = date;
                Price = price;
            }
        }

        public ClosingPricesAddedEvent(Guid entityId, int version, IEnumerable<ClosingPrice> closingPrices)
            : base(entityId, version)
        {
            ClosingPrices = closingPrices.ToArray();
        }
    }
}
