using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Web.Models.Portfolio;
using Booth.PortfolioManager.Web.Mappers;

namespace Booth.PortfolioManager.Web.Services
{

    public interface ICashAccountService
    {
        ServiceResult<CashAccountTransactionsResponse> GetTransactions(DateRange dateRange);
    }

    public class CashAccountService : ICashAccountService
    {
        private readonly IReadOnlyPortfolio _Portfolio;

        public CashAccountService(IReadOnlyPortfolio portfolio)
        {
            _Portfolio = portfolio;
        }

        public ServiceResult<CashAccountTransactionsResponse> GetTransactions(DateRange dateRange)
        {
            if (_Portfolio == null)
                return ServiceResult<CashAccountTransactionsResponse>.NotFound();

            var response = new CashAccountTransactionsResponse();
            response.OpeningBalance = _Portfolio.CashAccount.Balance(dateRange.FromDate);
            response.ClosingBalance = _Portfolio.CashAccount.Balance(dateRange.ToDate);

            var transactions = _Portfolio.CashAccount.Transactions.InDateRange(dateRange);
            foreach (var transaction in transactions)
            {
                response.Transactions.Add(new CashAccountTransactionsResponse.Transaction()
                {
                    Date = transaction.Date,
                    Type = transaction.Type.ToResponse(),
                    Description = transaction.Description,
                    Amount = transaction.Amount,
                    Balance = transaction.Balance
                });
            }

            return ServiceResult<CashAccountTransactionsResponse>.Ok(response); 
        } 
    } 
}
