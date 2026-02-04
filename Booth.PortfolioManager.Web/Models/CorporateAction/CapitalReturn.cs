using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

namespace Booth.PortfolioManager.Web.Models.CorporateAction
{
    public class CapitalReturn : CorporateAction
    {
        public override CorporateActionType Type => CorporateActionType.CapitalReturn;
        public Date PaymentDate { get; set; }
        public decimal Amount { get; set; }
    }
}


