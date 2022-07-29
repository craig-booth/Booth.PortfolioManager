using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Booth.PortfolioManager.Domain.CorporateActions;
using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Web.Mappers
{

    public interface ICorporateActionMapper
    {
        RestApi.CorporateActions.CorporateAction ToApi(ICorporateAction action);
        Domain.CorporateActions.CorporateAction FromApi(RestApi.CorporateActions.CorporateAction action);
        RestApi.CorporateActions.CapitalReturn ToApi(Domain.CorporateActions.CapitalReturn action);
        Domain.CorporateActions.CapitalReturn FromApi(RestApi.CorporateActions.CapitalReturn action);
        RestApi.CorporateActions.CompositeAction ToApi(Domain.CorporateActions.CompositeAction action);
        Domain.CorporateActions.CompositeAction FromApi(RestApi.CorporateActions.CompositeAction action);
        RestApi.CorporateActions.Dividend ToApi(Domain.CorporateActions.Dividend action);
        Domain.CorporateActions.Dividend FromApi(RestApi.CorporateActions.Dividend action);
        RestApi.CorporateActions.SplitConsolidation ToApi(Domain.CorporateActions.SplitConsolidation action);
        Domain.CorporateActions.SplitConsolidation FromApi(RestApi.CorporateActions.SplitConsolidation action);
        RestApi.CorporateActions.Transformation ToApi(Domain.CorporateActions.Transformation action);
        Domain.CorporateActions.Transformation FromApi(RestApi.CorporateActions.Transformation action);
    }

    class CorporateActionMapper : ICorporateActionMapper
    {
        private readonly IStockResolver _StockResolver;

        public CorporateActionMapper(IStockResolver stockResover)
        {
            _StockResolver = stockResover;  
        }

        public RestApi.CorporateActions.CorporateAction ToApi(ICorporateAction action)
        {
            if (action is Domain.CorporateActions.CapitalReturn capitalReturn)
                return ToApi(capitalReturn);
            else if (action is Domain.CorporateActions.CompositeAction compositeAction)
                return ToApi(compositeAction);
            else if (action is Domain.CorporateActions.Dividend dividend)
                return ToApi(dividend);
            else if (action is Domain.CorporateActions.SplitConsolidation splitConsolidation)
                return ToApi(splitConsolidation);
            else if (action is Domain.CorporateActions.Transformation transformation)
                return ToApi(transformation);
            else
                throw new NotSupportedException();
        }

        public Domain.CorporateActions.CorporateAction FromApi(RestApi.CorporateActions.CorporateAction action)
        {
            if (action is RestApi.CorporateActions.CapitalReturn capitalReturn)
                return FromApi(capitalReturn);
            else if (action is RestApi.CorporateActions.CompositeAction compositeAction)
                return FromApi(compositeAction);
            else if (action is RestApi.CorporateActions.Dividend dividend)
                return FromApi(dividend);
            else if (action is RestApi.CorporateActions.SplitConsolidation splitConsolidation)
                return FromApi(splitConsolidation);
            else if (action is RestApi.CorporateActions.Transformation transformation)
                return FromApi(transformation);
            else
                throw new NotSupportedException();
        }


        public  RestApi.CorporateActions.CapitalReturn ToApi(Domain.CorporateActions.CapitalReturn action)
        {
            var response = new RestApi.CorporateActions.CapitalReturn()
            {
                Id = action.Id,
                Stock = action.Stock.Id,
                ActionDate = action.Date,
                Description = action.Description,
                PaymentDate = action.PaymentDate,
                Amount = action.Amount
            };

            return response;
        }

        public Domain.CorporateActions.CapitalReturn FromApi(RestApi.CorporateActions.CapitalReturn action)
        {
            var stock = _StockResolver.GetStock(action.Stock);
            return new Domain.CorporateActions.CapitalReturn(action.Id, stock, action.ActionDate, action.Description, action.PaymentDate, action.Amount);
        }

        public RestApi.CorporateActions.CompositeAction ToApi(Domain.CorporateActions.CompositeAction action)
        {
            var response = new RestApi.CorporateActions.CompositeAction()
            {
                Id = action.Id,
                Stock = action.Stock.Id,
                ActionDate = action.Date,
                Description = action.Description
            };

            var childActions = action.ChildActions.Select(x => ToApi(x));
            response.ChildActions.AddRange(childActions);

            return response;
        }

        public Domain.CorporateActions.CompositeAction FromApi(RestApi.CorporateActions.CompositeAction action)
        {
            var stock = _StockResolver.GetStock(action.Stock);
            return new Domain.CorporateActions.CompositeAction(action.Id, stock, action.ActionDate, action.Description, action.ChildActions.Select(x => FromApi(x)));
        }

        public RestApi.CorporateActions.Dividend ToApi(Domain.CorporateActions.Dividend action)
        {
            var response = new RestApi.CorporateActions.Dividend()
            {
                Id = action.Id,
                Stock = action.Stock.Id,
                ActionDate = action.Date,
                Description = action.Description,
                PaymentDate = action.PaymentDate,
                Amount = action.DividendAmount,
                PercentFranked = action.PercentFranked,
                DrpPrice = action.DrpPrice
            };


            return response;
        }

        public Domain.CorporateActions.Dividend FromApi(RestApi.CorporateActions.Dividend action)
        {
            var stock = _StockResolver.GetStock(action.Stock);
            return new Domain.CorporateActions.Dividend(action.Id, stock, action.ActionDate, action.Description, action.PaymentDate, action.Amount, action.PercentFranked, action.DrpPrice);
        }

        public RestApi.CorporateActions.SplitConsolidation ToApi(Domain.CorporateActions.SplitConsolidation action)
        {
            var response = new RestApi.CorporateActions.SplitConsolidation()
            {

                Id = action.Id,
                Stock = action.Stock.Id,
                ActionDate = action.Date,
                Description = action.Description,
                NewUnits = action.NewUnits,
                OriginalUnits = action.OriginalUnits
            };

            return response;
        }

        public Domain.CorporateActions.SplitConsolidation FromApi(RestApi.CorporateActions.SplitConsolidation action)
        {
            var stock = _StockResolver.GetStock(action.Stock);
            return new Domain.CorporateActions.SplitConsolidation(action.Id, stock, action.ActionDate, action.Description, action.OriginalUnits, action.NewUnits);
        }

        public RestApi.CorporateActions.Transformation ToApi(Domain.CorporateActions.Transformation action)
        {
            var response = new RestApi.CorporateActions.Transformation()
            {
                Id = action.Id,
                Stock = action.Stock.Id,
                ActionDate = action.Date,
                Description = action.Description,
                CashComponent = action.CashComponent,
                RolloverRefliefApplies = action.RolloverRefliefApplies
            };

            var resultStocks = action.ResultingStocks.Select(x => new RestApi.CorporateActions.Transformation.ResultingStock()
            {
                Stock = x.Stock,
                OriginalUnits = x.OriginalUnits,
                NewUnits = x.NewUnits,
                CostBase = x.CostBasePercentage,
                AquisitionDate = x.AquisitionDate
            });

            response.ResultingStocks.AddRange(resultStocks);

            return response;
        }

        public Domain.CorporateActions.Transformation FromApi(RestApi.CorporateActions.Transformation action)
        {
            var stock = _StockResolver.GetStock(action.Stock);

            var resultingStocks = action.ResultingStocks.Select(x => new Domain.CorporateActions.Transformation.ResultingStock(x.Stock, x.OriginalUnits, x.NewUnits, x.CostBase, x.AquisitionDate));
            return new Domain.CorporateActions.Transformation(action.Id, stock, action.ActionDate, action.Description, action.ImplementationDate, action.CashComponent, action.RolloverRefliefApplies, resultingStocks);
        }

    }

}
