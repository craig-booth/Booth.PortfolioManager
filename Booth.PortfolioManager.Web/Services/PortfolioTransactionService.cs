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

        ServiceResult AddTransaction(Transaction transaction);
        ServiceResult UpdateTransaction(Guid id, Transaction transaction);
        ServiceResult DeleteTransaction(Guid id);
    }
    
    public class PortfolioTransactionService : IPortfolioTransactionService
    {
        private readonly IPortfolio _Portfolio;
        private readonly IPortfolioRepository _Repository;
        private readonly ITransactionMapper _Mapper;

        public PortfolioTransactionService(IPortfolio portfolio, IPortfolioRepository repository, ITransactionMapper mapper)
        {
            _Portfolio = portfolio;
            _Repository = repository;
            _Mapper = mapper;
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

            var response = _Mapper.ToApi(transaction);

            return ServiceResult<Transaction>.Ok(response);
        }

        public ServiceResult<TransactionsResponse> GetTransactions(DateRange dateRange)
        {
            if (_Portfolio == null)
                return ServiceResult<TransactionsResponse>.NotFound();

            var transactions = _Portfolio.Transactions.InDateRange(dateRange);


            var response = new TransactionsResponse();

            response.Transactions.AddRange(transactions.Select(x => _Mapper.ToTransactionItem(x, dateRange.ToDate)));
                 
            return ServiceResult<TransactionsResponse>.Ok(response);
        }

        public ServiceResult<TransactionsResponse> GetTransactions(Guid stockId, DateRange dateRange)
        {
            if (_Portfolio == null)
                return ServiceResult<TransactionsResponse>.NotFound();

            var transactions = _Portfolio.Transactions.ForHolding(stockId, dateRange);

            var response = new TransactionsResponse();

            response.Transactions.AddRange(transactions.Select(x => _Mapper.ToTransactionItem(x, dateRange.ToDate)));

            return ServiceResult<TransactionsResponse>.Ok(response);
        }

        public ServiceResult AddTransaction(Transaction transaction)
        {
            if (_Portfolio == null)
                return ServiceResult<TransactionsResponse>.NotFound();

            var newTransaction = _Mapper.FromApi(transaction);
            _Portfolio.AddTransaction(newTransaction);

            _Repository.AddTransaction((Portfolio)_Portfolio, transaction.Id);

            return ServiceResult.Ok();
        }

        public ServiceResult UpdateTransaction(Guid id, Transaction transaction)
        {
            if (_Portfolio == null)
                return ServiceResult<TransactionsResponse>.NotFound();

            if (!_Portfolio.Transactions.Contains(transaction.Id))
                return ServiceResult.NotFound();

            var updatedTransaction = _Mapper.FromApi(transaction);
            _Portfolio.UpdateTransaction(updatedTransaction);

            _Repository.UpdateTransaction((Portfolio)_Portfolio, transaction.Id);

            return ServiceResult.Ok();
        }

        public ServiceResult DeleteTransaction(Guid id)
        {
            if (_Portfolio == null)
                return ServiceResult<TransactionsResponse>.NotFound();

            if (!_Portfolio.Transactions.Contains(id))
                return ServiceResult.NotFound();

            _Portfolio.DeleteTransaction(id);

            _Repository.DeleteTransaction((Portfolio)_Portfolio, id);

            return ServiceResult.Ok();
        }

    } 
}
