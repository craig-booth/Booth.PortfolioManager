using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.CorporateActions;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.Stocks
{

    public interface ICorporateActionList : ITransactionList<CorporateAction>
    {
        void Add(CorporateAction action);
        void AddCapitalReturn(Guid id, Date recordDate, string description, Date paymentDate, decimal amount);
        void AddDividend(Guid id, Date recordDate, string description, Date paymentDate, decimal dividendAmount, decimal percentFranked, decimal drpPrice);
        void AddTransformation(Guid id, Date recordDate, string description, Date implementationDate, decimal cashComponent, bool rolloverReliefApplies, IEnumerable<Transformation.ResultingStock> resultingStocks);
        void AddSplitConsolidation(Guid id, Date recordDate, string description, int originalUnits, int newUnits);
        ICompositeActionBuilder StartCompositeAction(Guid id, Date recordDate, string description);
    }

    public class CorporateActionList : TransactionList<CorporateAction>, ICorporateActionList
    {
        private Stock _Stock;
        public IReadOnlyStock Stock { get { return _Stock; } }


        internal CorporateActionList(Stock stock)
        {
            _Stock = stock;
        }

        public new void Add(CorporateAction action)
        {
            action.Stock = _Stock;
            base.Add(action);
        }
        
        public void AddCapitalReturn(Guid id, Date recordDate, string description, Date paymentDate, decimal amount)
        {
            if (description == "")
                description = "Capital Return " + amount.ToString("$#,##0.00###");

            var capitalReturn = new CapitalReturn(id, Stock, recordDate, description, paymentDate, amount);

            Add(capitalReturn);
        }

        public void AddDividend(Guid id, Date recordDate, string description, Date paymentDate, decimal dividendAmount, decimal percentFranked, decimal drpPrice)
        {
            if (description == "")
                description = "Dividend " + MathUtils.FormatCurrency(dividendAmount, false, true);

            var dividend = new Dividend(id, Stock, recordDate, description, paymentDate, dividendAmount, percentFranked, drpPrice);
            Add(dividend);
        }

        public void AddTransformation(Guid id, Date recordDate, string description, Date implementationDate, decimal cashComponent, bool rolloverReliefApplies, IEnumerable<Transformation.ResultingStock> resultingStocks)
        {
            if (description == "")
                description = "Transformation";

            var transformation = new Transformation(id, Stock, recordDate, description, implementationDate, cashComponent, rolloverReliefApplies, resultingStocks);
            Add(transformation);
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

            var splitConsolidation = new SplitConsolidation(id, Stock, recordDate, description, originalUnits, newUnits);
            Add(splitConsolidation);
        }

        public ICompositeActionBuilder StartCompositeAction(Guid id, Date recordDate, string description)
        {
            if (description == "")
                description = "Complex corporate action";

            var builder = new CompositeActionBuilder(_Stock, id, recordDate, description, x => { Add(x); });

            return builder;
        }

    }

}
