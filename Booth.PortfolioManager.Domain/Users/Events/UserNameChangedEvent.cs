using System;
using System.Collections.Generic;
using System.Text;

using Booth.EventStore;

namespace Booth.PortfolioManager.Domain.Users.Events
{
    public class UserNameChangedEvent : Event
    {
        public string UserName { get; set; }

        public UserNameChangedEvent(Guid id, int version, string userName)
            : base(id, version)
        {
            UserName = userName;
        }
    }

}
