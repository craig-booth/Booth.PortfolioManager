using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Utils
{
    class LastInFirstOutCgtComparer : Comparer<Parcel>
    {
        public override int Compare(Parcel a, Parcel b)
        {
            return b.AquisitionDate.CompareTo(a.AquisitionDate);
        }
    }
}
