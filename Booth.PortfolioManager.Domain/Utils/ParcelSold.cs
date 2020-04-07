using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Utils
{
    public struct ParcelSold
    {
        public IReadOnlyParcel Parcel { get; private set; }
        public int UnitsSold { get; private set; }
        public decimal CostBase { get; private set; }
        public decimal AmountReceived { get; private set; }
        public decimal CapitalGain { get; private set; }
        public CGTMethod CgtMethod { get; private set; }
        public decimal DiscountedGain { get; private set; }

        public ParcelSold(IReadOnlyParcel parcel, int unitsSold, decimal costBase, decimal amountReceived, decimal capitalGain, CGTMethod cgtMethod, decimal discountedGain)
        {
            Parcel = parcel;
            UnitsSold = unitsSold;
            CostBase = costBase;
            AmountReceived = amountReceived;
            CapitalGain = capitalGain;
            CgtMethod = cgtMethod;
            DiscountedGain = discountedGain;
        }
    }
}
