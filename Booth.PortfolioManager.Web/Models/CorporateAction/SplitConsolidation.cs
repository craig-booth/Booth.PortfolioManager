using System;
using System.Collections.Generic;
using System.Text;

namespace Booth.PortfolioManager.Web.Models.CorporateAction
{
    public class SplitConsolidation : CorporateAction
    {
        public override CorporateActionType Type => CorporateActionType.SplitConsolidation;
        public int OriginalUnits { get; set; }
        public int NewUnits { get; set; }
    }
}
