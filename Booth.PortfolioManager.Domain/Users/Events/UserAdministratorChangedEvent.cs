using System;
using System.Collections.Generic;
using System.Text;

using Booth.EventStore;

namespace Booth.PortfolioManager.Domain.Users.Events
{
    public class UserAdministratorChangedEvent : Event
    {
        public bool Administrator { get; set; }

        public UserAdministratorChangedEvent(Guid id, int version, bool administrator)
            : base(id, version)
        {
            Administrator = administrator;
        }
    }
}
