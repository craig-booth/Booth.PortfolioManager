using System;
using System.Collections.Generic;
using System.Text;

using Booth.EventStore;

namespace Booth.PortfolioManager.Domain.Users.Events
{
    public class PasswordChangedEvent : Event
    {
        public string Password { get; set; }

        public PasswordChangedEvent(Guid id, int version, string password)
            : base(id, version)
        {
            Password = password;
        }
    }
}
