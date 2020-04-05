using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.EventStore;

namespace Booth.PortfolioManager.Domain.Transactions.Events
{
    public class UnitCountAdjustmentOccurredEvent : TransactionOccurredEvent
    {
        public int OriginalUnitCount { get; set; }
        public int NewUnitCount { get; set; }

        public UnitCountAdjustmentOccurredEvent(Guid entityId, int version, Guid transactionId, Date date, Guid stock, string comment)
            : base(entityId, version, transactionId, date, stock, comment)
        {
            TransactionId = transactionId;
            Date = date;
            Stock = stock;
            Comment = comment;
        }
    }
}
