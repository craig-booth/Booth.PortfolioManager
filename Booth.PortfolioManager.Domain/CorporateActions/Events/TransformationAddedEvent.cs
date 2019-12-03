using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.EventStore;

namespace Booth.PortfolioManager.Domain.CorporateActions.Events
{
    public class TransformationAddedEvent : CorporateActionAddedEvent
    {
        public Date ImplementationDate { get; set; }
        public decimal CashComponent { get; set; }
        public bool RolloverRefliefApplies { get; set; }

        public List<ResultingStock> ResultingStocks = new List<ResultingStock>();

        public TransformationAddedEvent(Guid entityId, int version, Guid actionId, Date actionDate, string description, Date implementationDate, decimal cashComponent, bool rolloverReliefApplies, IEnumerable<ResultingStock> resultingStocks)
            : base(entityId, version, actionId, actionDate, description)
        {
            ImplementationDate = implementationDate;
            CashComponent = cashComponent;
            RolloverRefliefApplies = rolloverReliefApplies;
            ResultingStocks.AddRange(resultingStocks);
        }

        public class ResultingStock
        {
            public Guid Stock { get; set; }
            public int OriginalUnits { get; set; }
            public int NewUnits { get; set; }
            public decimal CostBase { get; set; }
            public Date AquisitionDate { get; set; }

            public ResultingStock(Guid stock, int originalUnits, int newUnits, decimal costBase, Date aquisitionDate)
            {
                Stock = stock;
                OriginalUnits = originalUnits;
                NewUnits = newUnits;
                CostBase = costBase;
                AquisitionDate = aquisitionDate;
            }
        }
    }
}
