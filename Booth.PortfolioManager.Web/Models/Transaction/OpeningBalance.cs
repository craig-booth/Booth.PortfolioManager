using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

namespace Booth.PortfolioManager.Web.Models.Transaction
{
    public class OpeningBalance : Transaction
    {
        public override TransactionType Type => TransactionType.OpeningBalance;
        public int Units { get; set; }
        public decimal CostBase { get; set; }
        public Date AquisitionDate { get; set; }
    } 
}
