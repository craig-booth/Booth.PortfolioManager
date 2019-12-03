using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

namespace Booth.PortfolioManager.Domain.Transactions.Events
{
    public class OpeningBalanceOccurredEvent : TransactionOccurredEvent
    {
        public int Units { get; set; }
        public decimal CostBase { get; set; }
        public Date AquisitionDate { get; set; }

        public OpeningBalanceOccurredEvent(Guid entityId, int version, Guid transactionId, Date date, Guid stock, string comment)
            : base(entityId, version, transactionId, date, stock, comment)
        {

        }

    }
}
