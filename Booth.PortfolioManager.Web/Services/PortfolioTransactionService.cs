using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.RestApi.Portfolios;
using Booth.PortfolioManager.RestApi.Transactions;
using Booth.PortfolioManager.Web.Mappers;

namespace Booth.PortfolioManager.Web.Services
{

    public interface IPortfolioTransactionService
    {
        ServiceResult<Transaction> GetTransaction(Guid id);

        ServiceResult<TransactionsResponse> GetTransactions(DateRange dateRange);
        ServiceResult<TransactionsResponse> GetTransactions(Guid stockId, DateRange dateRange);

        ServiceResult ApplyTransaction(Transaction transaction);
    }
    
    public class PortfolioTransactionService : IPortfolioTransactionService
    {
        private readonly IPortfolio _Portfolio;
        private readonly IPortfolioRepository _Repository;

        public PortfolioTransactionService(IPortfolio portfolio, IPortfolioRepository repository)
        {
            _Portfolio = portfolio;
            _Repository = repository;
        }

        public ServiceResult<Transaction> GetTransaction(Guid id)
        {
            if (_Portfolio == null)
                return ServiceResult<Transaction>.NotFound();

            IPortfolioTransaction transaction;
            try 
            {
                transaction = _Portfolio.Transactions[id];
            }
            catch
            {
                return ServiceResult<Transaction>.NotFound();
            }

            var response = transaction.ToResponse();

            return ServiceResult<Transaction>.Ok(response);
        }

        public ServiceResult<TransactionsResponse> GetTransactions(DateRange dateRange)
        {
            if (_Portfolio == null)
                return ServiceResult<TransactionsResponse>.NotFound();

            var transactions = _Portfolio.Transactions.InDateRange(dateRange);


            var response = new TransactionsResponse();

            response.Transactions.AddRange(transactions.Select(x => x.ToTransactionItem(dateRange.ToDate)));
                 
            return ServiceResult<TransactionsResponse>.Ok(response);
        }

        public ServiceResult<TransactionsResponse> GetTransactions(Guid stockId, DateRange dateRange)
        {
            if (_Portfolio == null)
                return ServiceResult<TransactionsResponse>.NotFound();

            var transactions = _Portfolio.Transactions.ForHolding(stockId, dateRange);

            var response = new TransactionsResponse();

            response.Transactions.AddRange(transactions.Select(x => x.ToTransactionItem(dateRange.ToDate)));

            return ServiceResult<TransactionsResponse>.Ok(response);
        }

        public ServiceResult ApplyTransaction(Transaction transaction)
        {
            if (_Portfolio == null)
                return ServiceResult<TransactionsResponse>.NotFound();

            ServiceResult result;
            if (transaction is RestApi.Transactions.Aquisition aquisition)
                _Portfolio.AquireShares(aquisition.Stock, aquisition.TransactionDate, aquisition.Units, aquisition.AveragePrice, aquisition.TransactionCosts, aquisition.CreateCashTransaction, aquisition.Comment, aquisition.Id);
            else if (transaction is RestApi.Transactions.CashTransaction cashTransaction)
                _Portfolio.MakeCashTransaction(cashTransaction.TransactionDate, cashTransaction.CashTransactionType.ToDomain(), cashTransaction.Amount, cashTransaction.Comment, cashTransaction.Id);
            else if (transaction is RestApi.Transactions.CostBaseAdjustment costBaseAdjustment)
                _Portfolio.AdjustCostBase(costBaseAdjustment.Stock, costBaseAdjustment.TransactionDate, costBaseAdjustment.Percentage, costBaseAdjustment.Comment, costBaseAdjustment.Id);
            else if (transaction is RestApi.Transactions.Disposal disposal)
                _Portfolio.DisposeOfShares(disposal.Stock, disposal.TransactionDate, disposal.Units, disposal.AveragePrice, disposal.TransactionCosts, disposal.CgtMethod.ToDomain(), disposal.CreateCashTransaction, disposal.Comment, disposal.Id);
            else if (transaction is RestApi.Transactions.IncomeReceived income)
                _Portfolio.IncomeReceived(income.Stock, income.RecordDate, income.TransactionDate, income.FrankedAmount, income.UnfrankedAmount, income.FrankingCredits, income.Interest, income.TaxDeferred, income.DrpCashBalance, income.CreateCashTransaction, income.Comment, income.Id);
            else if (transaction is RestApi.Transactions.OpeningBalance openingBalance)
                _Portfolio.AddOpeningBalance(openingBalance.Stock, openingBalance.TransactionDate, openingBalance.AquisitionDate, openingBalance.Units, openingBalance.CostBase, openingBalance.Comment, openingBalance.Id);
            else if (transaction is RestApi.Transactions.ReturnOfCapital returnOfCapital)
                _Portfolio.ReturnOfCapitalReceived(returnOfCapital.Stock, returnOfCapital.TransactionDate, returnOfCapital.RecordDate, returnOfCapital.Amount, returnOfCapital.CreateCashTransaction, returnOfCapital.Comment, returnOfCapital.Id);
            else if (transaction is RestApi.Transactions.UnitCountAdjustment unitCountAdjustment)
                _Portfolio.AdjustUnitCount(unitCountAdjustment.Stock, unitCountAdjustment.TransactionDate, unitCountAdjustment.OriginalUnits, unitCountAdjustment.NewUnits, unitCountAdjustment.Comment, unitCountAdjustment.Id);
            else
                result = ServiceResult.Error("Unkown Transaction type");

            _Repository.AddTransaction((Portfolio)_Portfolio, transaction.Id);

            return ServiceResult.Ok();
        }

    } 
}
