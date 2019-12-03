using System;
using System.Collections.Generic;
using System.Linq;

namespace Booth.PortfolioManager.Domain.Stocks
{
    public class StockEntityFactory : IEntityFactory<Stock>
    {

        public Stock Create(Guid id, string storedEntityType)
        {
            if (storedEntityType == "Stock")
                return new Stock(id);
            else if (storedEntityType == "StapledSecurity")
                return new StapledSecurity(id);
            else
                throw new NotSupportedException();
        }
    }
}
