using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Utils
{
    class MinimizeGainCgtComparer : Comparer<Parcel>
    {
        private CgtComparerOld _CgtComparer;
        public MinimizeGainCgtComparer(Date disposalDate)
        {
            _CgtComparer = new CgtComparerOld(disposalDate, CGTCalculationMethod.MinimizeGain);
        }

        public override int Compare(Parcel a, Parcel b)
        {
            return _CgtComparer.Compare(a, b);
        }
    }
}
