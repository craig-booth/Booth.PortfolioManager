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
        IEnumerable<IReadOnlyParcel> Parcels();
        IEnumerable<IReadOnlyParcel> Parcels(Date date);
        IEnumerable<IReadOnlyParcel> Parcels(DateRange dateRange);
        decimal Value(Date date);
    }

    public interface IHolding : IReadOnlyHolding
    {
        IEnumerable<IParcel> this[Date date] { get; }

        IParcel AddParcel(Date date, Date aquisitionDate, int units, decimal amount, decimal costBase, IPortfolioTransaction transaction);
        void DisposeOfParcel(Guid parcelId, Date date, int units, decimal amount, IPortfolioTransaction transaction);

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

        public IEnumerable<IReadOnlyParcel> Parcels()
        {
            return _Parcels.Values;
        }

        public IEnumerable<IReadOnlyParcel> Parcels(Date date)
        {
            return this[date];
        }

        public IEnumerable<IReadOnlyParcel> Parcels(DateRange dateRange)
        {
            return _Parcels.Values.Where(x => x.IsEffectiveDuring(dateRange));
        }

        public IParcel AddParcel(Date date, Date aquisitionDate, int units, decimal amount, decimal costBase, IPortfolioTransaction transaction)
        {
            var parcel = new Parcel(Guid.NewGuid(), date, aquisitionDate, new ParcelProperties(units, amount, costBase), transaction);

            _Parcels.Add(parcel.Id, parcel);

            var exisingProperties = Properties[date];
            var newProperties = new HoldingProperties(exisingProperties.Units + units, exisingProperties.Amount + amount, exisingProperties.CostBase + costBase);
            _Properties.Change(date, newProperties);

            return parcel;
        }

        public void DisposeOfParcel(Guid parcelId, Date date, int units, decimal amount, IPortfolioTransaction transaction)
        {        
            if (!_Parcels.TryGetValue(parcelId, out var parcel))
                throw new ArgumentException("Parcel is not part of this holding");

            var parcelProperties = parcel.Properties[date];
            if (units > parcelProperties.Units)
                throw new NotEnoughSharesForDisposal(transaction, "Not enough shares in parcel");

            // Adjust Parcel
            decimal costBaseChange;
            decimal amountChange;
            if (units == parcelProperties.Units)
            {
                amountChange = parcelProperties.Amount;
                costBaseChange = parcelProperties.CostBase;
            }
            else
            {
                amountChange = (parcelProperties.Amount * ((decimal)units / parcelProperties.Units)).ToCurrency(RoundingRule.Round);
                costBaseChange = (parcelProperties.CostBase * ((decimal)units / parcelProperties.Units)).ToCurrency(RoundingRule.Round);
            }
            parcel.Change(date, -units, -amountChange, -costBaseChange, transaction);

            // Adjust holding          
            var holdingProperties = Properties[date];
            HoldingProperties newProperties;
            if (units == holdingProperties.Units)
            {
                End(date);
                newProperties = new HoldingProperties(0, 0.00m, 0.00m);
            }
            else
            {
                newProperties = new HoldingProperties(holdingProperties.Units - units, holdingProperties.Amount - amountChange, holdingProperties.CostBase - costBaseChange);
            }
            _Properties.Change(date, newProperties);
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
