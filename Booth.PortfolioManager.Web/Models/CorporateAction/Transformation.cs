using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

namespace Booth.PortfolioManager.Web.Models.CorporateAction
{
    public class Transformation : CorporateAction
    {
        public override CorporateActionType Type => CorporateActionType.Transformation;
        public Date ImplementationDate { get; set; }
        public decimal CashComponent { get; set; }
        public bool RolloverRefliefApplies { get; set; }

        public List<ResultingStock> ResultingStocks { get; set; }

        public class ResultingStock
        {
            public Guid Stock { get; set; }
            public int OriginalUnits { get; set; }
            public int NewUnits { get; set; }
            public decimal CostBase { get; set; }
            public Date AquisitionDate { get; set; }
        }
    }
}
