using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.EventStore;

namespace Booth.PortfolioManager.Domain.CorporateActions.Events
{
    public class CompositeActionAddedEvent : CorporateActionAddedEvent
    {

        public List<CorporateActionAddedEvent> ChildActions = new List<CorporateActionAddedEvent>();

        public CompositeActionAddedEvent(Guid entityId, int version, Guid actionId, Date actionDate, string description)
            : base(entityId, version, actionId, actionDate, description)
        {
        }

    }
}
