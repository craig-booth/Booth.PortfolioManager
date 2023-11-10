using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.RestApi.Portfolios;

namespace Booth.PortfolioManager.Web.Mappers
{

    public interface IHoldingMapper
    {
        RestApi.Portfolios.Holding ToApi(Domain.Portfolios.IReadOnlyHolding holding, Date date);
    }

    public class HoldingMapper : IHoldingMapper
    {
        private readonly IStockPriceRetriever _PriceRetreiver;
        public HoldingMapper(IStockPriceRetriever priceRetriever)
        {
            _PriceRetreiver = priceRetriever;
        }

        public RestApi.Portfolios.Holding ToApi(Domain.Portfolios.IReadOnlyHolding holding, Date date)
        {
            var properties = holding.Properties.ClosestTo(date);

            var response = new Holding()
            {
                Stock = holding.Stock.ToSummaryResponse(date),
                Units = properties.Units,
                Value = properties.Units * _PriceRetreiver.GetPrice(holding.Stock.Id, date),
                Cost = properties.Amount,
                CostBase = properties.CostBase
            };

            return response;
        }
    }
}
