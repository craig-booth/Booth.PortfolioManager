using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.EventStore;

namespace Booth.PortfolioManager.Domain.Stocks.Events
{
    public class StockPropertiesChangedEvent : Event
    {
        public Date ChangeDate { get; set; }
        public string ASXCode { get; set; }
        public string Name { get; set; }
        public AssetCategory Category { get; set; }

        public StockPropertiesChangedEvent(Guid entityId, int version, Date changeDate, string asxCode, string name, AssetCategory category)
            : base(entityId, version)
        {
            ChangeDate = changeDate;
            ASXCode = asxCode;
            Name = name;
            Category = category;
        }
    }
}
