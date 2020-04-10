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
    public class CapitalReturn : ICorporateAction
    {
        public Guid Id { get; private set; }
        public IReadOnlyStock Stock { get; private set; }
        public Date Date { get; private set; }
        public CorporateActionType Type { get; private set; }
        public string Description { get; private set; }
        public Date PaymentDate { get; private set; }
        public decimal Amount { get; private set; }

        internal CapitalReturn(Guid id, IReadOnlyStock stock, Date actionDate, string description, Date paymentDate, decimal amount)
        {
            Id = id;
            Stock = stock;
            Date = actionDate;
            Type = CorporateActionType.CapitalReturn;
            Description = description;
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
