using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Transactions
{
    class UnitCountAdjustment : IPortfolioTransaction
    {
        public Guid Id { get; set; }
        public Date Date { get; set; }
        public Stock Stock { get; set; }
        public string Comment { get; set; }
        public int OriginalUnits { get; set; }
        public int NewUnits { get; set; }

        public string Description
        {
            get
            {
                  return "Adjust unit count using ratio " + OriginalUnits.ToString("n0") + ":" + NewUnits.ToString("n0");
            }
        }
    }
}
