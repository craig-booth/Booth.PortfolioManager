using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.CorporateActions
{
    public class CapitalReturn : CorporateAction, ICorporateAction
    {
        public Date PaymentDate { get; private set; }
        public decimal Amount { get; private set; }

        internal CapitalReturn(Guid id, Stock stock, Date actionDate, string description, Date paymentDate, decimal amount)
            : base(id, stock, CorporateActionType.CapitalReturn, actionDate, description)
        {
            PaymentDate = paymentDate;
            Amount = amount;
        }

        public IEnumerable<IPortfolioTransaction> GetTransactionList(IReadOnlyHolding holding, IStockResolver stockResolver)
        {
            var transactions = new List<IPortfolioTransaction>();

            var holdingProperties = holding.Properties[Date];
            if (holdingProperties.Units == 0)
                return transactions;

            var dividendRules = Stock.DividendRules[Date];

            var amount = (holdingProperties.Units * Amount).ToCurrency(dividendRules.DividendRoundingRule);

            var returnOfCapital = new ReturnOfCapital()
            {
                Id = Guid.NewGuid(),
                Date = PaymentDate,
                Stock = Stock,
                RecordDate = Date,
                Amount = amount,
                CreateCashTransaction = true,
                Comment = Description
            };
            transactions.Add(returnOfCapital);

            return transactions;
        }

        public bool HasBeenApplied(IPortfolioTransactionList transactions)
        {
            return transactions.ForHolding(Stock.Id, PaymentDate).OfType<ReturnOfCapital>().Any();
        }
    }
}
