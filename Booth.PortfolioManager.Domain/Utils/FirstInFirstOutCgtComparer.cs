using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Utils
{
    class FirstInFirstOutCgtComparer : Comparer<IParcel>
    {
        public override int Compare(IParcel a, IParcel b)
        {
            return a.AquisitionDate.CompareTo(b.AquisitionDate);
        }
    }
}
