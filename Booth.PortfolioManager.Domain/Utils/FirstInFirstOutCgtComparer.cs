using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Utils
{
    class FirstInFirstOutCgtComparer : Comparer<Parcel>
    {
        public override int Compare(Parcel a, Parcel b)
        {
            return a.AquisitionDate.CompareTo(b.AquisitionDate);
        }
    }
}
