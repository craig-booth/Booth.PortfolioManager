using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;
using Booth.EventStore;

using Booth.PortfolioManager.Domain.CorporateActions;
using Booth.PortfolioManager.Domain.CorporateActions.Events;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.Stocks
{

    public interface ICorporateActionList : ITransactionList<ICorporateAction>
    {
        void AddCapitalReturn(Guid id, Date recordDate, string description, Date paymentDate, decimal amount);
        void AddDividend(Guid id, Date recordDate, string description, Date paymentDate, decimal dividendAmount, decimal percentFranked, decimal drpPrice);
        void AddTransformation(Guid id, Date recordDate, string description, Date implementationDate, decimal cashComponent, bool rolloverReliefApplies, IEnumerable<Transformation.ResultingStock> resultingStocks);
        void AddSplitConsolidation(Guid id, Date recordDate, string description, int originalUnits, int newUnits);
        ICompositeActionBuilder StartCompositeAction(Guid id, Date recordDate, string description);
    }

    public class CorporateActionList : TransactionList<ICorporateAction>, ICorporateActionList
    {
        private IEventList _Events;

        private Stock _Stock;
        public IReadOnlyStock Stock { get { return _Stock; } }


        internal CorporateActionList(Stock stock, IEventList eventList)
        {
            _Stock = stock;
            _Events = eventList;
        }

        protected void PublishEvent(Event @event)
        {
            _Events.Add(@event);
        }

        public void Apply(CorporateActionAddedEvent @event)
        {
            var corporateAction = CorporateActionFromEvent(@event);

            Add(corporateAction);
        }
        private ICorporateAction CorporateActionFromEvent(CorporateActionAddedEvent @event)
        {
            if (@event is CapitalReturnAddedEvent capitalReturnEvent)
                return CorporateActionFromEvent(capitalReturnEvent);
            else if (@event is DividendAddedEvent dividendEvent)
                return CorporateActionFromEvent(dividendEvent);
            else if (@event is SplitConsolidationAddedEvent splitEvent)
                return CorporateActionFromEvent(splitEvent);
            else if (@event is TransformationAddedEvent transformEvent)
                return CorporateActionFromEvent(transformEvent);
            else if (@event is CompositeActionAddedEvent compositeEvent)
                return CorporateActionFromEvent(compositeEvent);
            else
                return null;
        }
        
        public void AddCapitalReturn(Guid id, Date recordDate, string description, Date paymentDate, decimal amount)
        {
            if (description == "")
                description = "Capital Return " + amount.ToString("$#,##0.00###");

            var @event = new CapitalReturnAddedEvent(_Stock.Id, _Stock.Version, id, recordDate, description, paymentDate, amount);

            Apply(@event);
            PublishEvent(@event);
        }

        private ICorporateAction CorporateActionFromEvent(CapitalReturnAddedEvent @event)
        {
            var capitalReturn = new CapitalReturn(@event.ActionId, Stock, @event.ActionDate, @event.Description, @event.PaymentDate, @event.Amount);

            return capitalReturn;
        }

        public void AddDividend(Guid id, Date recordDate, string description, Date paymentDate, decimal dividendAmount, decimal percentFranked, decimal drpPrice)
        {
            if (description == "")
                description = "Dividend " + MathUtils.FormatCurrency(dividendAmount, false, true);

            var @event = new DividendAddedEvent(_Stock.Id, _Stock.Version, id, recordDate, description, paymentDate, dividendAmount, percentFranked, drpPrice);

            Apply(@event);
            PublishEvent(@event);
        }

        private ICorporateAction CorporateActionFromEvent(DividendAddedEvent @event)
        {
            var dividend = new Dividend(@event.ActionId, Stock, @event.ActionDate, @event.Description, @event.PaymentDate, @event.DividendAmount, @event.PercentFranked, @event.DrpPrice);

            return dividend;
        }

        public void AddTransformation(Guid id, Date recordDate, string description, Date implementationDate, decimal cashComponent, bool rolloverReliefApplies, IEnumerable<Transformation.ResultingStock> resultingStocks)
        {
            if (description == "")
                description = "Transformation";

            var eventResultingStocks = resultingStocks.Select(x => new TransformationAddedEvent.ResultingStock(x.Stock, x.OriginalUnits, x.NewUnits, x.CostBasePercentage, x.AquisitionDate));
            var @event = new TransformationAddedEvent(_Stock.Id, _Stock.Version, id, recordDate, description, implementationDate, cashComponent, rolloverReliefApplies, eventResultingStocks);                

            Apply(@event);
            PublishEvent(@event);
        }

        private ICorporateAction CorporateActionFromEvent(TransformationAddedEvent @event)
        {
            var transformationResultingStocks = @event.ResultingStocks.Select(x => new Transformation.ResultingStock(x.Stock, x.OriginalUnits, x.NewUnits, x.CostBasePercentage, x.AquisitionDate));
            var transformation = new Transformation(@event.ActionId, Stock, @event.ActionDate, @event.Description, @event.ImplementationDate, @event.CashComponent, @event.RolloverRefliefApplies, transformationResultingStocks);

            return transformation;
        }

        public void AddSplitConsolidation(Guid id, Date recordDate, string description, int originalUnits, int newUnits)
        {
            if (description == "")
            {
                if (originalUnits <= newUnits)
                    description = String.Format("{0} for {1} Stock Split", originalUnits, newUnits);
                else
                    description = String.Format("{0} for {1} Stock Comsolication", originalUnits, newUnits);
            }           

            var @event = new SplitConsolidationAddedEvent(_Stock.Id, _Stock.Version, id, recordDate, description, originalUnits, newUnits);

            Apply(@event);
            PublishEvent(@event);
        }

        private ICorporateAction CorporateActionFromEvent(SplitConsolidationAddedEvent @event)
        {
            var splitConsolidation = new SplitConsolidation(@event.ActionId, Stock, @event.ActionDate, @event.Description, @event.OriginalUnits, @event.NewUnits);

            return splitConsolidation;
        }

        public ICompositeActionBuilder StartCompositeAction(Guid id, Date recordDate, string description)
        {
            if (description == "")
                description = "Complex corporate action";

            var builder = new CompositeActionBuilder(_Stock, id, recordDate, description, x => { Apply(x); PublishEvent(x); });

            return builder;
        }

        private ICorporateAction CorporateActionFromEvent(CompositeActionAddedEvent @event)
        {
            var childActions = @event.ChildActions.Select(x => CorporateActionFromEvent(x));
            var compositeAction = new CompositeAction(@event.ActionId, Stock, @event.ActionDate, @event.Description, childActions);

            return compositeAction; 
        }
    }

}
