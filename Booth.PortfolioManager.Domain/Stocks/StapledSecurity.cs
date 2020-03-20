using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CSharp;

using Booth.Common;
using Booth.EventStore;

using Booth.PortfolioManager.Domain.Stocks.Events;

namespace Booth.PortfolioManager.Domain.Stocks
{
    public class StapledSecurity : Stock
    {
        private StapledSecurityChild[] _ChildSecurities;
        public IReadOnlyList<StapledSecurityChild> ChildSecurities
        {
            get { return _ChildSecurities; }
        }

        private EffectiveProperties<RelativeNTA> _RelativeNTAs = new EffectiveProperties<RelativeNTA>();
        public IEffectiveProperties<RelativeNTA> RelativeNTAs => _RelativeNTAs;

        public StapledSecurity(Guid id)
            : base(id)
        {

        }


        public void List(string asxCode, string name, Date date, AssetCategory category, IEnumerable<StapledSecurityChild> childSecurities)
        {
            var @event = new StapledSecurityListedEvent(Id, Version, asxCode, name, date, category, childSecurities?.ToArray());
            Apply(@event);

            PublishEvent(@event);
        }

        public void Apply(StapledSecurityListedEvent @event)
        {
            Version++;

            Start(@event.ListingDate);

            var properties = new StockProperties(@event.ASXCode, @event.Name, @event.Category);
            _Properties.Change(@event.ListingDate, properties);

            _ChildSecurities = new StapledSecurityChild[@event.ChildSecurities.Length];
            for (var i = 0; i < @event.ChildSecurities.Length; i++)
                _ChildSecurities[i] = new StapledSecurityChild(@event.ChildSecurities[i].ASXCode, @event.ChildSecurities[i].Name, @event.ChildSecurities[i].Trust);
            
            var dividendRules = new DividendRules(0.30m, RoundingRule.Round, false, DRPMethod.Round);
            _DividendRules.Change(@event.ListingDate, dividendRules);

            var percentages = new decimal[_ChildSecurities.Length];
            for (var i = 0; i < @event.ChildSecurities.Length; i++)
                percentages[i] = 1 / _ChildSecurities.Length;

            _RelativeNTAs.Change(@event.ListingDate, new RelativeNTA(percentages));
        }

        public override void Apply(StockDelistedEvent @event)
        {
            base.Apply(@event);

            _RelativeNTAs.End(@event.DelistedDate);
        }

        public void SetRelativeNTAs(Date date, IEnumerable<decimal> percentages)
        {
            var percentagesArray = percentages.ToArray();

            if (percentagesArray.Length != _ChildSecurities.Length)
                throw new Exception(String.Format("Expecting {0} values but received {1}", _ChildSecurities.Length, percentagesArray.Length));

            var total = percentagesArray.Sum();
            if (total != 1.00m)
                throw new Exception(String.Format("Total percentage must add up to 1.00 but was {0}", total));

            var @event = new RelativeNTAChangedEvent(Id, Version, date, percentagesArray);
            Apply(@event);

            PublishEvent(@event);
        }

        public void Apply(RelativeNTAChangedEvent @event)
        {
            Version++;

            _RelativeNTAs.Change(@event.Date, new RelativeNTA(@event.Percentages));
        }

        public new void ApplyEvents(IEnumerable<Event> events)
        {
            foreach (var @event in events)
            {
                dynamic dynamicEvent = @event;
                Apply(dynamicEvent);
            }
        }

    }

    public class StapledSecurityChild
    {
        public string ASXCode { get; set; }
        public string Name { get; set; }
        public bool Trust { get; set; }

        public StapledSecurityChild(string asxCode, string name, bool trust)
        {
            ASXCode = asxCode;
            Name = name;
            Trust = trust;
        }
    }

    public struct RelativeNTA
    {
        public readonly decimal[] Percentages;

        public RelativeNTA(decimal[] percentages)
        {
            Percentages = percentages;
        }
    }
}
