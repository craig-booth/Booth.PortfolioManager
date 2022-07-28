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
    public class Dividend : CorporateAction
    {
        public Date PaymentDate { get; private set; }
        public decimal DividendAmount { get; private set; }
        public decimal PercentFranked { get; private set; }
        public decimal DrpPrice { get; private set; }

        public Dividend(Guid id, IReadOnlyStock stock, Date actionDate, string description, Date paymentDate, decimal dividendAmount, decimal percentFranked, decimal drpPrice)
            : base(id, stock, actionDate, (description != "") ? description : "Dividend " + MathUtils.FormatCurrency(dividendAmount, false, true))
        {
            PaymentDate = paymentDate;
            DividendAmount = dividendAmount;
            PercentFranked = percentFranked;
            DrpPrice = drpPrice;
        }

        public override IEnumerable<IPortfolioTransaction> GetTransactionList(IReadOnlyHolding holding, IStockResolver stockResolver)
        {
            var transactions = new List<IPortfolioTransaction>();

            var holdingProperties = holding.Properties[Date];        
            if (holdingProperties.Units == 0)
                return transactions;

            var dividendRules = Stock.DividendRules[Date];

            var totalAmount = holdingProperties.Units * DividendAmount;

            var amountPaid = totalAmount.ToCurrency(dividendRules.DividendRoundingRule);
            var franked = (totalAmount * PercentFranked).ToCurrency(dividendRules.DividendRoundingRule);
            var unFranked = (totalAmount * (1 - PercentFranked)).ToCurrency(dividendRules.DividendRoundingRule);
            var frankingCredits = (((totalAmount / (1 - dividendRules.CompanyTaxRate)) - totalAmount) * PercentFranked).ToCurrency(dividendRules.DividendRoundingRule);
            
            var incomeReceived = new IncomeReceived()
            {
                Id = Guid.NewGuid(),
                Date = PaymentDate,
                Stock = Stock,
                RecordDate = Date,
                FrankedAmount = franked,
                UnfrankedAmount = unFranked,
                FrankingCredits = frankingCredits,
                Interest = 0.00m,
                TaxDeferred = 0.00m,
                CreateCashTransaction = true,
                DrpCashBalance = 0.00m,
                Comment = Description
            };
            transactions.Add(incomeReceived);

            /* Handle Dividend Reinvestment Plan */
            var holdingSettings = holding.Settings;
            if (dividendRules.DrpActive && holdingSettings.ParticipateInDrp && (DrpPrice != 0.00m))
            { 
                incomeReceived.CreateCashTransaction = false;

                int drpUnits;
                decimal costBase;

                if (dividendRules.DrpMethod == DrpMethod.RoundUp)
                {
                    drpUnits = (int)Math.Ceiling(amountPaid / DrpPrice);
                    costBase = amountPaid;
                }
                else if (dividendRules.DrpMethod == DrpMethod.RoundDown)
                {
                    drpUnits = (int)Math.Floor(amountPaid / DrpPrice);
                    costBase = amountPaid;
                }
                else if (dividendRules.DrpMethod == DrpMethod.RetainCashBalance)
                {
                    var drpCashBalance = holding.DrpAccount.Balance(Date);

                    var availableAmount = amountPaid + drpCashBalance;
                    drpUnits = (int)Math.Floor(availableAmount / DrpPrice);
                    costBase = (drpUnits * DrpPrice).ToCurrency(dividendRules.DividendRoundingRule);
                    incomeReceived.DrpCashBalance = availableAmount - costBase;
                }
                else
                {
                    drpUnits = (int)Math.Round(amountPaid / DrpPrice);
                    costBase = amountPaid;
                }

                if (drpUnits > 0)
                {
                    transactions.Add(new OpeningBalance()
                    {
                        Id = Guid.NewGuid(),
                        Date = PaymentDate,
                        Stock = Stock,
                        Units = drpUnits,
                        CostBase = costBase,
                        AquisitionDate = PaymentDate,
                        Comment = "DRP " + MathUtils.FormatCurrency(DrpPrice, false, true)
                    });
                } 
            } 

            return transactions;
        }

        public override bool HasBeenApplied(IPortfolioTransactionList transactions)
        {
            return transactions.ForHolding(Stock.Id, PaymentDate).OfType<IncomeReceived>().Any();
        }
    }
}
