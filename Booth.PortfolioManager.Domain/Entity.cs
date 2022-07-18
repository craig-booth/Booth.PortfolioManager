using System;
using System.Collections.Generic;
using System.Text;

namespace Booth.PortfolioManager.Domain
{
    public interface IEntity : Booth.EventStore.IEntity
    {
    }

    public interface ITrackedEntity : Booth.EventStore.ITrackedEntity, IEntity
    {
    }

    public class TrackedEntity : Booth.EventStore.TrackedEntity, ITrackedEntity
    {
        public TrackedEntity(Guid id) : base(id)
        {
        }
    }
}
