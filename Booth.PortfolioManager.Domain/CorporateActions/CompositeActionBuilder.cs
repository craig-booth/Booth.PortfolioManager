using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.CorporateActions.Events;

namespace Booth.PortfolioManager.Domain.CorporateActions
{
    public interface ICompositeActionBuilder
    {
        ICompositeActionBuilder AddCapitalReturn(string description, Date paymentDate, decimal amount);
        ICompositeActionBuilder AddDividend(string description, Date paymentDate, decimal dividendAmount, decimal percentFranked, decimal drpPrice);
        ICompositeActionBuilder AddTransformation(string description, Date implementationDate, decimal cashComponent, bool rolloverReliefApplies, IEnumerable<Transformation.ResultingStock> resultingStocks);
        ICompositeActionBuilder AddSplitConsolidation(string description, int originalUnits, int newUnits);

        void Finish();
    }

    public class CompositeActionBuilder : ICompositeActionBuilder
    {
        private CompositeActionAddedEvent _Event;
        private Action<CompositeActionAddedEvent> _Callback;
        private IEventList _Events;
        private CorporateActionList _ChildActions;
        public CompositeActionBuilder(Stock stock, Guid id, Date recordDate, string description, Action<CompositeActionAddedEvent> callback)
        {
            _Events = new EventList();
            _ChildActions = new CorporateActionList(stock, _Events);

            _Event = new CompositeActionAddedEvent(stock.Id, stock.Version, id, recordDate, description);
            _Callback = callback;
        }

        public void Finish()
        {
            if (_Callback != null)
            {
                _Callback(_Event);
            }
        }

        public ICompositeActionBuilder AddCapitalReturn(string description, Date paymentDate, decimal amount)
        {
            _ChildActions.AddCapitalReturn(_Event.ActionId, _Event.ActionDate, description, paymentDate, amount);

            return this;
        }

        public ICompositeActionBuilder AddDividend(string description, Date paymentDate, decimal dividendAmount, decimal percentFranked, decimal drpPrice)
        {
            _ChildActions.AddDividend(_Event.ActionId, _Event.ActionDate, description, paymentDate, dividendAmount, percentFranked, drpPrice);

            return this;
        }

        public ICompositeActionBuilder AddSplitConsolidation(string description, int originalUnits, int newUnits)
        {
            _ChildActions.AddSplitConsolidation(_Event.ActionId, _Event.ActionDate, description, originalUnits, newUnits);

            return this;
        }

        public ICompositeActionBuilder AddTransformation(string description, Date implementationDate, decimal cashComponent, bool rolloverReliefApplies, IEnumerable<Transformation.ResultingStock> resultingStocks)
        {
            _ChildActions.AddTransformation(_Event.ActionId, _Event.ActionDate, description, implementationDate, cashComponent, rolloverReliefApplies, resultingStocks);

            return this;
        }
    }
}
