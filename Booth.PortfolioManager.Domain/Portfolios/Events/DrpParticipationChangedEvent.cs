using System;
using System.Collections.Generic;
using System.Text;

using Booth.EventStore;

namespace Booth.PortfolioManager.Domain.Portfolios.Events
{
    public class DrpParticipationChangedEvent : Event
    {
        public Guid Holding { get; set; }
        public bool ParticipateInDrp { get; set; }

        public DrpParticipationChangedEvent(Guid entityId, int version, Guid holding, bool participateInDrp)
            : base(entityId, version)
        {
            Holding = holding;
            ParticipateInDrp = participateInDrp;
        }

    }
}
