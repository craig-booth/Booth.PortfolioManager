using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CSharp;

using Booth.Common;


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

        [Obsolete]
        public new void List(string asxCode, string name, Date date, bool trust, AssetCategory category)
        {
            throw new NotSupportedException();
        }

        public void List(string asxCode, string name, Date date, AssetCategory category, IEnumerable<StapledSecurityChild> childSecurities)
        {
            base.List(asxCode, name, date, false, category);

            var children = childSecurities.ToList();

            _ChildSecurities = new StapledSecurityChild[children.Count];
            for (var i = 0; i < children.Count; i++)
                _ChildSecurities[i] = new StapledSecurityChild(children[i].AsxCode, children[i].Name, children[i].Trust);


            var percentages = new ApportionedCurrencyValue[children.Count];
            for (var i = 0; i < children.Count; i++)
                percentages[i].Units = 1;
            MathUtils.ApportionAmount(1.00m, percentages);
            _RelativeNTAs.Change(date, new RelativeNTA(percentages.Select(x => x.Amount).ToArray()));
        }


        public override void DeList(Date date)
        {
            _RelativeNTAs.End(date);

            base.DeList(date);
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


            _RelativeNTAs.Change(date, new RelativeNTA(percentagesArray));
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

    public struct RelativeNTA
    {
        public decimal[] Percentages { get; private set; }

        public RelativeNTA(decimal[] percentages)
        {
            Percentages = percentages;
        }
    }
}
