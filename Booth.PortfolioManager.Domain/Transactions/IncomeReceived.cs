using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Transactions
{
    public class IncomeReceived : IPortfolioTransaction
    {
        public Guid Id { get; set; }
        public Date Date { get; set; }
        public IReadOnlyStock Stock { get; set; }
        public string Comment { get; set; }
        public Date RecordDate { get; set; }
        public decimal FrankedAmount { get; set; }
        public decimal UnfrankedAmount { get; set; }
        public decimal FrankingCredits { get; set; }
        public decimal Interest { get; set; }
        public decimal TaxDeferred { get; set; }
        public bool CreateCashTransaction { get; set; }
        public decimal DRPCashBalance { get; set; }

        public string Description
        {
            get
            {
                return "Income received " + MathUtils.FormatCurrency(CashIncome, false, true);
            }
        }

        public decimal CashIncome
        {
            get { return FrankedAmount + UnfrankedAmount + Interest + TaxDeferred; }
        }

        public decimal NonCashIncome
        {
            get { return FrankingCredits; }
        }

        public decimal TotalIncome
        {
            get { return CashIncome + NonCashIncome; }
        }
    }
}
