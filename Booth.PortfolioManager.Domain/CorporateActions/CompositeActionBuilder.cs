using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;

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

    class CompositeActionBuilder : ICompositeActionBuilder
    {
        private Stock _Stock;
        private Guid _Id;
        private Date _RecordDate;
        private string _Description;

        private Action<CompositeAction> _Callback;
        private CorporateActionList _ChildActions;

        public CompositeActionBuilder(Stock stock, Guid id, Date recordDate, string description, Action<CompositeAction> callback)
        {
            _ChildActions = new CorporateActionList(stock);

            _Stock = stock;
            _Id = id;
            _RecordDate = recordDate;
            _Description = description;

            _Callback = callback;
        }

        public void Finish()
        {
            var compositeAction = new CompositeAction(_Id, _Stock, _RecordDate, _Description, _ChildActions);

            _Callback?.Invoke(compositeAction);
        }

        public ICompositeActionBuilder AddCapitalReturn(string description, Date paymentDate, decimal amount)
        {
            _ChildActions.AddCapitalReturn(Guid.NewGuid(), _RecordDate, description, paymentDate, amount);

            return this;
        }

        public ICompositeActionBuilder AddDividend(string description, Date paymentDate, decimal dividendAmount, decimal percentFranked, decimal drpPrice)
        {
            _ChildActions.AddDividend(Guid.NewGuid(), _RecordDate, description, paymentDate, dividendAmount, percentFranked, drpPrice);

            return this;
        }

        public ICompositeActionBuilder AddSplitConsolidation(string description, int originalUnits, int newUnits)
        {
            _ChildActions.AddSplitConsolidation(Guid.NewGuid(), _RecordDate, description, originalUnits, newUnits);

            return this;
        }

        public ICompositeActionBuilder AddTransformation(string description, Date implementationDate, decimal cashComponent, bool rolloverReliefApplies, IEnumerable<Transformation.ResultingStock> resultingStocks)
        {
            _ChildActions.AddTransformation(Guid.NewGuid(), _RecordDate, description, implementationDate, cashComponent, rolloverReliefApplies, resultingStocks);

            return this;
        }
    }
}
