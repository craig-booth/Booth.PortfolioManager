using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using Booth.EventStore;

[assembly: InternalsVisibleToAttribute("Booth.PortfolioManager.Domain.Test")]

namespace Booth.PortfolioManager.Domain
{
    class EventList
    {
        private Queue<Event> _Events = new Queue<Event>();

        public void Add(Event @event)
        {
            _Events.Enqueue(@event);
        }

        public bool EventsAvailable
        {
            get { return _Events.Count > 0; }
        }
        
        public List<Event> Fetch()
        {
            var eventsToReturn = new List<Event>();

            while (_Events.Count > 0)
                eventsToReturn.Add(_Events.Dequeue());

            return eventsToReturn;
        }

    }
}
