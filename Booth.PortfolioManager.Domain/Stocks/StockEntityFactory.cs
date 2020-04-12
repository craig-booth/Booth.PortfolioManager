using System;
using System.Collections.Generic;
using System.Linq;

using Booth.EventStore;

namespace Booth.PortfolioManager.Domain.Stocks
{
    public class StockEntityFactory : ITrackedEntityFactory<Stock>
    {

        public Stock Create(Guid id, string storedEntityType)
        {
            if (storedEntityType == "Stock")
                return new Stock(id);
            else if (storedEntityType == "StapledSecurity")
                return new StapledSecurity(id);
            else
                throw new ArgumentException("Unknown entity type");
        }
    }
}
