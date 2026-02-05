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
        Models.CorporateAction.CorporateAction ToApi(ICorporateAction action);
        Domain.CorporateActions.CorporateAction FromApi(Models.CorporateAction.CorporateAction action);
        Models.CorporateAction.CapitalReturn ToApi(Domain.CorporateActions.CapitalReturn action);
        Domain.CorporateActions.CapitalReturn FromApi(Models.CorporateAction.CapitalReturn action);
        Models.CorporateAction.CompositeAction ToApi(Domain.CorporateActions.CompositeAction action);
        Domain.CorporateActions.CompositeAction FromApi(Models.CorporateAction.CompositeAction action);
        Models.CorporateAction.Dividend ToApi(Domain.CorporateActions.Dividend action);
        Domain.CorporateActions.Dividend FromApi(Models.CorporateAction.Dividend action);
        Models.CorporateAction.SplitConsolidation ToApi(Domain.CorporateActions.SplitConsolidation action);
        Domain.CorporateActions.SplitConsolidation FromApi(Models.CorporateAction.SplitConsolidation action);
        Models.CorporateAction.Transformation ToApi(Domain.CorporateActions.Transformation action);
        Domain.CorporateActions.Transformation FromApi(Models.CorporateAction.Transformation action);
    }

    class CorporateActionMapper : ICorporateActionMapper
    {
        private readonly IStockResolver _StockResolver;

        public CorporateActionMapper(IStockResolver stockResover)
        {
            _StockResolver = stockResover;  
        }

        public Models.CorporateAction.CorporateAction ToApi(ICorporateAction action)
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

        public Domain.CorporateActions.CorporateAction FromApi(Models.CorporateAction.CorporateAction action)
        {
            if (action is Models.CorporateAction.CapitalReturn capitalReturn)
                return FromApi(capitalReturn);
            else if (action is Models.CorporateAction.CompositeAction compositeAction)
                return FromApi(compositeAction);
            else if (action is Models.CorporateAction.Dividend dividend)
                return FromApi(dividend);
            else if (action is Models.CorporateAction.SplitConsolidation splitConsolidation)
                return FromApi(splitConsolidation);
            else if (action is Models.CorporateAction.Transformation transformation)
                return FromApi(transformation);
            else
                throw new NotSupportedException();
        }


        public Models.CorporateAction.CapitalReturn ToApi(Domain.CorporateActions.CapitalReturn action)
        {
            var response = new Models.CorporateAction.CapitalReturn()
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

        public Domain.CorporateActions.CapitalReturn FromApi(Models.CorporateAction.CapitalReturn action)
        {
            var stock = _StockResolver.GetStock(action.Stock);
            return new Domain.CorporateActions.CapitalReturn(action.Id, stock, action.ActionDate, action.Description, action.PaymentDate, action.Amount);
        }

        public Models.CorporateAction.CompositeAction ToApi(Domain.CorporateActions.CompositeAction action)
        {
            var response = new Models.CorporateAction.CompositeAction()
            {
                Id = action.Id,
                Stock = action.Stock.Id,
                ActionDate = action.Date,
                Description = action.Description
            };

            var childActions = action.ChildActions.Select(x => ToApi(x));
            response.ChildActions = new List<Models.CorporateAction.CorporateAction>(childActions);

            return response;
        }

        public Domain.CorporateActions.CompositeAction FromApi(Models.CorporateAction.CompositeAction action)
        {
            var stock = _StockResolver.GetStock(action.Stock);
            return new Domain.CorporateActions.CompositeAction(action.Id, stock, action.ActionDate, action.Description, action.ChildActions.Select(x => FromApi(x)));
        }

        public Models.CorporateAction.Dividend ToApi(Domain.CorporateActions.Dividend action)
        {
            var response = new Models.CorporateAction.Dividend()
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

        public Domain.CorporateActions.Dividend FromApi(Models.CorporateAction.Dividend action)
        {
            var stock = _StockResolver.GetStock(action.Stock);
            return new Domain.CorporateActions.Dividend(action.Id, stock, action.ActionDate, action.Description, action.PaymentDate, action.Amount, action.PercentFranked, action.DrpPrice);
        }

        public Models.CorporateAction.SplitConsolidation ToApi(Domain.CorporateActions.SplitConsolidation action)
        {
            var response = new Models.CorporateAction.SplitConsolidation()
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

        public Domain.CorporateActions.SplitConsolidation FromApi(Models.CorporateAction.SplitConsolidation action)
        {
            var stock = _StockResolver.GetStock(action.Stock);
            return new Domain.CorporateActions.SplitConsolidation(action.Id, stock, action.ActionDate, action.Description, action.OriginalUnits, action.NewUnits);
        }

        public Models.CorporateAction.Transformation ToApi(Domain.CorporateActions.Transformation action)
        {
            var response = new Models.CorporateAction.Transformation()
            {
                Id = action.Id,
                Stock = action.Stock.Id,
                ActionDate = action.Date,
                Description = action.Description,
                CashComponent = action.CashComponent,
                RolloverRefliefApplies = action.RolloverRefliefApplies
            };

            var resultStocks = action.ResultingStocks.Select(x => new Models.CorporateAction.Transformation.ResultingStock()
            {
                Stock = x.Stock,
                OriginalUnits = x.OriginalUnits,
                NewUnits = x.NewUnits,
                CostBase = x.CostBasePercentage,
                AquisitionDate = x.AquisitionDate
            });
            response.ResultingStocks = new List<Models.CorporateAction.Transformation.ResultingStock>(resultStocks);

            return response;
        }

        public Domain.CorporateActions.Transformation FromApi(Models.CorporateAction.Transformation action)
        {
            var stock = _StockResolver.GetStock(action.Stock);

            var resultingStocks = action.ResultingStocks.Select(x => new Domain.CorporateActions.Transformation.ResultingStock(x.Stock, x.OriginalUnits, x.NewUnits, x.CostBase, x.AquisitionDate));
            return new Domain.CorporateActions.Transformation(action.Id, stock, action.ActionDate, action.Description, action.ImplementationDate, action.CashComponent, action.RolloverRefliefApplies, resultingStocks);
        }

    }

}
