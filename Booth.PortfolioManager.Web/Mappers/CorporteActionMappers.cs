using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.Domain.CorporateActions;
using Booth.PortfolioManager.RestApi.CorporateActions;
using Microsoft.AspNetCore.Builder;

namespace Booth.PortfolioManager.Web.Mappers
{
    static class CorporateActionMappers
    {

        public static RestApi.CorporateActions.CorporateAction ToApi(this ICorporateAction action)
        {
            if (action is Domain.CorporateActions.CapitalReturn capitalReturn)
                return capitalReturn.ToApi();
            else if (action is Domain.CorporateActions.CompositeAction compositeAction)
                return compositeAction.ToApi();
            else if (action is Domain.CorporateActions.Dividend dividend)
                return dividend.ToApi();
            else if (action is Domain.CorporateActions.SplitConsolidation splitConsolidation)
                return splitConsolidation.ToApi();
            else if (action is Domain.CorporateActions.Transformation transformation)
                return transformation.ToApi();
            else
                throw new NotSupportedException();
        }

        public static Domain.CorporateActions.CorporateAction FromApi(this RestApi.CorporateActions.CorporateAction action)
        {
            if (action is RestApi.CorporateActions.CapitalReturn capitalReturn)
                return capitalReturn.FromApi();
            else if (action is RestApi.CorporateActions.CompositeAction compositeAction)
                return compositeAction.FromApi();
            else if (action is RestApi.CorporateActions.Dividend dividend)
                return dividend.FromApi();
            else if (action is RestApi.CorporateActions.SplitConsolidation splitConsolidation)
                return splitConsolidation.FromApi();
            else if (action is RestApi.CorporateActions.Transformation transformation)
                return transformation.FromApi();
            else
                throw new NotSupportedException();
        }

        public static RestApi.CorporateActions.CapitalReturn ToApi(this Domain.CorporateActions.CapitalReturn action)
        {
            var response = new RestApi.CorporateActions.CapitalReturn(); 

            PopulateCorporateAction(response, action);

            response.PaymentDate = action.PaymentDate;
            response.Amount = action.Amount;

            return response;
        }

        public static Domain.CorporateActions.CapitalReturn FromApi(this RestApi.CorporateActions.CapitalReturn action)
        {
            return new Domain.CorporateActions.CapitalReturn(action.Id, null, action.ActionDate, action.Description, action.PaymentDate, action.Amount);
        }

        public static RestApi.CorporateActions.CompositeAction ToApi(this Domain.CorporateActions.CompositeAction action)
        {
            var response = new RestApi.CorporateActions.CompositeAction();

            PopulateCorporateAction(response, action);

            var childActions = action.ChildActions.Select(x => x.ToApi());
            response.ChildActions.AddRange(childActions);

            return response;
        }
        public static Domain.CorporateActions.CompositeAction FromApi(this RestApi.CorporateActions.CompositeAction action)
        {
            return new Domain.CorporateActions.CompositeAction(action.Id, null, action.ActionDate, action.Description, action.ChildActions.Select(x => x.FromApi()));
        }

        public static RestApi.CorporateActions.Dividend ToApi(this Domain.CorporateActions.Dividend action)
        {
            var response = new RestApi.CorporateActions.Dividend();

            PopulateCorporateAction(response, action);

            response.PaymentDate = action.PaymentDate;
            response.Amount = action.DividendAmount;
            response.PercentFranked = action.PercentFranked;
            response.DrpPrice = action.DrpPrice;

            return response;
        }
        public static Domain.CorporateActions.Dividend FromApi(this RestApi.CorporateActions.Dividend action)
        {
            return new Domain.CorporateActions.Dividend(action.Id, null, action.ActionDate, action.Description, action.PaymentDate, action.Amount, action.PercentFranked, action.DrpPrice);
        }

        public static RestApi.CorporateActions.SplitConsolidation ToApi(this Domain.CorporateActions.SplitConsolidation action)
        {
            var response = new RestApi.CorporateActions.SplitConsolidation();

            PopulateCorporateAction(response, action);

            response.NewUnits = action.NewUnits;
            response.OriginalUnits = action.OriginalUnits;

            return response;
        }
        public static Domain.CorporateActions.SplitConsolidation FromApi(this RestApi.CorporateActions.SplitConsolidation action)
        {
            return new Domain.CorporateActions.SplitConsolidation(action.Id, null, action.ActionDate, action.Description, action.OriginalUnits, action.NewUnits);
        }

        public static RestApi.CorporateActions.Transformation ToApi(this Domain.CorporateActions.Transformation action)
        {
            var response = new RestApi.CorporateActions.Transformation();

            PopulateCorporateAction(response, action);

            response.CashComponent = action.CashComponent;
            response.RolloverRefliefApplies = action.RolloverRefliefApplies;

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
        public static Domain.CorporateActions.Transformation FromApi(this RestApi.CorporateActions.Transformation action)
        {
            var resultingStocks = action.ResultingStocks.Select(x => new Domain.CorporateActions.Transformation.ResultingStock(x.Stock, x.OriginalUnits, x.NewUnits, x.CostBase, x.AquisitionDate));
            return new Domain.CorporateActions.Transformation(action.Id, null, action.ActionDate, action.Description, action.ImplementationDate, action.CashComponent, action.RolloverRefliefApplies, resultingStocks);
        }

        private static void PopulateCorporateAction(RestApi.CorporateActions.CorporateAction response, Domain.CorporateActions.CorporateAction action)
        {
            response.Id = action.Id;
            response.Stock = action.Stock.Id;
            response.ActionDate = action.Date;
            response.Description = action.Description;
        }
    }
}
