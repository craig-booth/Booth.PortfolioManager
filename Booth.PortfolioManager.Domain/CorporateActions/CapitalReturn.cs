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
    public class CapitalReturn : CorporateAction
    {
        public Date PaymentDate { get; private set; }
        public decimal Amount { get; private set; }

        public CapitalReturn(Guid id, IReadOnlyStock stock, Date actionDate, string description, Date paymentDate, decimal amount)
            : base(id, stock, actionDate, (description != "") ? description : "Capital Return " + amount.ToString("$#,##0.00###"))
        {
            PaymentDate = paymentDate;
            Amount = amount;
        }

        public override IEnumerable<IPortfolioTransaction> GetTransactionList(IReadOnlyHolding holding, IStockResolver stockResolver)
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

        public override bool HasBeenApplied(IPortfolioTransactionList transactions)
        {
            return transactions.ForHolding(Stock.Id, PaymentDate).OfType<ReturnOfCapital>().Any();
        }
    }
}
