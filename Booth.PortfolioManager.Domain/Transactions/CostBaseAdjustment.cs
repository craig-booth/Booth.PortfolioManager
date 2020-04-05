using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Transactions
{
    class CostBaseAdjustment : IPortfolioTransaction
    {
        public Guid Id { get; set; }
        public Date Date { get; set; }
        public Stock Stock { get; set; }
        public string Comment { get; set; }
        public decimal Percentage { get; set; }

        public string Description
        {
            get
            {
                return "Adjust cost base by " + Percentage.ToString("P2");
            }
        }
    }
}
