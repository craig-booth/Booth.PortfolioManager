using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Transactions;

namespace Booth.PortfolioManager.Domain.Portfolios
{

    public interface IReadOnlyHolding : IEffectiveEntity
    {
        Stock Stock { get; }
        IEffectiveProperties<HoldingProperties> Properties { get; }
        HoldingSettings Settings { get; }
        IReadOnlyCashAccount DrpAccount { get; }
        IEnumerable<IReadOnlyParcel> Parcels(Date date);

        decimal Value(Date date);
    }

    public interface IHolding : IReadOnlyHolding
    {
        IEnumerable<IParcel> this[Date date] { get; }

        IParcel AddParcel(Date date, Date aquisitionDate, int units, decimal amount, decimal costBase, Transaction transaction);
        void DisposeOfParcel(IParcel parcel, Date date, int units, decimal amount, Transaction transaction);

        void AddDrpAccountAmount(Date date, decimal amount);

        void ChangeDrpParticipation(bool participateInDrp);
    }

    public class Holding : EffectiveEntity, IHolding, IReadOnlyHolding
    {
        public Stock Stock { get; set; }

        private EffectiveProperties<HoldingProperties> _Properties = new EffectiveProperties<HoldingProperties>();
        public IEffectiveProperties<HoldingProperties> Properties => _Properties;

        public HoldingSettings Settings { get; } = new HoldingSettings(false);

        private Dictionary<Guid, Parcel> _Parcels = new Dictionary<Guid, Parcel>();

        private CashAccount _DrpAccount = new CashAccount();
        public IReadOnlyCashAccount DrpAccount => _DrpAccount;

        public Holding(Stock stock, Date fromDate)
            : base(stock.Id)
        {
            Stock = stock;
            Start(fromDate);

            _Properties.Change(fromDate, new HoldingProperties(0, 0.00m, 0.00m));
        }

        public IEnumerable<IParcel> this[Date date] 
        { 
            get
            {
                return _Parcels.Values.Where(x => x.IsEffectiveAt(date));
            }
        }


        public IEnumerable<IReadOnlyParcel> Parcels(Date date)
        {
            return this[date];
        }

        public IParcel AddParcel(Date date, Date aquisitionDate, int units, decimal amount, decimal costBase, Transaction transaction)
        {
            var parcel = new Parcel(Guid.NewGuid(), date, aquisitionDate, new ParcelProperties(units, amount, costBase), transaction);

            _Parcels.Add(parcel.Id, parcel);

            var exisingProperties = Properties[date];
            var newProperties = new HoldingProperties(exisingProperties.Units + units, exisingProperties.Amount + amount, exisingProperties.CostBase + costBase);
            _Properties.Change(date, newProperties);

            return parcel;
        }

        public void DisposeOfParcel(IParcel parcel, Date date, int units, decimal amount, Transaction transaction)
        {        
            var parcelProperties = parcel.Properties[date];

            if (units > parcelProperties.Units)
                throw new Exception("Not enough shares in parcel");

            var costBase = 0.00m;
            if (units == parcelProperties.Units)
                costBase = parcelProperties.CostBase;
            else
                costBase = (parcelProperties.CostBase * ((decimal)units / parcelProperties.Units)).ToCurrency(RoundingRule.Round);

            parcel.Change(date, -units, -amount, -costBase, transaction);

            var existingProperties = Properties[date];
            if (units == existingProperties.Units)
            {
                End(date);
            }
            else
            {                
                var newProperties = new HoldingProperties(existingProperties.Units - units, existingProperties.Amount - amount, existingProperties.CostBase - costBase);
                _Properties.Change(date, newProperties);
            }
        }

        public decimal Value(Date date)
        {
            if (EffectivePeriod.Contains(date))
                return Properties[date].Units * Stock.GetPrice(date);
            else
                return 0.00m;
        }

        public void AddDrpAccountAmount(Date date, decimal amount)
        {
            if (amount > 0.00m)
                _DrpAccount.Deposit(date, amount, "");
            else if (amount < 0.00m)
                _DrpAccount.Withdraw(date, -amount, "");
        }

        public void ChangeDrpParticipation(bool participateInDrp)
        {
            Settings.ParticipateInDrp = participateInDrp;
        }
    }

    public struct HoldingProperties
    {
        public readonly int Units;
        public readonly decimal Amount;
        public readonly decimal CostBase;

        public HoldingProperties(int units, decimal amount, decimal costBase)
        {
            Units = units;
            Amount = amount;
            CostBase = costBase;
        }
    }

    public class HoldingSettings
    {
        public bool ParticipateInDrp { get; internal set; }

        public HoldingSettings(bool participateInDrp)
        {
            ParticipateInDrp = participateInDrp;
        }
    }
}
