using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.EventStore;

namespace Booth.PortfolioManager.Domain.CorporateActions.Events
{
    public class CorporateActionAddedEvent : Event
    {
        public Guid ActionId { get; set; }
        public Date ActionDate { get; set; }
        public string Description { get; set; }

        public CorporateActionAddedEvent(Guid entityId, int version, Guid actionId, Date actionDate, string description)
            : base(entityId, version)
        {
            ActionId = actionId;
            ActionDate = actionDate;
            Description = description;
        }
    }
}
