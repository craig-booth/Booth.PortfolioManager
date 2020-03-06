using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Transactions
{
    public class ReturnOfCapital : IPortfolioTransaction
    {
        public Guid Id { get; set; }
        public Date Date { get; set; }
        public Stock Stock { get; set; }
        public string Comment { get; set; }
        public Date RecordDate { get; set; }
        public decimal Amount { get; set; }
        public bool CreateCashTransaction { get; set; }

        public string Description
        {
            get
            {
                return "Return of capital " + MathUtils.FormatCurrency(Amount, false, true);
            }
        }
    }
}
