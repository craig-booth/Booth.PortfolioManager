using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

namespace Booth.PortfolioManager.Web.Models.Transaction
{

    public enum TransactionType { Aquisition, CashTransaction, CostBaseAdjustment, Disposal, IncomeReceived, OpeningBalance, ReturnOfCapital, UnitCountAdjustment }
    
    public abstract class Transaction
    {
        public Guid Id { get; set; }
        public abstract TransactionType Type { get; }
        public Guid Stock { get; set; }
        public Date TransactionDate { get; set; }
        public string Comment { get; set; }
        public string Description { get; set; }
    }
}
