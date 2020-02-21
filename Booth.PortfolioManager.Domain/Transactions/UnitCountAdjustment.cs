using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

namespace Booth.PortfolioManager.Domain.Transactions
{
    class UnitCountAdjustment : Transaction
    {
        public int OriginalUnits { get; set; }
        public int NewUnits { get; set; }

        public override string Description
        {
            get
            {
                  return "Adjust unit count using ratio " + OriginalUnits.ToString("n0") + ":" + NewUnits.ToString("n0");
            }
        }
    }
}
