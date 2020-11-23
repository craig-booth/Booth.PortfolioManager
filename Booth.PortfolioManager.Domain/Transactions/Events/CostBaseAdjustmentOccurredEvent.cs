using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.EventStore;

namespace Booth.PortfolioManager.Domain.Transactions.Events
{
    public class CostBaseAdjustmentOccurredEvent : TransactionOccurredEvent
    {
        public decimal Percentage { get; set; }

        public CostBaseAdjustmentOccurredEvent(Guid entityId, int version, Guid transactionId, Date date, Guid stock, string comment)
            : base(entityId, version, transactionId, date, stock, comment)
        {
            TransactionId = transactionId;
            Date = date;
            Stock = stock;
            Comment = comment;
        }
    }
}
