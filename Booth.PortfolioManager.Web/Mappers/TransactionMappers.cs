using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.RestApi.Portfolios;
using MongoDB.Driver.Core.Operations;

namespace Booth.PortfolioManager.Web.Mappers
{
    static class TransactionMappers
    {

        public static RestApi.Transactions.Transaction ToResponse(this Domain.Transactions.IPortfolioTransaction transaction)
        {
            if (transaction is Domain.Transactions.Aquisition aquisition)
                return aquisition.ToResponse();
            else if (transaction is Domain.Transactions.CashTransaction cashTransaction)
                return cashTransaction.ToResponse();
            else if (transaction is Domain.Transactions.CostBaseAdjustment costBaseAdjustment)
                return costBaseAdjustment.ToResponse();
            else if (transaction is Domain.Transactions.Disposal disposal)
                return disposal.ToResponse();
            else if (transaction is Domain.Transactions.IncomeReceived incomeReceived)
                return incomeReceived.ToResponse();
            else if (transaction is Domain.Transactions.OpeningBalance openingBalance)
                return openingBalance.ToResponse();
            else if (transaction is Domain.Transactions.ReturnOfCapital returnOfCapital)
                return returnOfCapital.ToResponse();
            else if (transaction is Domain.Transactions.UnitCountAdjustment unitCountAdjustment)
                return unitCountAdjustment.ToResponse();
            else
                throw new NotSupportedException();
        }

        public static RestApi.Transactions.Aquisition ToResponse(this Domain.Transactions.Aquisition aquisition)
        {
            var response = new RestApi.Transactions.Aquisition();

            PopulatePortfolioTransaction(response, aquisition);

            response.AveragePrice = aquisition.AveragePrice;
            response.CreateCashTransaction = aquisition.CreateCashTransaction;
            response.TransactionCosts = aquisition.TransactionCosts;
            response.Units = aquisition.Units;

            return response;
        }

        public static RestApi.Transactions.CashTransaction ToResponse(this Domain.Transactions.CashTransaction cashTransaction)
        {
            var response = new RestApi.Transactions.CashTransaction();

            PopulatePortfolioTransaction(response, cashTransaction);

            response.CashTransactionType = cashTransaction.CashTransactionType.ToResponse();
            response.Amount = cashTransaction.Amount;

            return response;
        }

        public static RestApi.Transactions.CostBaseAdjustment ToResponse(this Domain.Transactions.CostBaseAdjustment adjustment)
        {
            var response = new RestApi.Transactions.CostBaseAdjustment();

            PopulatePortfolioTransaction(response, adjustment);

            response.Percentage = adjustment.Percentage;

            return response;
        }

        public static RestApi.Transactions.Disposal ToResponse(this Domain.Transactions.Disposal disposal)
        {
            var response = new RestApi.Transactions.Disposal();

            PopulatePortfolioTransaction(response, disposal);

            response.Units = disposal.Units;
            response.AveragePrice = disposal.AveragePrice;
            response.TransactionCosts = disposal.TransactionCosts;
            response.CgtMethod = disposal.CgtMethod.ToResponse();
            response.CreateCashTransaction = disposal.CreateCashTransaction;

            return response;
        }

        public static RestApi.Transactions.IncomeReceived ToResponse(this Domain.Transactions.IncomeReceived income)
        {
            var response = new RestApi.Transactions.IncomeReceived();

            PopulatePortfolioTransaction(response, income);

            response.RecordDate = income.RecordDate;
            response.FrankedAmount = income.FrankedAmount;
            response.UnfrankedAmount = income.UnfrankedAmount;
            response.FrankingCredits = income.FrankingCredits;
            response.Interest = income.Interest;
            response.TaxDeferred = income.TaxDeferred;
            response.CreateCashTransaction = income.CreateCashTransaction;
            response.DrpCashBalance = income.DrpCashBalance;

            return response;
        }

        public static RestApi.Transactions.OpeningBalance ToResponse(this Domain.Transactions.OpeningBalance openingBalance)
        {
            var response = new RestApi.Transactions.OpeningBalance();

            PopulatePortfolioTransaction(response, openingBalance);

            response.Units = openingBalance.Units;
            response.CostBase = openingBalance.CostBase;
            response.AquisitionDate = openingBalance.AquisitionDate;

            return response;
        }

        public static RestApi.Transactions.ReturnOfCapital ToResponse(this Domain.Transactions.ReturnOfCapital returnOfCapital)
        {
            var response = new RestApi.Transactions.ReturnOfCapital();

            PopulatePortfolioTransaction(response, returnOfCapital);

            response.RecordDate = returnOfCapital.RecordDate;
            response.Amount = returnOfCapital.Amount;
            response.CreateCashTransaction = returnOfCapital.CreateCashTransaction;

            return response;
        }

        public static RestApi.Transactions.UnitCountAdjustment ToResponse(this Domain.Transactions.UnitCountAdjustment adjustment)
        {
            var response = new RestApi.Transactions.UnitCountAdjustment();

            PopulatePortfolioTransaction(response, adjustment);

            response.OriginalUnits = adjustment.OriginalUnits;
            response.NewUnits = adjustment.NewUnits;

            return response;
        }
        private static void PopulatePortfolioTransaction(RestApi.Transactions.Transaction response, Domain.Transactions.PortfolioTransaction transaction)
        {
            response.Id = transaction.Id;
            if (transaction.Stock != null)
                response.Stock = transaction.Stock.Id;
            else
                response.Stock = Guid.Empty;
            response.TransactionDate = transaction.Date;
            response.Comment = transaction.Comment;
            response.Description = transaction.Description;
        }

        public static TransactionsResponse.TransactionItem ToTransactionItem(this Domain.Transactions.IPortfolioTransaction transaction, Date date)
        {
            var transactionItem = new TransactionsResponse.TransactionItem();

            transactionItem.Id = transaction.Id;
            if (transaction.Stock != null)
                transactionItem.Stock = transaction.Stock.ToSummaryResponse(date);
            else
                transactionItem.Stock = null;
            transactionItem.TransactionDate = transaction.Date;
            transactionItem.Description = transaction.Description;
            transactionItem.Comment = transaction.Comment;

            return transactionItem;
        }
    }
}
