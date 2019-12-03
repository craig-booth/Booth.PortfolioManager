using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.Transactions
{
    public class Disposal : Transaction
    {
        public int Units { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal TransactionCosts { get; set; }
        public CGTCalculationMethod CGTMethod { get; set; }
        public bool CreateCashTransaction { get; set; }

        public override string Description
        {
            get { return "Disposed of " + Units.ToString("n0") + " shares @ " + MathUtils.FormatCurrency(AveragePrice, false, true); }
        }
    }
}
