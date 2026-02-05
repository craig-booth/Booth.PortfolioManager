using System;
using System.Collections.Generic;
using System.Text;

namespace Booth.PortfolioManager.Web.Models.CorporateAction
{
    public class CompositeAction : CorporateAction
    {
        public override CorporateActionType Type => CorporateActionType.CompositeAction;
        public List<CorporateAction> ChildActions { get; set; }
    }
}
