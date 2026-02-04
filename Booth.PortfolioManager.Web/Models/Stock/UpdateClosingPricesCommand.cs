using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

namespace Booth.PortfolioManager.Web.Models.Stock
{
    public class UpdateClosingPricesCommand
    {
        public Guid Id { get; set; }
        public List<ClosingPrice> ClosingPrices { get; } = new List<ClosingPrice>();

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
