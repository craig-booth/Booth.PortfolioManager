using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.RestApi.Portfolios;

namespace Booth.PortfolioManager.Web.Mappers
{
    public static class HoldingMappers
    {
        public static Holding ToResponse(this Domain.Portfolios.IReadOnlyHolding holding, Date date)
        {
            var properties = holding.Properties.ClosestTo(date);

            var response = new Holding()
            {
                Stock = holding.Stock.ToSummaryResponse(date),
                Units = properties.Units,
                Value = properties.Units * holding.Stock.GetPrice(date),
                Cost = properties.Amount,
                CostBase = properties.CostBase
            };

            return response;
        }
    }
}
