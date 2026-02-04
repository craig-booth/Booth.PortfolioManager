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

        public List<ResultingStock> ResultingStocks = new List<ResultingStock>();

        public void AddResultingStock(Guid stock, int originalUnits, int newUnits, decimal costBase, Date aquisitionDate)
        {
            var resultingStock = new ResultingStock()
            {
                Stock = stock,
                OriginalUnits = originalUnits,
                NewUnits = newUnits,
                CostBase = costBase,
                AquisitionDate = aquisitionDate
            };
            ResultingStocks.Add(resultingStock);
        }

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
