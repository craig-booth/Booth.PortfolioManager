using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CSharp;

using Booth.Common;
using Booth.EventStore;

using Booth.PortfolioManager.Domain.Stocks.Events;

namespace Booth.PortfolioManager.Domain.Stocks
{
    class StapledSecurity : Stock
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

        [Obsolete]
        public new void List(string asxCode, string name, Date date, bool trust, AssetCategory category)
        {
            throw new NotSupportedException();
        }

        public void List(string asxCode, string name, Date date, AssetCategory category, IEnumerable<StapledSecurityChild> childSecurities)
        {
            var children = childSecurities.Select(x => new StapledSecurityListedEvent.StapledSecurityChild(x.AsxCode, x.Name, x.Trust)).ToArray();
            
            var @event = new StapledSecurityListedEvent(Id, Version, asxCode, name, date, category, children);
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
            
            var dividendRules = new DividendRules(0.30m, RoundingRule.Round, false, DrpMethod.Round);
            _DividendRules.Change(@event.ListingDate, dividendRules);

            var percentages = new ApportionedCurrencyValue[_ChildSecurities.Length];
            for (var i = 0; i < @event.ChildSecurities.Length; i++)
                percentages[i].Units = 1;
            MathUtils.ApportionAmount(1.00m, percentages);

            _RelativeNTAs.Change(@event.ListingDate, new RelativeNTA(percentages.Select(x => x.Amount).ToArray()));
        }

        public override void Apply(StockDelistedEvent @event)
        {
            base.Apply(@event);

            _RelativeNTAs.End(@event.DelistedDate);
        }

        public void SetRelativeNTAs(Date date, IEnumerable<decimal> percentages)
        {
            if (!IsEffectiveAt(date))
                throw new EffectiveDateException(String.Format("Stock not active at {0}", date));

            var percentagesArray = percentages.ToArray();

            if (percentagesArray.Length != _ChildSecurities.Length)
                throw new ArgumentException(String.Format("Expecting {0} values but received {1}", _ChildSecurities.Length, percentagesArray.Length));

            var total = percentagesArray.Sum();
            if (total != 1.00m)
                throw new ArgumentException(String.Format("Total percentage must add up to 1.00 but was {0}", total));

            var @event = new RelativeNTAChangedEvent(Id, Version, date, percentagesArray);
            Apply(@event);

            PublishEvent(@event);
        }

        public void Apply(RelativeNTAChangedEvent @event)
        {
            Version++;

            _RelativeNTAs.Change(@event.Date, new RelativeNTA(@event.Percentages));
        }

        public override void ApplyEvents(IEnumerable<Event> events)
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
        public string AsxCode { get; set; }
        public string Name { get; set; }
        public bool Trust { get; set; }

        public StapledSecurityChild(string asxCode, string name, bool trust)
        {
            AsxCode = asxCode;
            Name = name;
            Trust = trust;
        }
    }

    struct RelativeNTA
    {
        public readonly decimal[] Percentages;

        public RelativeNTA(decimal[] percentages)
        {
            Percentages = percentages;
        }
    }
}
