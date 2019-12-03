using System;
using System.Collections.Generic;
using System.Text;

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

        internal CapitalReturn(Guid id, Stock stock, Date actionDate, string description, Date paymentDate, decimal amount)
            : base(id, stock, CorporateActionType.CapitalReturn, actionDate, description)
        {
            PaymentDate = paymentDate;
            Amount = amount;
        }

        public override IEnumerable<Transaction> GetTransactionList(Holding holding)
        {
            var transactions = new List<Transaction>();

            return transactions;
        }

        public override bool HasBeenApplied(ITransactionCollection transactions)
        {
            return false;
        }
    }
}
