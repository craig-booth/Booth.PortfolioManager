using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Utils
{
    public enum CGTCalculationMethod { MinimizeGain, MaximizeGain, FirstInFirstOut, LastInFirstOut }

    class CgtCalculator
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

        public static decimal DiscountedCgt(decimal cgtAmount, CGTMethod cgtMethod)
        {
            if ((cgtMethod == CGTMethod.Discount) && (cgtAmount > 0.00m))
                return (cgtAmount / 2).ToCurrency(RoundingRule.Round);
            
            return cgtAmount;
        }

        public static IComparer<IReadOnlyParcel> GetCgtComparer(Date disposalDate, CGTCalculationMethod method)
        {
            switch (method)
            {
                case CGTCalculationMethod.FirstInFirstOut:
                    return new FirstInFirstOutCgtComparer();
                case CGTCalculationMethod.LastInFirstOut:
                    return new LastInFirstOutCgtComparer();
                case CGTCalculationMethod.MaximizeGain:
                    return new MaximizeGainCgtComparer(disposalDate);
                case CGTCalculationMethod.MinimizeGain:
                    return new MinimizeGainCgtComparer(disposalDate);
                default:
                    throw new ArgumentException();
            }
        }

        public IEnumerable<ParcelSold> Calculate(IEnumerable<IReadOnlyParcel> parcelsOwned, Date disposalDate, int unitsSold, decimal amountReceived, IComparer<IReadOnlyParcel> cgtComparer)
        {
            var parcelsInSellOrder = parcelsOwned.Where(x => x.EffectivePeriod.ToDate == Date.MaxValue).OrderBy(x => x, cgtComparer);

            // Create list of parcels sold
            foreach (var parcel in parcelsInSellOrder)
            {
                var parcelProperties = parcel.Properties[disposalDate];

                var units = Math.Min(parcelProperties.Units, unitsSold);
                var amount = ((units / (decimal)unitsSold) * amountReceived).ToCurrency(RoundingRule.Round);

                yield return CalculateParcelCgt(parcel, disposalDate, units, amount);

                unitsSold -= units;
                amountReceived -= amount;

                if (unitsSold == 0)
                    yield break;
            }

            throw new ArgumentException("Not enough units");
        }

        public ParcelSold CalculateParcelCgt(IReadOnlyParcel parcel, Date disposalDate, int unitsSold, decimal amountReceived)
        {
            decimal costBase;

            var properties = parcel.Properties[disposalDate];
            if (unitsSold == properties.Units)
                costBase = properties.CostBase;
            else
                costBase = (properties.CostBase * ((decimal)unitsSold / properties.Units)).ToCurrency(RoundingRule.Round);

            var capitalGain = amountReceived - costBase;
            var cgtMethod = CgtMethodForParcel(parcel.AquisitionDate, disposalDate);
            var discountedGain = DiscountedCgt(capitalGain, cgtMethod);       

            return new ParcelSold(parcel, unitsSold, costBase, amountReceived, capitalGain, cgtMethod, discountedGain);
        }
    }
}
