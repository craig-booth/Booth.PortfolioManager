using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.Common;

namespace Booth.PortfolioManager.Web.Models.Stock
{
    public class StockPriceResponse
    {
        public Guid Id { get; set; }
        public string AsxCode { get; set; }
        public string Name { get; set; }
        public List<ClosingPrice> ClosingPrices { get; set; } = new List<ClosingPrice>();

        public void AddClosingPrice(Date date, decimal price)
        {
            var closingPrice = new ClosingPrice()
            {
                Date = date,
                Price = price
            };
            ClosingPrices.Add(closingPrice);
        }
    }
}
