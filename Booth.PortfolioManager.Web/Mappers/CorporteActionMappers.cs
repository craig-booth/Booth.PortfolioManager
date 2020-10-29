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

        public static RestApi.CorporateActions.CorporateAction ToResponse(this Domain.CorporateActions.ICorporateAction action)
        {
            if (action is Domain.CorporateActions.CapitalReturn capitalReturn)
                return capitalReturn.ToResponse();
            else if (action is Domain.CorporateActions.CompositeAction compositeAction)
                return compositeAction.ToResponse();
            else if (action is Domain.CorporateActions.Dividend dividend)
                return dividend.ToResponse();
            else if (action is Domain.CorporateActions.SplitConsolidation splitConsolidation)
                return splitConsolidation.ToResponse();
            else if (action is Domain.CorporateActions.Transformation transformation)
                return transformation.ToResponse();
            else
                throw new NotSupportedException();
        }

        public static RestApi.CorporateActions.CorporateAction ToResponse(this Domain.CorporateActions.CorporateAction action)
        {
            if (action is Domain.CorporateActions.CapitalReturn capitalReturn)
                return capitalReturn.ToResponse();
            else if (action is Domain.CorporateActions.CompositeAction compositeAction)
                return compositeAction.ToResponse();
            else if (action is Domain.CorporateActions.Dividend dividend)
                return dividend.ToResponse();
            else if (action is Domain.CorporateActions.SplitConsolidation splitConsolidation)
                return splitConsolidation.ToResponse();
            else if (action is Domain.CorporateActions.Transformation transformation)
                return transformation.ToResponse();
            else
                throw new NotSupportedException();
        }

        public static RestApi.CorporateActions.CapitalReturn ToResponse(this Domain.CorporateActions.CapitalReturn action)
        {
            var response = new RestApi.CorporateActions.CapitalReturn(); 

            PopulateCorporateAction(response, action);

            response.PaymentDate = action.PaymentDate;
            response.Amount = action.Amount;

            return response;
        }

        public static RestApi.CorporateActions.CompositeAction ToResponse(this Domain.CorporateActions.CompositeAction action)
        {
            var response = new RestApi.CorporateActions.CompositeAction();

            PopulateCorporateAction(response, action);

            var childActions = action.ChildActions.Select(x => x.ToResponse());
            response.ChildActions.AddRange(childActions);

            return response;
        }
        
        public static RestApi.CorporateActions.Dividend ToResponse(this Domain.CorporateActions.Dividend action)
        {
            var response = new RestApi.CorporateActions.Dividend();

            PopulateCorporateAction(response, action);

            response.PaymentDate = action.PaymentDate;
            response.Amount = action.DividendAmount;
            response.PercentFranked = action.PercentFranked;
            response.DrpPrice = action.DrpPrice;

            return response;
        }

        public static RestApi.CorporateActions.SplitConsolidation ToResponse(this Domain.CorporateActions.SplitConsolidation action)
        {
            var response = new RestApi.CorporateActions.SplitConsolidation();

            PopulateCorporateAction(response, action);

            response.NewUnits = action.NewUnits;
            response.OriginalUnits = action.OriginalUnits;

            return response;
        }

        public static RestApi.CorporateActions.Transformation ToResponse(this Domain.CorporateActions.Transformation action)
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

        private static void PopulateCorporateAction(RestApi.CorporateActions.CorporateAction response, Domain.CorporateActions.CorporateAction action)
        {
            response.Id = action.Id;
            response.Stock = action.Stock.Id;
            response.ActionDate = action.Date;
            response.Description = action.Description;
        }
    }
}
