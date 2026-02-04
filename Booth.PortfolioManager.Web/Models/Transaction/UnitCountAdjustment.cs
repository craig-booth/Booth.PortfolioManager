using System;
using System.Collections.Generic;
using System.Text;


namespace Booth.PortfolioManager.Web.Models.Transaction
{
    public class UnitCountAdjustment : Transaction
    {
        public override TransactionType Type => TransactionType.UnitCountAdjustment;
        public int OriginalUnits { get; set; }
        public int NewUnits { get; set; }
    } 
}
