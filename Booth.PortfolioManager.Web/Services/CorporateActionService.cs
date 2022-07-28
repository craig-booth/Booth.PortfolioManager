using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.RestApi.CorporateActions;
using Booth.PortfolioManager.Web.Mappers;
using Booth.PortfolioManager.Web.Utilities;

namespace Booth.PortfolioManager.Web.Services
{
    public interface ICorporateActionService
    {
        ServiceResult<CorporateAction> GetCorporateAction(Guid stockId, Guid id);
        ServiceResult<List<CorporateAction>> GetCorporateActions(Guid stockId, DateRange dateRange);
        ServiceResult AddCorporateAction(Guid stockId, CorporateAction corporateAction);
    }

    public class CorporateActionService : ICorporateActionService
    {
        private readonly IStockQuery _StockQuery;
        private readonly IStockRepository _Repository;

        public CorporateActionService(IStockQuery stockQuery, IStockRepository repository)
        {
            _StockQuery = stockQuery;
            _Repository = repository;
        }

        public ServiceResult<CorporateAction> GetCorporateAction(Guid stockId, Guid id)
        {
            var stock = _StockQuery.Get(stockId);
            if (stock == null)
                return ServiceResult<CorporateAction>.NotFound();

            Domain.CorporateActions.CorporateAction corporateAction;
            try
            {
                corporateAction = stock.CorporateActions[id];
            }
            catch
            {
                return ServiceResult<CorporateAction>.NotFound();
            }
            
            var result = corporateAction.ToApi();

            return ServiceResult<CorporateAction>.Ok(result);  
        }

        public ServiceResult<List<CorporateAction>> GetCorporateActions(Guid stockId, DateRange dateRange)
        {
            var stock = _StockQuery.Get(stockId);
            if (stock == null)
                return ServiceResult<List<CorporateAction>>.NotFound();

            var corporateActions = stock.CorporateActions.InDateRange(dateRange);

            var result = corporateActions.Select(x => x.ToApi()).ToList();

            return ServiceResult<List<CorporateAction>>.Ok(result);
        }

        public ServiceResult AddCorporateAction(Guid stockId, CorporateAction corporateAction)
        {
            var stock = _StockQuery.Get(stockId);
            if (stock == null)
                return ServiceResult<List<CorporateAction>>.NotFound();

            stock.CorporateActions.Add(corporateAction.FromApi());
            _Repository.AddCorporateAction(stock, corporateAction.Id);

            return ServiceResult.Ok();
        }

    } 
}
