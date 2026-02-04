using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Web.Models.CorporateAction;
using Booth.PortfolioManager.Web.Mappers;
using Booth.PortfolioManager.Web.Utilities;

namespace Booth.PortfolioManager.Web.Services
{
    public interface ICorporateActionService
    {
        ServiceResult<CorporateAction> GetCorporateAction(Guid stockId, Guid id);
        ServiceResult<List<CorporateAction>> GetCorporateActions(Guid stockId, DateRange dateRange);
        Task<ServiceResult> AddCorporateActionAsync(Guid stockId, CorporateAction corporateAction);
        Task<ServiceResult> UpdateCorporateActionAsync(Guid stockId, CorporateAction corporateAction);
        Task<ServiceResult> DeleteCorporateActionAsync(Guid stockId, Guid corporateActionId);
    }

    public class CorporateActionService : ICorporateActionService
    {
        private readonly IStockQuery _StockQuery;
        private readonly IStockRepository _Repository;
        private readonly ICorporateActionMapper _Mapper;

        public CorporateActionService(IStockQuery stockQuery, IStockRepository repository, ICorporateActionMapper mapper)
        {
            _StockQuery = stockQuery;
            _Repository = repository;
            _Mapper = mapper;
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
            
            var result = _Mapper.ToApi(corporateAction);

            return ServiceResult<CorporateAction>.Ok(result);  
        }

        public ServiceResult<List<CorporateAction>> GetCorporateActions(Guid stockId, DateRange dateRange)
        {
            var stock = _StockQuery.Get(stockId);
            if (stock == null)
                return ServiceResult<List<CorporateAction>>.NotFound();
            
            var corporateActions = stock.CorporateActions.InDateRange(dateRange);

            var result = corporateActions.Select(x => _Mapper.ToApi(x)).ToList();

            return ServiceResult<List<CorporateAction>>.Ok(result);
        }

        public async Task<ServiceResult> AddCorporateActionAsync(Guid stockId, CorporateAction corporateAction)
        {
            var stock = _StockQuery.Get(stockId);
            if (stock == null)
                return ServiceResult.NotFound();

            stock.CorporateActions.Add(_Mapper.FromApi(corporateAction));
            await _Repository.AddCorporateActionAsync(stock, corporateAction.Id);

            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> UpdateCorporateActionAsync(Guid stockId, CorporateAction corporateAction)
        {
            var stock = _StockQuery.Get(stockId);
            if (stock == null)
                return ServiceResult.NotFound();

            if (!stock.CorporateActions.Contains(corporateAction.Id))
                return ServiceResult.NotFound();

            stock.CorporateActions.Update(_Mapper.FromApi(corporateAction));

            await _Repository.UpdateCorporateActionAsync(stock, corporateAction.Id);

            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> DeleteCorporateActionAsync(Guid stockId, Guid corporateActionId)
        {
            var stock = _StockQuery.Get(stockId);
            if (stock == null)
                return ServiceResult.NotFound();

            if (!stock.CorporateActions.Contains(corporateActionId))
                return ServiceResult.NotFound();
           
            stock.CorporateActions.Remove(corporateActionId);
            await _Repository.DeleteCorporateActionAsync(stock, corporateActionId);

            return ServiceResult.Ok();
        }

    } 
}
