using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.RestApi.Portfolios;
using Booth.PortfolioManager.RestApi.Transactions;

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
            /*  var portfolio = _PortfolioCache.Get(portfolioId);

              var transaction = portfolio.Transactions[id];
              if (transaction == null)
                  throw new TransactionNotFoundException(id);

              return _Mapper.Map<RestApi.Transactions.Transaction>(transaction); */
            throw new NotSupportedException();
        }

        public ServiceResult<TransactionsResponse> GetTransactions(DateRange dateRange)
        {
            /*      var portfolio = _PortfolioCache.Get(portfolioId);

                  return GetTransactions(portfolio.Transactions.InDateRange(dateRange), dateRange.ToDate); */
            throw new NotSupportedException();
        }

        public ServiceResult<TransactionsResponse> GetTransactions(Guid stockId, DateRange dateRange)
        {
            /*      var portfolio = _PortfolioCache.Get(portfolioId);

                  var holding = portfolio.Holdings.Get(stockId);
                  if (holding == null)
                      throw new HoldingNotFoundException(stockId);

                  return GetTransactions(portfolio.Transactions.ForHolding(holding.Stock.Id, dateRange), dateRange.ToDate); */
            throw new NotSupportedException();
        }

        /*    private RestApi.Portfolios.TransactionsResponse GetTransactions(IEnumerable<Transaction> transactions, DateTime date)
            {
                var response = new RestApi.Portfolios.TransactionsResponse();

                foreach (var transaction in transactions)
                {
                    var t = _Mapper.Map<RestApi.Portfolios.TransactionsResponse.TransactionItem>(transaction, opts => opts.Items["date"] = date);
                    response.Transactions.Add(t);
                }

                return response;
            } */

        public ServiceResult ApplyTransaction(Transaction transaction)
        {
            /*       var portfolio = _PortfolioCache.Get(portfolioId);

                   var stock = _StockQuery.Get(aquisition.Stock);
                   portfolio.AquireShares(aquisition.TransactionDate, stock, aquisition.Units, aquisition.AveragePrice, aquisition.TransactionCosts, aquisition.CreateCashTransaction, aquisition.Comment, aquisition.Id);

                   _PortfolioRepository.Update(portfolio); */
            throw new NotSupportedException();
        }

        private ServiceResult ApplyTransaction(Aquisition aquisition)
        {
            /*       var portfolio = _PortfolioCache.Get(portfolioId);

                   var stock = _StockQuery.Get(aquisition.Stock);
                   portfolio.AquireShares(aquisition.TransactionDate, stock, aquisition.Units, aquisition.AveragePrice, aquisition.TransactionCosts, aquisition.CreateCashTransaction, aquisition.Comment, aquisition.Id);

                   _PortfolioRepository.Update(portfolio); */
            throw new NotSupportedException();
        }

        private ServiceResult ApplyTransaction(CashTransaction cashTransaction)
        {
            /*var portfolio = _PortfolioCache.Get(portfolioId);

                        portfolio.MakeCashTransaction(cashTransaction.TransactionDate, PortfolioManager.RestApi.Transactions.RestApiNameMapping.ToBankAccountTransactionType(cashTransaction.CashTransactionType), cashTransaction.Amount, cashTransaction.Comment, cashTransaction.Id);

                        _PortfolioRepository.Update(portfolio); */
            throw new NotSupportedException();
        }

        private ServiceResult ApplyTransaction(CostBaseAdjustment costBaseAdjustment)
        {
            throw new NotSupportedException();
        }

        private ServiceResult ApplyTransaction(Disposal disposal)
        {
            /*  var portfolio = _PortfolioCache.Get(portfolioId);

              var stock = _StockQuery.Get(disposal.Stock);
              portfolio.DisposeOfShares(disposal.TransactionDate, stock, disposal.Units, disposal.AveragePrice, disposal.TransactionCosts, RestApi.Transactions.RestApiNameMapping.ToCGTCalculationMethod(disposal.CGTMethod), disposal.CreateCashTransaction, disposal.Comment, disposal.Id);

              _PortfolioRepository.Update(portfolio); */
            throw new NotSupportedException();
        }

        private ServiceResult ApplyTransaction(IncomeReceived incomeReceived)
        {
            /*     var portfolio = _PortfolioCache.Get(portfolioId);

                 var stock = _StockQuery.Get(incomeReceived.Stock);
                 portfolio.IncomeReceived(incomeReceived.RecordDate, incomeReceived.TransactionDate, stock, incomeReceived.FrankedAmount, incomeReceived.UnfrankedAmount, incomeReceived.FrankingCredits, incomeReceived.Interest, incomeReceived.TaxDeferred, incomeReceived.DRPCashBalance, incomeReceived.CreateCashTransaction, incomeReceived.Comment, incomeReceived.Id);

                 _PortfolioRepository.Update(portfolio); */

            throw new NotSupportedException();
        }

        private ServiceResult ApplyTransaction(OpeningBalance openingBalance)
        {
            /*      var portfolio = _PortfolioCache.Get(portfolioId);

                  var stock = _StockQuery.Get(openingBalance.Stock);
                  portfolio.AddOpeningBalance(openingBalance.TransactionDate, openingBalance.AquisitionDate, stock, openingBalance.Units, openingBalance.CostBase, openingBalance.Comment, openingBalance.Id);

                  _PortfolioRepository.Update(portfolio); */

            throw new NotSupportedException();
        }

        private ServiceResult ApplyTransaction(ReturnOfCapital returnOfCapital)
        {
            /*   var portfolio = _PortfolioCache.Get(portfolioId);

               var stock = _StockQuery.Get(returnOfCapital.Stock);
               portfolio.ReturnOfCapitalReceived(returnOfCapital.TransactionDate, returnOfCapital.RecordDate, stock, returnOfCapital.Amount, returnOfCapital.CreateCashTransaction, returnOfCapital.Comment, returnOfCapital.Id);

               _PortfolioRepository.Update(portfolio); */
            throw new NotSupportedException();
        }

        private ServiceResult ApplyTransaction(UnitCountAdjustment unitCountAdjustment)
        {
            throw new NotSupportedException();
        }
    } 
}
