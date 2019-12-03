using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.EventStore;

namespace Booth.PortfolioManager.Domain.CorporateActions.Events
{
    public class CapitalReturnAddedEvent : CorporateActionAddedEvent
    {
        public Date PaymentDate { get; set; }
        public decimal Amount { get; set; }

        public CapitalReturnAddedEvent(Guid entityId, int version, Guid actionId, Date actionDate, string description, Date paymentDate, decimal amount)
            : base(entityId, version, actionId, actionDate, description)
        {
            PaymentDate = paymentDate;
            Amount = amount;
        }
    }
}
