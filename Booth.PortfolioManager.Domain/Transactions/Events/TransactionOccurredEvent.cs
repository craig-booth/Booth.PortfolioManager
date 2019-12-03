using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.EventStore;

namespace Booth.PortfolioManager.Domain.Transactions.Events
{
    public abstract class TransactionOccurredEvent : Event
    {
        public Guid TransactionId { get; set; }
        public Date Date { get; set; }
        public Guid Stock { get; set; }
        public string Comment { get; set; }

        public TransactionOccurredEvent(Guid entityId, int version, Guid transactionId, Date date, Guid stock, string comment)
            : base(entityId, version)
        {
            TransactionId = transactionId;
            Date = date;
            Stock = stock;
            Comment = comment;
        }

    }
}
