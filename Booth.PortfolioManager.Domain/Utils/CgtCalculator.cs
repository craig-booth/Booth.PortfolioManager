using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Utils
{
    class CgtCalculator
    {
        public static Date IndexationEndDate = new Date(1999, 09, 21);

        public static CGTMethod CgtMethodForParcel(Date aquisitionDate, Date eventDate)
        {
            return CgtCalculatorOld.CgtMethodForParcel(aquisitionDate, eventDate);
        }

        public static decimal DiscountedCgt(decimal cgtAmount, Date aquisitionDate, Date eventDate)
        {
            return CgtCalculatorOld.DiscountedCgt(cgtAmount, aquisitionDate, eventDate);
        }

        public static IComparer<Parcel> GetCgtComparer(Date disposalDate, CGTCalculationMethod method)
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

        public IEnumerable<ParcelSold> CalculateParcelCgt(IEnumerable<Parcel> parcelsOwned, Date disposalDate, int unitsSold, decimal amountReceived, IComparer<Parcel> cgtComparer)
        {
            var calculation = CgtCalculatorOld.CalculateCapitalGain(parcelsOwned, disposalDate, unitsSold, amountReceived, cgtComparer, CGTCalculationMethod.FirstInFirstOut);

            if (calculation.ParcelsSold.Sum(x => x.UnitsSold) < unitsSold)
                throw new ArgumentException("Not enough units");

            return calculation.ParcelsSold.Select(x => new ParcelSold(x.Parcel, x.UnitsSold, x.CostBase, x.AmountReceived, x.CapitalGain, x.DiscountedGain));
        }

        public ParcelSold CalculateParcelCgt(Parcel parcel, Date disposalDate, int unitsSold, decimal amountReceived)
        {
            var parcelSold = new ParcelSoldOld(parcel, unitsSold, disposalDate);

            parcelSold.CalculateCapitalGain(amountReceived);

            return new ParcelSold(parcel, unitsSold, parcelSold.CostBase, amountReceived, parcelSold.CapitalGain, parcelSold.DiscountedGain);
        }
    }
}
