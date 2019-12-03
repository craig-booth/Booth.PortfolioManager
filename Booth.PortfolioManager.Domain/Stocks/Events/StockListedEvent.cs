using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.EventStore;

namespace Booth.PortfolioManager.Domain.Stocks.Events
{
    public class StockListedEvent : Event
    {
        public string ASXCode { get; set; }
        public string Name { get; set; }
        public Date ListingDate { get; set; }
        public AssetCategory Category { get; set; }
        public bool Trust { get; set; }

        public StockListedEvent(Guid entityId, int version, string asxCode, string name, Date listingDate, AssetCategory category, bool trust)
            : base(entityId, version)
        {
            ASXCode = asxCode;
            Name = name;
            ListingDate = listingDate;
            Trust = trust;
            Category = category;
        }
    }
}
