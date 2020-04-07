using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

using Booth.PortfolioManager.Domain.Transactions;

namespace Booth.PortfolioManager.Domain.Portfolios
{
    public struct ParcelAudit
    {
        public Date Date { get; }
        public int UnitCountChange { get;  }
        public decimal CostBaseChange { get; }
        public decimal AmountChange { get; }

        public IPortfolioTransaction Transaction { get; }

        public ParcelAudit(Date date, int unitCountChange, decimal costBaseChange, decimal amountChange, IPortfolioTransaction transaction)
        {
            Date = date;
            UnitCountChange = unitCountChange;
            CostBaseChange = costBaseChange;
            AmountChange = amountChange;
            Transaction = transaction;
        }

    }
}
