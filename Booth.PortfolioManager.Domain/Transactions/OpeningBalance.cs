using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

namespace Booth.PortfolioManager.Domain.Transactions
{
    public class OpeningBalance : Transaction
    {
        public int Units { get; set; }
        public decimal CostBase { get; set; }
        public Date AquisitionDate { get; set; }

        public override string Description
        {
            get { return "Opening balance of " + Units.ToString("n0") + " shares"; }
        } 
    }
}
