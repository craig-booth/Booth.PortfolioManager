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
    public interface IPortfolioCorporateActionsService
    {
        ServiceResult<CorporateActionsResponse> GetCorporateActions();
        ServiceResult<CorporateActionsResponse> GetCorporateActions(Guid stockId);
        ServiceResult<List<Transaction>> GetTransactionsForCorporateAction(Guid stockId, Guid actionId);
    }

    
    public class PortfolioCorporateActionsService : IPortfolioCorporateActionsService
    {

        private readonly IReadOnlyPortfolio _Portfolio;

        public PortfolioCorporateActionsService(IReadOnlyPortfolio portfolio)
        {
            _Portfolio = portfolio;
        }

        public ServiceResult<CorporateActionsResponse> GetCorporateActions()
        {/*
            var portfolio = _PortfolioCache.Get(portfolioId);

            return GetCorporateActions(portfolio, portfolio.Holdings.All(DateTime.Today)); */
            throw new NotSupportedException();
        }

        public ServiceResult<CorporateActionsResponse> GetCorporateActions(Guid stockId)
        {
            /*     var portfolio = _PortfolioCache.Get(portfolioId);

                 var holding = portfolio.Holdings.Get(stockId);
                 if (holding == null)
                     throw new HoldingNotFoundException(stockId);

                 return GetCorporateActions(portfolio, new[] { holding }); */
            throw new NotSupportedException();
        }

     /*   private CorporateActionsResponse GetCorporateActions(Portfolio portfolio, IEnumerable<Domain.Portfolios.Holding> holdings)
        {
            var response = new CorporateActionsResponse();

            foreach (var holding in holdings)
            {
                foreach (var corporateAction in holding.Stock.CorporateActions.InDateRange(holding.EffectivePeriod))
                {
                    if (! corporateAction.HasBeenApplied(portfolio.Transactions))
                    {
                        response.CorporateActions.Add(new CorporateActionsResponse.CorporateActionItem()
                        {
                            Id = corporateAction.Id,
                            ActionDate = corporateAction.Date,
                            Stock = corporateAction.Stock.Convert(corporateAction.Date),
                            Description = corporateAction.Description
                        });
                    }
                }
            }

            return response; 
        } */

        public ServiceResult<List<Transaction>> GetTransactionsForCorporateAction(Guid stockId, Guid actionId)
        {
            /*     var portfolio = _PortfolioCache.Get(portfolioId);

                 var holding = portfolio.Holdings.Get(stockId);
                 if (holding == null)
                     throw new HoldingNotFoundException(stockId);

                 var corporateAction = holding.Stock.CorporateActions[actionId];
                 if (corporateAction == null)
                     throw new CorporateActionNotFoundException(actionId);

                 var transactions = corporateAction.GetTransactionList(holding);

                 return _Mapper.Map<IEnumerable<RestApi.Transactions.Transaction>>(transactions); */
            throw new NotSupportedException();
        }

    } 
}
