﻿using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.CorporateActions
{
    public class Dividend : ICorporateAction
    {
        public Guid Id { get; private set; }
        public IReadOnlyStock Stock { get; private set; }
        public Date Date { get; private set; }
        public CorporateActionType Type { get; private set; }
        public string Description { get; private set; }
        public Date PaymentDate { get; private set; }
        public decimal DividendAmount { get; private set; }
        public decimal PercentFranked { get; private set; }
        public decimal DRPPrice { get; private set; }

        internal Dividend(Guid id, IReadOnlyStock stock, Date actionDate, string description, Date paymentDate, decimal dividendAmount, decimal percentFranked, decimal drpPrice)
        {
            Id = id;
            Stock = stock;
            Date = actionDate;
            Type = CorporateActionType.Dividend;
            Description = description;
            PaymentDate = paymentDate;
            DividendAmount = dividendAmount;
            PercentFranked = percentFranked;
            DRPPrice = drpPrice;
        }

        public IEnumerable<IPortfolioTransaction> GetTransactionList(IReadOnlyHolding holding, IStockResolver stockResolver)
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
                DRPCashBalance = 0.00m,
                Comment = Description
            };
            transactions.Add(incomeReceived);

            /* Handle Dividend Reinvestment Plan */
            var holdingSettings = holding.Settings;
            if (dividendRules.DRPActive && holdingSettings.ParticipateInDrp && (DRPPrice != 0.00m))
            { 
                incomeReceived.CreateCashTransaction = false;

                int drpUnits;
                decimal costBase;

                if (dividendRules.DRPMethod == DRPMethod.RoundUp)
                {
                    drpUnits = (int)Math.Ceiling(amountPaid / DRPPrice);
                    costBase = amountPaid;
                }
                else if (dividendRules.DRPMethod == DRPMethod.RoundDown)
                {
                    drpUnits = (int)Math.Floor(amountPaid / DRPPrice);
                    costBase = amountPaid;
                }
                else if (dividendRules.DRPMethod == DRPMethod.RetainCashBalance)
                {
                    var drpCashBalance = holding.DrpAccount.Balance(Date);

                    var availableAmount = amountPaid + drpCashBalance;
                    drpUnits = (int)Math.Floor(availableAmount / DRPPrice);
                    costBase = (drpUnits * DRPPrice).ToCurrency(dividendRules.DividendRoundingRule);
                    incomeReceived.DRPCashBalance = availableAmount - costBase;
                }
                else
                {
                    drpUnits = (int)Math.Round(amountPaid / DRPPrice);
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
                        Comment = "DRP " + MathUtils.FormatCurrency(DRPPrice, false, true)
                    });
                } 
            } 

            return transactions;
        }

        public bool HasBeenApplied(IPortfolioTransactionList transactions)
        {
            return transactions.ForHolding(Stock.Id, PaymentDate).OfType<IncomeReceived>().Any();
        }
    }
}
