using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.EventStore;

namespace Booth.PortfolioManager.Domain.Stocks.Events
{
    public class StapledSecurityListedEvent : Event
    {
        public string AsxCode { get; set; }
        public string Name { get; set; }
        public Date ListingDate { get; set; }
        public AssetCategory Category { get; set; }
        public StapledSecurityChild[] ChildSecurities { get; set; }

        public class StapledSecurityChild
        {
            public string AsxCode { get; set; }
            public string Name { get; set; }
            public bool Trust { get; set; }

            public StapledSecurityChild(string asxCode, string name, bool trust)
            {
                AsxCode = asxCode;
                Name = name;
                Trust = trust;
            }
        }

        public StapledSecurityListedEvent(Guid entityId, int version, string asxCode, string name, Date listingDate, AssetCategory category, StapledSecurityChild[] childSecurities)
            : base(entityId, version)
        {
            AsxCode = asxCode;
            Name = name;
            ListingDate = listingDate;
            Category = category;
            ChildSecurities = childSecurities;
        }
    }
}
