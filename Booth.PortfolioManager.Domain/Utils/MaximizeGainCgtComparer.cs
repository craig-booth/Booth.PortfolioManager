using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Utils
{
    class MaximizeGainCgtComparer : Comparer<Parcel>
    {
        private CgtComparerOld _CgtComparer;
        public MaximizeGainCgtComparer(Date disposalDate)
        {
            _CgtComparer = new CgtComparerOld(disposalDate, CGTCalculationMethod.MaximizeGain);
        }

        public override int Compare(Parcel a, Parcel b)
        {
            return _CgtComparer.Compare(a, b);
        }
    }
}
