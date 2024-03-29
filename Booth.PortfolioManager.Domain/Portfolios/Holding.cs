﻿using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Transactions;

namespace Booth.PortfolioManager.Domain.Portfolios
{

    public interface IReadOnlyHolding : IEffectiveEntity
    {
        IReadOnlyStock Stock { get; }
        IEffectiveProperties<HoldingProperties> Properties { get; }
        HoldingSettings Settings { get; }
        IReadOnlyCashAccount DrpAccount { get; }
        IEnumerable<IParcel> Parcels();
        IEnumerable<IParcel> Parcels(Date date);
        IEnumerable<IParcel> Parcels(DateRange dateRange);
    }

    public interface IHolding : IEffectiveEntity, IReadOnlyHolding
    {
        IParcel AddParcel(Date date, Date aquisitionDate, int units, decimal amount, decimal costBase, IPortfolioTransaction transaction);
        void DisposeOfParcel(Guid parcelId, Date date, int units, decimal amount, decimal capitalGain, CgtMethod cgtMethod, IPortfolioTransaction transaction);
        void ChangeParcelUnitCount(Guid parcelId, Date date, int newCount, IPortfolioTransaction transaction);
        void ReduceParcelCostBase(Guid parcelId, Date date, decimal amount, IPortfolioTransaction transaction);


        void AddDrpAccountAmount(Date date, decimal amount);
        void ChangeDrpParticipation(bool participateInDrp);
    }

    public class CgtEventArgs : EventArgs
    {   
        public Date EventDate { get; set; }
        public IReadOnlyStock Stock { get; set; }
        public int Units { get; set; }
        public decimal CostBase { get; set; }
        public decimal AmountReceived { get; set; }
        public decimal CapitalGain { get; set; }
        public CgtMethod CgtMethod { get; set; }
        public IPortfolioTransaction Transaction { get; set; }
    }

    class Holding : EffectiveEntity, IHolding, IReadOnlyHolding
    {
        public IReadOnlyStock Stock { get; set; }

        private EffectiveProperties<HoldingProperties> _Properties = new EffectiveProperties<HoldingProperties>();
        public IEffectiveProperties<HoldingProperties> Properties => _Properties;

        public HoldingSettings Settings { get; } = new HoldingSettings(false);

        private Dictionary<Guid, Parcel> _Parcels = new Dictionary<Guid, Parcel>();

        private CashAccount _DrpAccount = new CashAccount();
        public IReadOnlyCashAccount DrpAccount => _DrpAccount;

        public event EventHandler<CgtEventArgs> CgtEventOccurred;

        public Holding(IReadOnlyStock stock, Date fromDate)
            : base(stock.Id)
        {
            Stock = stock;
            Start(fromDate);

            _Properties.Change(fromDate, new HoldingProperties(0, 0.00m, 0.00m));
        }

        public HoldingProperties this[Date date]
        {
            get { return _Properties[date]; }
        }

        public IEnumerable<IParcel> Parcels()
        {
            return _Parcels.Values;
        }


        public IEnumerable<IParcel> Parcels(Date date)
        {
            return _Parcels.Values.Where(x => x.IsEffectiveAt(date));
        }

        public IEnumerable<IParcel> Parcels(DateRange dateRange)
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

        public void DisposeOfParcel(Guid parcelId, Date date, int units, decimal amount, decimal capitalGain, CgtMethod cgtMethod, IPortfolioTransaction transaction)
        {
            if (!_Parcels.TryGetValue(parcelId, out var parcel))
                throw new ArgumentException("Parcel is not part of this holding");

            var parcelProperties = parcel.Properties[date];
            if (units > parcelProperties.Units)
                throw new NotEnoughSharesForDisposal("Not enough shares in parcel");

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

            OnCgtEventOccured(date, Stock, units, costBaseChange, amount, capitalGain, cgtMethod, transaction);
        }

        public void ChangeParcelUnitCount(Guid parcelId, Date date, int newCount, IPortfolioTransaction transaction)
        {
            if (!_Parcels.TryGetValue(parcelId, out var parcel))
                throw new ArgumentException("Parcel is not part of this holding");

            var parcelProperties = parcel.Properties[date];

            // Adjust Parcel
            var unitCountChange = newCount - parcelProperties.Units;
            parcel.Change(date, unitCountChange, 0.00m, 0.00m, transaction);

            // Adjust holding          
            var holdingProperties = Properties[date];
            var newHoldingCount = holdingProperties.Units + unitCountChange;
            HoldingProperties newProperties;
            if (newHoldingCount == 0)
            {
                End(date);
                newProperties = new HoldingProperties(0, 0.00m, 0.00m);
            }
            else
            {
                newProperties = new HoldingProperties(newHoldingCount, holdingProperties.Amount, holdingProperties.CostBase);
            }
            _Properties.Change(date, newProperties);
        }

        public void ReduceParcelCostBase(Guid parcelId, Date date, decimal amount, IPortfolioTransaction transaction)
        {
            if (!_Parcels.TryGetValue(parcelId, out var parcel))
                throw new ArgumentException("Parcel is not part of this holding");

            var parcelProperties = parcel.Properties[date];

            // Adjust Parcel
            decimal costBaseChange;
            decimal capitalGain;
            if (parcelProperties.CostBase >= amount)
            {
                costBaseChange = -amount;
                capitalGain = 0.00m;
            }
            else
            {
                costBaseChange = -parcelProperties.CostBase;
                capitalGain = amount - parcelProperties.CostBase;
            }
            parcel.Change(date, 0, 0.00m, costBaseChange, transaction);

            // Adjust holding          
            var holdingProperties = Properties[date];
            var newProperties = new HoldingProperties(holdingProperties.Units, holdingProperties.Amount, holdingProperties.CostBase + costBaseChange);
            _Properties.Change(date, newProperties);

            if (capitalGain > 0)
                OnCgtEventOccured(date, Stock, holdingProperties.Units, capitalGain, amount, capitalGain, CgtMethod.Discount, transaction);
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

        private void OnCgtEventOccured(Date eventDate, IReadOnlyStock stock, int units, decimal costBase, decimal amountReceived, decimal capitalGain, CgtMethod cgtMethod, IPortfolioTransaction transaction)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            var handler = CgtEventOccurred;

            if (handler != null)
            {
                var e = new CgtEventArgs()
                {
                    EventDate = eventDate,
                    Stock = stock,
                    Units = units,
                    CostBase = costBase,
                    AmountReceived = amountReceived,
                    CapitalGain = capitalGain,
                    CgtMethod = cgtMethod,
                    Transaction = transaction
                };

                handler(this, e);
            }
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
