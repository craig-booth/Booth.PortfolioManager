using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Utils
{
    public class ParcelSold
    {
        public Parcel Parcel { get; private set; }

        public Date DisposalDate { get; private set; }
        public int UnitsSold { get; private set; }   
        public decimal CostBase { get; private set; }
        public decimal AmountReceived { get; private set; }
        public decimal CapitalGain { get; private set; }
        public decimal DiscountedGain { get; private set; }

        public CGTMethod CgtMethod { get; private set; }

        public ParcelSold(Parcel parcel, int unitsSold, Date disposalDate)
        {
            Parcel = parcel;
            UnitsSold = unitsSold;
            DisposalDate = disposalDate;
        }

        public void CalculateCapitalGain(decimal amountReceived)
        {
            AmountReceived = amountReceived;
            
            var properties = Parcel.Properties[DisposalDate];
            if (UnitsSold == properties.Units)
                CostBase = properties.CostBase;                
            else
                CostBase = (properties.CostBase * ((decimal)UnitsSold / properties.Units)).ToCurrency(RoundingRule.Round);

            CapitalGain = amountReceived - CostBase;
            DiscountedGain = CgtCalculator.DiscountedCgt(CapitalGain, Parcel.AquisitionDate, DisposalDate);
            CgtMethod = CgtCalculator.CgtMethodForParcel(Parcel.AquisitionDate, DisposalDate);
        }
    }

    public enum CGTCalculationMethod { MinimizeGain, MaximizeGain, FirstInFirstOut, LastInFirstOut }

    public class CgtCalculation
    {
        private List<ParcelSold> _ParcelsSold;

        public Date DisposalDate { get; private set; }
        public int UnitsSold { get; private set; }
        public decimal AmountReceived { get; private set; }
        public decimal CapitalGain { get; private set; }
        public CGTCalculationMethod MethodUsed { get; private set; }
        public IReadOnlyCollection<ParcelSold> ParcelsSold
        {
            get
            {
                return _ParcelsSold;
            }
        }

        public CgtCalculation(Date disposalDate, decimal amountReceived, IEnumerable<ParcelSold> parcelsSold, CGTCalculationMethod method)
        {
            DisposalDate = disposalDate;
            AmountReceived = amountReceived;
            MethodUsed = method;
            _ParcelsSold = new List<ParcelSold>(parcelsSold);

            // Apportion amount received over each parcel 
            ApportionedCurrencyValue[] apportionedAmountReceived = new ApportionedCurrencyValue[_ParcelsSold.Count];
            int i = 0;
            foreach (ParcelSold parcelSold in _ParcelsSold)
                apportionedAmountReceived[i++].Units = parcelSold.UnitsSold;
            MathUtils.ApportionAmount(amountReceived, apportionedAmountReceived);


            // Calculate units sold and capital gain
            i = 0;
            foreach (ParcelSold parcelSold in _ParcelsSold)
            {
                parcelSold.CalculateCapitalGain(apportionedAmountReceived[i++].Amount);

                UnitsSold += parcelSold.UnitsSold;
                CapitalGain += parcelSold.CapitalGain;
            }
            
        }

    }

    public static class CgtCalculator
    {
 
        public static Date IndexationEndDate = new Date(1999, 09, 21);

        public static CGTMethod CgtMethodForParcel(Date aquisitionDate, Date eventDate)
        {
            if (aquisitionDate < IndexationEndDate)
                return CGTMethod.Indexation;
            else if ((eventDate - aquisitionDate).Days > 365)
                return CGTMethod.Discount;
            else
                return CGTMethod.Other;
        }

        public static decimal DiscountedCgt(decimal cgtAmount, Date aquisitionDate, Date eventDate)
        {
            var cgtMethod = CgtMethodForParcel(aquisitionDate, eventDate);

            if (cgtMethod == CGTMethod.Indexation)
                return cgtAmount;
            else if (cgtMethod == CGTMethod.Discount)
                return (cgtAmount / 2).ToCurrency(RoundingRule.Round);
            else
                return cgtAmount;

        } 

        public static IEnumerable<Parcel> ParcelsInSellOrder(IEnumerable<Parcel> parcelsOwned, IComparer<Parcel> comparer)
        {
            var sortedParcels = parcelsOwned.Where(x => x.EffectivePeriod.ToDate == Date.MaxValue).OrderBy(x => x, comparer);

            return sortedParcels;
        }


        public static CgtCalculation CalculateCapitalGain(IEnumerable<Parcel> parcelsOwned, Date saleDate, int unitsToSell, decimal amountReceived, CGTCalculationMethod method)
        {
            // Create list of parcels sold
            var parcelsSold = new List<ParcelSold>();
            foreach (var parcel in ParcelsInSellOrder(parcelsOwned, new CgtComparer(saleDate, method)))
            {
                var parcelProperties = parcel.Properties[saleDate];
                var units = Math.Min(parcelProperties.Units, unitsToSell);

                parcelsSold.Add(new ParcelSold(parcel, units, saleDate));
                unitsToSell -= units;
                if (unitsToSell == 0)
                    break;
            }

            return new CgtCalculation(saleDate, amountReceived, parcelsSold, method);
        }

    }

 
}
