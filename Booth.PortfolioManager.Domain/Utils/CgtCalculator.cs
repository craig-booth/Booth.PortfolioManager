using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Utils
{
    public enum CgtCalculationMethod { MinimizeGain, MaximizeGain, FirstInFirstOut, LastInFirstOut }

    public static class CgtUtils
    {
        public static Date IndexationEndDate = new Date(1999, 09, 21);

        public static CgtMethod CgtMethodForParcel(Date aquisitionDate, Date eventDate)
        {
            if (aquisitionDate < IndexationEndDate)
                return CgtMethod.Indexation;
            else if ((eventDate - aquisitionDate).Days > 365)
                return CgtMethod.Discount;
            else
                return CgtMethod.Other;
        }

        public static decimal DiscountedCgt(decimal cgtAmount, CgtMethod cgtMethod)
        {
            if ((cgtMethod == CgtMethod.Discount) && (cgtAmount > 0.00m))
                return (cgtAmount / 2).ToCurrency(RoundingRule.Round);

            return cgtAmount;
        }
    }

    class CgtCalculator
    {
        public static IComparer<IParcel> GetCgtComparer(Date disposalDate, CgtCalculationMethod method)
        {
            switch (method)
            {
                case CgtCalculationMethod.FirstInFirstOut:
                    return new FirstInFirstOutCgtComparer();
                case CgtCalculationMethod.LastInFirstOut:
                    return new LastInFirstOutCgtComparer();
                case CgtCalculationMethod.MaximizeGain:
                    return new MaximizeGainCgtComparer(disposalDate);
                case CgtCalculationMethod.MinimizeGain:
                    return new MinimizeGainCgtComparer(disposalDate);
                default:
                    throw new ArgumentException();
            }
        }

        public IEnumerable<ParcelSold> Calculate(IEnumerable<IParcel> parcelsOwned, Date disposalDate, int unitsSold, decimal amountReceived, IComparer<IParcel> cgtComparer)
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

        public ParcelSold CalculateParcelCgt(IParcel parcel, Date disposalDate, int unitsSold, decimal amountReceived)
        {
            decimal costBase;

            var properties = parcel.Properties[disposalDate];
            if (unitsSold == properties.Units)
                costBase = properties.CostBase;
            else
                costBase = (properties.CostBase * ((decimal)unitsSold / properties.Units)).ToCurrency(RoundingRule.Round);

            var capitalGain = amountReceived - costBase;
            var cgtMethod = CgtUtils.CgtMethodForParcel(parcel.AquisitionDate, disposalDate);
            var discountedGain = CgtUtils.DiscountedCgt(capitalGain, cgtMethod);       

            return new ParcelSold(parcel, unitsSold, costBase, amountReceived, capitalGain, cgtMethod, discountedGain);
        }
    }
}
