using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Utils
{
    class FirstInFirstOutCgtComparer : Comparer<IReadOnlyParcel>
    {
        public override int Compare(IReadOnlyParcel a, IReadOnlyParcel b)
        {
            return a.AquisitionDate.CompareTo(b.AquisitionDate);
        }
    }
}
