using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

namespace Booth.PortfolioManager.Domain.Stocks
{
    public struct StockPrice
    {
        public Date Date { get; }
        public decimal Price { get; }

        public StockPrice(Date date, decimal price)
        {
            Date = date;
            Price = price;
        }
    }
}
