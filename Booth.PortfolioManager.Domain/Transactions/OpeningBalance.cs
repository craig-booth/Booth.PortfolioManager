using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Transactions
{
    public class OpeningBalance : IPortfolioTransaction
    {
        public Guid Id { get; set; }
        public Date Date { get; set; }
        public Stock Stock { get; set; }
        public string Comment { get; set; }
        public int Units { get; set; }
        public decimal CostBase { get; set; }
        public Date AquisitionDate { get; set; }

        public string Description
        {
            get { return "Opening balance of " + Units.ToString("n0") + " shares"; }
        } 
    }
}
