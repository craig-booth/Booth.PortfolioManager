using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Transactions
{
    public class CostBaseAdjustment : PortfolioTransaction
    {
        public decimal Percentage { get; set; }

        public override string Description
        {
            get
            {
                return "Adjust cost base by " + Percentage.ToString("P2");
            }
        }
    }
}
