using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Utils
{
    class FirstInFirstOutCgtComparer : Comparer<Parcel>
    {
        private CgtComparerOld _CgtComparer;
        public FirstInFirstOutCgtComparer()
        {
            _CgtComparer = new CgtComparerOld(Date.MinValue, CGTCalculationMethod.FirstInFirstOut);
        }

        public override int Compare(Parcel a, Parcel b)
        {
            return _CgtComparer.Compare(a, b);
        }
    }
}
