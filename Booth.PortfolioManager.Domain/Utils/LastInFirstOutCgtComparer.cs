using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Utils
{
    class LastInFirstOutCgtComparer : Comparer<IParcel>
    {
        public override int Compare(IParcel a, IParcel b)
        {
            return b.AquisitionDate.CompareTo(a.AquisitionDate);
        }
    }
}
