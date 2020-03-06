﻿using System;
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
        public Stock Stock { get; }

        internal CorporateActionList(Stock stock, IEventList eventList)
        {
            Stock = stock;
            _Events = eventList;
        }

        protected void PublishEvent(Event @event)
        {
            _Events.Add(@event);
        }

        public void AddCapitalReturn(Guid id, Date recordDate, string description, Date paymentDate, decimal amount)
        {
            if (description == "")
                description = "Capital Return " + amount.ToString("$#,##0.00###");

            var @event = new CapitalReturnAddedEvent(Stock.Id, Stock.Version, id, recordDate, description, paymentDate, amount);

            Apply(@event);
            PublishEvent(@event);
        }

        public void Apply(CapitalReturnAddedEvent @event)
        {
            var capitalReturn = new CapitalReturn(@event.ActionId, Stock, @event.ActionDate, @event.Description, @event.PaymentDate, @event.Amount);

            Add(capitalReturn);
        }

        public void AddDividend(Guid id, Date recordDate, string description, Date paymentDate, decimal dividendAmount, decimal percentFranked, decimal drpPrice)
        {
            if (description == "")
                description = "Dividend " + MathUtils.FormatCurrency(dividendAmount, false, true);

            var @event = new DividendAddedEvent(Stock.Id, Stock.Version, id, recordDate, description, paymentDate, dividendAmount, percentFranked, drpPrice);

            Apply(@event);
            PublishEvent(@event);
        }

        public void Apply(DividendAddedEvent @event)
        {
            var dividend = new Dividend(@event.ActionId, Stock, @event.ActionDate, @event.Description, @event.PaymentDate, @event.DividendAmount, @event.PercentFranked, @event.DRPPrice);

            Add(dividend);
        }

        public void AddTransformation(Guid id, Date recordDate, string description, Date implementationDate, decimal cashComponent, bool rolloverReliefApplies, IEnumerable<Transformation.ResultingStock> resultingStocks)
        {
            var eventResultingStocks = resultingStocks.Select(x => new TransformationAddedEvent.ResultingStock(x.Stock, x.OriginalUnits, x.NewUnits, x.CostBasePercentage, x.AquisitionDate));

            var @event = new TransformationAddedEvent(Stock.Id, Stock.Version, id, recordDate, description, implementationDate, cashComponent, rolloverReliefApplies, eventResultingStocks);                

            Apply(@event);
            PublishEvent(@event);
        }

        public void Apply(TransformationAddedEvent @event)
        {
            var transformationResultingStocks = @event.ResultingStocks.Select(x => new Transformation.ResultingStock(x.Stock, x.OriginalUnits, x.NewUnits, x.CostBasePercentage, x.AquisitionDate));
            var transformation = new Transformation(@event.ActionId, Stock, @event.ActionDate, @event.Description, @event.ImplementationDate, @event.CashComponent, @event.RolloverRefliefApplies, transformationResultingStocks);

            Add(transformation);
        }

        public void AddSplitConsolidation(Guid id, Date recordDate, string description, int originalUnits, int newUnits)
        {

        }

        public void Apply(SplitConsolidationAddedEvent @event)
        {

        }

        public ICompositeActionBuilder StartCompositeAction(Guid id, Date recordDate, string description)
        {
            if (description == "")
                description = "Complex corporate action ";

            var builder = new CompositeActionBuilder(Stock, id, recordDate, description, x => { Apply(x); PublishEvent(x); });

            return builder;
        }

        public void Apply(CompositeActionAddedEvent @event)
        {

        }
    }

}
