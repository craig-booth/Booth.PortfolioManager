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
        void Change(Date date, int unitChange, decimal amountChange, decimal costBaseChange, IPortfolioTransaction transaction);
    }

    class Parcel : EffectiveEntity, IParcel, IReadOnlyParcel
    {
        public Date AquisitionDate { get; private set; }

        private EffectiveProperties<ParcelProperties> _Properties { get; } = new EffectiveProperties<ParcelProperties>();
        public IEffectiveProperties<ParcelProperties> Properties => _Properties;

        private List<ParcelAudit> _Audit = new List<ParcelAudit>();
        public IEnumerable<ParcelAudit> Audit => _Audit;

        public Parcel(Guid id, Date fromDate, Date aquisitionDate, ParcelProperties properties, IPortfolioTransaction transaction)
            : base(id)
        {
            Start(fromDate);

            AquisitionDate = aquisitionDate;
            _Properties.Change(fromDate, properties);

            _Audit.Add(new ParcelAudit(fromDate, properties.Units, properties.CostBase, properties.Amount, transaction));
        }

        public void Change(Date date, int unitChange, decimal amountChange, decimal costBaseChange, IPortfolioTransaction transaction)
        {
            if (!EffectivePeriod.Contains(date))
                throw new EffectiveDateException("The parcel is not effective at that date");

            var parcelProperties = _Properties[date];

            var newUnits = parcelProperties.Units + unitChange;
            var newAmount = parcelProperties.Amount + amountChange;
            var newCostBase = parcelProperties.CostBase + costBaseChange;

            if (newUnits < 0)
                throw new ArgumentException("Units cannot be changed to be less than 0");
            if (newAmount < 0)
                throw new ArgumentException("Amount cannot be changed to be less than 0");
            if (newCostBase < 0)
                throw new ArgumentException("Costbase cannot be changed to be less than 0");

            ParcelProperties newParcelProperties;
            if (newUnits == 0)
            {
                End(date);
                newParcelProperties = new ParcelProperties(0, 0.00m, 0.00m);
            }
            else
            {
                newParcelProperties = new ParcelProperties(newUnits, newAmount, newCostBase);
            }

            _Properties.Change(date, newParcelProperties);

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
