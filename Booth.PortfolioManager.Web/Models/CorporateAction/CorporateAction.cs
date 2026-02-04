using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

namespace Booth.PortfolioManager.Web.Models.CorporateAction
{
    public enum CorporateActionType {CapitalReturn, CompositeAction, Dividend, SplitConsolidation, Transformation}
    public abstract class CorporateAction
    {
        public Guid Id { get; set; }
        public abstract CorporateActionType Type { get; }
        public Guid Stock { get; set; }
        public Date ActionDate { get; set; }
        public string Description { get; set; }
    }     
}
