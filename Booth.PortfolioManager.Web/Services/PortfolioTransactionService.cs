using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.Common;
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
        private readonly IReadOnlyPortfolio _Portfolio;

        public PortfolioTransactionService(IReadOnlyPortfolio portfolio)
        {
            _Portfolio = portfolio;
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
            /*       var portfolio = _PortfolioCache.Get(portfolioId);

                   var stock = _StockQuery.Get(aquisition.Stock);
                   portfolio.AquireShares(aquisition.TransactionDate, stock, aquisition.Units, aquisition.AveragePrice, aquisition.TransactionCosts, aquisition.CreateCashTransaction, aquisition.Comment, aquisition.Id);

                   _PortfolioRepository.Update(portfolio); */
            throw new NotSupportedException();
        }

        private ServiceResult ApplyTransaction(RestApi.Transactions.Aquisition aquisition)
        {
            /*       var portfolio = _PortfolioCache.Get(portfolioId);

                   var stock = _StockQuery.Get(aquisition.Stock);
                   portfolio.AquireShares(aquisition.TransactionDate, stock, aquisition.Units, aquisition.AveragePrice, aquisition.TransactionCosts, aquisition.CreateCashTransaction, aquisition.Comment, aquisition.Id);

                   _PortfolioRepository.Update(portfolio); */
            throw new NotSupportedException();
        }

        private ServiceResult ApplyTransaction(RestApi.Transactions.CashTransaction cashTransaction)
        {
            /*var portfolio = _PortfolioCache.Get(portfolioId);

                        portfolio.MakeCashTransaction(cashTransaction.TransactionDate, PortfolioManager.RestApi.Transactions.RestApiNameMapping.ToBankAccountTransactionType(cashTransaction.CashTransactionType), cashTransaction.Amount, cashTransaction.Comment, cashTransaction.Id);

                        _PortfolioRepository.Update(portfolio); */
            throw new NotSupportedException();
        }

        private ServiceResult ApplyTransaction(RestApi.Transactions.CostBaseAdjustment costBaseAdjustment)
        {
            throw new NotSupportedException();
        }

        private ServiceResult ApplyTransaction(RestApi.Transactions.Disposal disposal)
        {
            /*  var portfolio = _PortfolioCache.Get(portfolioId);

              var stock = _StockQuery.Get(disposal.Stock);
              portfolio.DisposeOfShares(disposal.TransactionDate, stock, disposal.Units, disposal.AveragePrice, disposal.TransactionCosts, RestApi.Transactions.RestApiNameMapping.ToCGTCalculationMethod(disposal.CGTMethod), disposal.CreateCashTransaction, disposal.Comment, disposal.Id);

              _PortfolioRepository.Update(portfolio); */
            throw new NotSupportedException();
        }

        private ServiceResult ApplyTransaction(RestApi.Transactions.IncomeReceived incomeReceived)
        {
            /*     var portfolio = _PortfolioCache.Get(portfolioId);

                 var stock = _StockQuery.Get(incomeReceived.Stock);
                 portfolio.IncomeReceived(incomeReceived.RecordDate, incomeReceived.TransactionDate, stock, incomeReceived.FrankedAmount, incomeReceived.UnfrankedAmount, incomeReceived.FrankingCredits, incomeReceived.Interest, incomeReceived.TaxDeferred, incomeReceived.DRPCashBalance, incomeReceived.CreateCashTransaction, incomeReceived.Comment, incomeReceived.Id);

                 _PortfolioRepository.Update(portfolio); */

            throw new NotSupportedException();
        }

        private ServiceResult ApplyTransaction(RestApi.Transactions.OpeningBalance openingBalance)
        {
            /*      var portfolio = _PortfolioCache.Get(portfolioId);

                  var stock = _StockQuery.Get(openingBalance.Stock);
                  portfolio.AddOpeningBalance(openingBalance.TransactionDate, openingBalance.AquisitionDate, stock, openingBalance.Units, openingBalance.CostBase, openingBalance.Comment, openingBalance.Id);

                  _PortfolioRepository.Update(portfolio); */

            throw new NotSupportedException();
        }

        private ServiceResult ApplyTransaction(RestApi.Transactions.ReturnOfCapital returnOfCapital)
        {
            /*   var portfolio = _PortfolioCache.Get(portfolioId);

               var stock = _StockQuery.Get(returnOfCapital.Stock);
               portfolio.ReturnOfCapitalReceived(returnOfCapital.TransactionDate, returnOfCapital.RecordDate, stock, returnOfCapital.Amount, returnOfCapital.CreateCashTransaction, returnOfCapital.Comment, returnOfCapital.Id);

               _PortfolioRepository.Update(portfolio); */
            throw new NotSupportedException();
        }

        private ServiceResult ApplyTransaction(RestApi.Transactions.UnitCountAdjustment unitCountAdjustment)
        {
            throw new NotSupportedException();
        }
    } 
}
