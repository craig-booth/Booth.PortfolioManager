using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.RestApi.Portfolios;

namespace Booth.PortfolioManager.Web.Services
{
    public interface IPortfolioCapitalGainsService
    {
        ServiceResult<SimpleUnrealisedGainsResponse> GetCapitalGains(Date date);
        ServiceResult<SimpleUnrealisedGainsResponse> GetCapitalGains(Guid stockId, Date date);
        ServiceResult<DetailedUnrealisedGainsResponse> GetDetailedCapitalGains(Date date);
        ServiceResult<DetailedUnrealisedGainsResponse> GetDetailedCapitalGains(Guid stockId, Date date);
    }

    public class PortfolioCapitalGainsService: IPortfolioCapitalGainsService
    {

        private readonly IReadOnlyPortfolio _Portfolio;

        public PortfolioCapitalGainsService(IReadOnlyPortfolio portfolio)
        {
            _Portfolio = portfolio;
        }

        public ServiceResult<SimpleUnrealisedGainsResponse> GetCapitalGains(Date date)
        {
            /* var portfolio = _PortfolioCache.Get(portfolioId);

             return GetCapitalGains(portfolio.Holdings.All(date), date); */

            throw new NotSupportedException();
        }

        public ServiceResult<SimpleUnrealisedGainsResponse> GetCapitalGains(Guid stockId, Date date)
        {
          /*  var portfolio = _PortfolioCache.Get(portfolioId);

            var holding = portfolio.Holdings.Get(stockId);
            if (holding == null)
                throw new HoldingNotFoundException(stockId);

            return GetCapitalGains(new[] { holding} , date); */

            throw new NotSupportedException();
        }

        public ServiceResult<DetailedUnrealisedGainsResponse> GetDetailedCapitalGains(Date date)
        {
         /*   var portfolio = _PortfolioCache.Get(portfolioId);

            return GetDetailedCapitalGains(portfolio.Holdings.All(date), date); */

            throw new NotSupportedException();
        }

        public ServiceResult<DetailedUnrealisedGainsResponse> GetDetailedCapitalGains(Guid stockId, Date date)
        {
     /*       var portfolio = _PortfolioCache.Get(portfolioId);

            var holding = portfolio.Holdings.Get(stockId);
            if (holding == null)
                throw new HoldingNotFoundException(stockId);

            return GetDetailedCapitalGains(new[] { holding }, date);
      */
            throw new NotSupportedException();
        }

  /*      private SimpleUnrealisedGainsResponse GetCapitalGains(IEnumerable<Domain.Portfolios.Holding> holdings, DateTime date)
        {
            var response = new SimpleUnrealisedGainsResponse();

            foreach (var holding in holdings)
            {
                foreach (var parcel in holding.Parcels(date))
                {
                    var properties = parcel.Properties[date];

                    var value = properties.Units * holding.Stock.GetPrice(date);
                    var capitalGain = value - properties.CostBase;
                    var discountMethod = CgtCalculator.CgtMethodForParcel(parcel.AquisitionDate, date);
                    var discoutedGain = (discountMethod == CGTMethod.Discount) ? CgtCalculator.CgtDiscount(capitalGain) : capitalGain;

                    var unrealisedGain = new SimpleUnrealisedGainsItem()
                    {
                        Stock = holding.Stock.Convert(date),
                        AquisitionDate = parcel.AquisitionDate,
                        Units = properties.Units,
                        CostBase = properties.CostBase,
                        MarketValue = value,
                        CapitalGain = capitalGain,
                        DiscoutedGain = discoutedGain,
                        DiscountMethod = discountMethod
                    };

                    response.UnrealisedGains.Add(unrealisedGain);
                }
            }

            return response;
        }

        private DetailedUnrealisedGainsResponse GetDetailedCapitalGains(IEnumerable<Domain.Portfolios.Holding> holdings, DateTime date)
        {
            var response = new DetailedUnrealisedGainsResponse();

            foreach (var holding in holdings)
            {
                foreach (var parcel in holding.Parcels(date))
                {
                    var properties = parcel.Properties[date];

                    var value = properties.Units * holding.Stock.GetPrice(date);
                    var capitalGain = value - properties.CostBase;
                    var discountMethod = CgtCalculator.CgtMethodForParcel(parcel.AquisitionDate, date);
                    var discoutedGain = (discountMethod == CGTMethod.Discount) ? CgtCalculator.CgtDiscount(capitalGain) : capitalGain;

                    var unrealisedGain = new DetailedUnrealisedGainsItem()
                    {
                        Stock = holding.Stock.Convert(date),
                        AquisitionDate = parcel.AquisitionDate,
                        Units = properties.Units,
                        CostBase = properties.CostBase,
                        MarketValue = value,
                        CapitalGain = capitalGain,
                        DiscoutedGain = discoutedGain,
                        DiscountMethod = discountMethod
                    };

                    int units = 0;
                    decimal costBase = 0.00m;
                    foreach (var auditRecord in parcel.Audit.TakeWhile(x => x.Date <= date))
                    {
                        units += auditRecord.UnitCountChange;
                        costBase += auditRecord.CostBaseChange;

                        var cgtEvent = new DetailedUnrealisedGainsItem.CGTEventItem()
                        {
                            Date = auditRecord.Date,
                            Description = auditRecord.Transaction.Description,
                            Units = units,
                            CostBaseChange = auditRecord.CostBaseChange,
                            CostBase = costBase,
                        };

                        unrealisedGain.CGTEvents.Add(cgtEvent);
                    }

                    response.UnrealisedGains.Add(unrealisedGain);

                }
            }

            return response;
        } */
    } 
}
