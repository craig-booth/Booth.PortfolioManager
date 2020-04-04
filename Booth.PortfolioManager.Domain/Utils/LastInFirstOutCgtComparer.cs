using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Utils
{
    class LastInFirstOutCgtComparer : Comparer<Parcel>
    {
        private CgtComparerOld _CgtComparer;
        public LastInFirstOutCgtComparer()
        {
            _CgtComparer = new CgtComparerOld(Date.MinValue, CGTCalculationMethod.LastInFirstOut);
        }

        public override int Compare(Parcel a, Parcel b)
        {
            return _CgtComparer.Compare(a, b);
        }
    }
}
