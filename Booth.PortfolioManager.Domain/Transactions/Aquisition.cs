using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Transactions
{
    class Aquisition : IPortfolioTransaction
    {
        public Guid Id { get; set; }
        public Date Date { get; set; }
        public IReadOnlyStock Stock { get; set; }
        public string Comment { get; set; }
        public int Units { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal TransactionCosts { get; set; }
        public bool CreateCashTransaction { get; set; }

        public string Description
        {
            get { return "Aquired " + Units.ToString("n0") + " shares @ " + MathUtils.FormatCurrency(AveragePrice, false, true); }
        }
    }
}
