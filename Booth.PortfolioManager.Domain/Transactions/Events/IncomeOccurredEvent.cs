using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

namespace Booth.PortfolioManager.Domain.Transactions.Events
{
    public class IncomeOccurredEvent : TransactionOccurredEvent
    {
        public Date RecordDate { get; set; }
        public decimal FrankedAmount { get; set; }
        public decimal UnfrankedAmount { get; set; }
        public decimal FrankingCredits { get; set; }
        public decimal Interest { get; set; }
        public decimal TaxDeferred { get; set; }
        public bool CreateCashTransaction { get; set; }
        public decimal DRPCashBalance { get; set; }

        public IncomeOccurredEvent(Guid entityId, int version, Guid transactionId, Date date, Guid stock, string comment)
            : base(entityId, version, transactionId, date, stock, comment)
        {

        }

    }
}
