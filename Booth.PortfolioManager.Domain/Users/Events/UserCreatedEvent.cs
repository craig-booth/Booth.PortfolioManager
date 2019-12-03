using System;
using System.Collections.Generic;
using System.Text;

using Booth.EventStore;

namespace Booth.PortfolioManager.Domain.Users.Events
{
    public class UserCreatedEvent : Event
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public UserCreatedEvent(Guid id, int version, string userName, string password)
            : base(id, version)
        {
            UserName = userName;
            Password = password;
        }
    }
}
