using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.EventStore;

namespace Booth.PortfolioManager.Domain.CorporateActions.Events
{
    public class SplitConsolidationAddedEvent : CorporateActionAddedEvent
    { 
        public int OriginalUnits { get; private set; }
        public int NewUnits { get; private set; }

        public SplitConsolidationAddedEvent(Guid entityId, int version, Guid actionId, Date actionDate, string description, int originalUnits, int newUnits)
            : base(entityId, version, actionId, actionDate, description)
        {
            OriginalUnits = originalUnits;
            NewUnits = newUnits;
        }
    }
}
