using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.RestApi.Portfolios;
using Booth.PortfolioManager.RestApi.Transactions;
using Booth.PortfolioManager.Web.Mappers;

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
        private readonly IStockResolver _StockResolver;

        public PortfolioCorporateActionsService(IReadOnlyPortfolio portfolio, IStockResolver stockResolver)
        {
            _Portfolio = portfolio;
            _StockResolver = stockResolver;
        }

        public ServiceResult<CorporateActionsResponse> GetCorporateActions()
        {
            if (_Portfolio == null)
                return ServiceResult<CorporateActionsResponse>.NotFound();

            var response = new CorporateActionsResponse();

            foreach (var holding in _Portfolio.Holdings.All(Date.Today))
            {
                foreach (var corporateAction in holding.Stock.CorporateActions.InDateRange(holding.EffectivePeriod))
                {
                    if (!corporateAction.HasBeenApplied(_Portfolio.Transactions))
                    {
                        response.CorporateActions.Add(new CorporateActionsResponse.CorporateActionItem()
                        {
                            Id = corporateAction.Id,
                            ActionDate = corporateAction.Date,
                            Stock = corporateAction.Stock.ToSummaryResponse(corporateAction.Date),
                            Description = corporateAction.Description
                        });
                    }
                }
            }

            return ServiceResult<CorporateActionsResponse>.Ok(response);
        }

        public ServiceResult<CorporateActionsResponse> GetCorporateActions(Guid stockId)
        {
            if (_Portfolio == null)
                return ServiceResult<CorporateActionsResponse>.NotFound();

            var holding = _Portfolio.Holdings[stockId];
            if (holding == null)
                return ServiceResult<CorporateActionsResponse>.NotFound();

            var response = new CorporateActionsResponse();

            foreach (var corporateAction in holding.Stock.CorporateActions.InDateRange(holding.EffectivePeriod))
            {
                if (!corporateAction.HasBeenApplied(_Portfolio.Transactions))
                {
                    response.CorporateActions.Add(new CorporateActionsResponse.CorporateActionItem()
                    {
                        Id = corporateAction.Id,
                        ActionDate = corporateAction.Date,
                        Stock = corporateAction.Stock.ToSummaryResponse(corporateAction.Date),
                        Description = corporateAction.Description
                    });
                }
            }

            return ServiceResult<CorporateActionsResponse>.Ok(response);
        }

        public ServiceResult<List<Transaction>> GetTransactionsForCorporateAction(Guid stockId, Guid actionId)
        {
            if (_Portfolio == null)
                return ServiceResult<List<Transaction>>.NotFound();

            var holding = _Portfolio.Holdings[stockId];
            if (holding == null)
                return ServiceResult<List<Transaction>>.NotFound();

            var corporateAction = holding.Stock.CorporateActions[actionId];
            if (corporateAction == null)
                return ServiceResult<List<Transaction>>.NotFound();

            var transactions = corporateAction.GetTransactionList(holding, _StockResolver);

            var result = transactions.Select(x => x.ToResponse()).ToList();

            return ServiceResult<List<Transaction>>.Ok(result);
        }

    } 
}
