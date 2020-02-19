using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

using Booth.PortfolioManager.Domain.Transactions;

namespace Booth.PortfolioManager.Domain.Portfolios
{
    public interface IReadOnlyParcel : IEffectiveEntity
    {
        Date AquisitionDate { get; }
        IEffectiveProperties<ParcelProperties> Properties { get; }
        IEnumerable<ParcelAudit> Audit { get; }
    }

    public interface IParcel : IReadOnlyParcel
    {          
        void Change(Date date, int unitChange, decimal amountChange, decimal costBaseChange, Transaction transaction);
    }

    public class Parcel : EffectiveEntity, IParcel, IReadOnlyParcel
    {
        public Date AquisitionDate { get; private set; }

        private EffectiveProperties<ParcelProperties> _Properties { get; } = new EffectiveProperties<ParcelProperties>();
        public IEffectiveProperties<ParcelProperties> Properties => _Properties;

        private List<ParcelAudit> _Audit = new List<ParcelAudit>();
        public IEnumerable<ParcelAudit> Audit => _Audit;

        public Parcel(Guid id, Date fromDate, Date aquisitionDate, ParcelProperties properties, Transaction transaction)
            : base(id)
        {
            Start(fromDate);

            AquisitionDate = aquisitionDate;
            _Properties.Change(fromDate, properties);

            _Audit.Add(new ParcelAudit(aquisitionDate, properties.Units, properties.CostBase, properties.Amount, transaction));
        }

        public void Change(Date date, int unitChange, decimal amountChange, decimal costBaseChange, Transaction transaction)
        {
            var parcelProperties = _Properties[date];

            var newUnits = parcelProperties.Units + unitChange;
            var newAmount = parcelProperties.Amount + amountChange;
            var newCostBase = parcelProperties.CostBase + costBaseChange;

            if (newUnits < 0)
                throw new Exception("Not enough shares in parcel");
            else if (newUnits == 0)
            {              
                End(date);
            }
            else
            {              
                var newParcelProperties = new ParcelProperties(newUnits, newAmount, newCostBase);
                _Properties.Change(date, newParcelProperties);
            }

            _Audit.Add(new ParcelAudit(date, unitChange, costBaseChange, amountChange, transaction));
        }

    }

    public struct ParcelProperties
    {
        public readonly int Units;
        public readonly decimal Amount;
        public readonly decimal CostBase;

        public ParcelProperties(int units, decimal amount, decimal costBase)
        {
            Units = units;
            Amount = amount;
            CostBase = costBase;
        }
    }
}
