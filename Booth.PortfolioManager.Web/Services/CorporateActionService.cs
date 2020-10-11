using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.Common;
using Booth.EventStore;
using Booth.PortfolioManager.Domain.Portfolios;
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
        private readonly IRepository<Stock> _Repository;

        public CorporateActionService(IStockQuery stockQuery, IRepository<Stock> repository)
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
            
            var result = corporateAction.ToResponse();

            return ServiceResult<CorporateAction>.Ok(result);  
        }

        public ServiceResult<List<CorporateAction>> GetCorporateActions(Guid stockId, DateRange dateRange)
        {
            var stock = _StockQuery.Get(stockId);
            if (stock == null)
                return ServiceResult<List<CorporateAction>>.NotFound();

            var corporateActions = stock.CorporateActions.InDateRange(dateRange);

            var result = corporateActions.Select(x => x.ToResponse()).ToList();

            return ServiceResult<List<CorporateAction>>.Ok(result);
        }

        public ServiceResult AddCorporateAction(Guid stockId, CorporateAction corporateAction)
        {
            var stock = _StockQuery.Get(stockId);
            if (stock == null)
                return ServiceResult<List<CorporateAction>>.NotFound();

            ServiceResult result;
            if (corporateAction is RestApi.CorporateActions.CapitalReturn capitalReturn)
                stock.CorporateActions.AddCapitalReturn(capitalReturn.Id, capitalReturn.ActionDate, capitalReturn.Description, capitalReturn.PaymentDate, capitalReturn.Amount);
            else if (corporateAction is RestApi.CorporateActions.CompositeAction compositeAction)
                AddCompositeAction(stock, compositeAction);
            else if (corporateAction is RestApi.CorporateActions.Dividend dividend)
                stock.CorporateActions.AddDividend(dividend.Id, dividend.ActionDate, dividend.Description, dividend.PaymentDate, dividend.Amount, dividend.PercentFranked, dividend.DrpPrice);
            else if (corporateAction is RestApi.CorporateActions.SplitConsolidation splitConsolidation)
                stock.CorporateActions.AddSplitConsolidation(splitConsolidation.Id, splitConsolidation.ActionDate, splitConsolidation.Description, splitConsolidation.OriginalUnits, splitConsolidation.NewUnits);
            else if (corporateAction is RestApi.CorporateActions.Transformation transformation)
            {
                var resultingStocks = transformation.ResultingStocks.Select(x => new Domain.CorporateActions.Transformation.ResultingStock(x.Stock, x.OriginalUnits, x.NewUnits, x.CostBase, x.AquisitionDate));
                stock.CorporateActions.AddTransformation(transformation.Id, transformation.ActionDate, transformation.Description, transformation.ImplementationDate, transformation.CashComponent, transformation.RolloverRefliefApplies, resultingStocks);
            }
            else
                result = ServiceResult.Error("Unkown Corporate Action type");

            _Repository.Update(stock);

            return ServiceResult.Ok();
        }

        private void AddCompositeAction(Stock stock, RestApi.CorporateActions.CompositeAction corporateAction)
        {
            var builder = stock.CorporateActions.StartCompositeAction(corporateAction.Id, corporateAction.ActionDate, corporateAction.Description);           
            foreach (var childAction in corporateAction.ChildActions)
            {
                if (childAction is RestApi.CorporateActions.CapitalReturn capitalReturn)
                    builder.AddCapitalReturn(capitalReturn.Description, capitalReturn.PaymentDate, capitalReturn.Amount);
                else if (childAction is RestApi.CorporateActions.Dividend dividend)
                    builder.AddDividend(dividend.Description, dividend.PaymentDate, dividend.Amount, dividend.PercentFranked, dividend.DrpPrice);
                else if (childAction is RestApi.CorporateActions.SplitConsolidation splitConsolidation)
                    builder.AddSplitConsolidation(splitConsolidation.Description, splitConsolidation.OriginalUnits, splitConsolidation.NewUnits);
                else if (childAction is RestApi.CorporateActions.Transformation transformation)
                {
                    var resultingStocks = transformation.ResultingStocks.Select(x => new Domain.CorporateActions.Transformation.ResultingStock(x.Stock, x.OriginalUnits, x.NewUnits, x.CostBase, x.AquisitionDate));
                    builder.AddTransformation(transformation.Description, transformation.ImplementationDate, transformation.CashComponent, transformation.RolloverRefliefApplies, resultingStocks);
                }
            }
            builder.Finish();
        }

    } 
}
