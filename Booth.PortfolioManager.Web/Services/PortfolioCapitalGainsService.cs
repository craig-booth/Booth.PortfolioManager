using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Utils;
using Booth.PortfolioManager.RestApi.Portfolios;
using Booth.PortfolioManager.Web.Mappers;

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
        private readonly IStockPriceRetriever _PriceRetriever;
        private readonly IStockMapper _Mapper;

        public PortfolioCapitalGainsService(IReadOnlyPortfolio portfolio, IStockPriceRetriever priceRetriever, IStockMapper mapper)
        {
            _Portfolio = portfolio;
            _PriceRetriever = priceRetriever;
            _Mapper = mapper;
        }

        public ServiceResult<SimpleUnrealisedGainsResponse> GetCapitalGains(Date date)
        {
            if (_Portfolio == null)
                return ServiceResult<SimpleUnrealisedGainsResponse>.NotFound();

            var result = GetCapitalGains(_Portfolio.Holdings.All(date), date); 

            return ServiceResult<SimpleUnrealisedGainsResponse>.Ok(result);

        }

        public ServiceResult<SimpleUnrealisedGainsResponse> GetCapitalGains(Guid stockId, Date date)
        {
            if (_Portfolio == null)
                return ServiceResult<SimpleUnrealisedGainsResponse>.NotFound();

            var holding = _Portfolio.Holdings[stockId];
            if (holding == null)
                return ServiceResult<SimpleUnrealisedGainsResponse>.NotFound();

            var result = GetCapitalGains(new[] { holding }, date);

            return ServiceResult<SimpleUnrealisedGainsResponse>.Ok(result);
        }

        public ServiceResult<DetailedUnrealisedGainsResponse> GetDetailedCapitalGains(Date date)
        {
            if (_Portfolio == null)
                return ServiceResult<DetailedUnrealisedGainsResponse>.NotFound();

            var result = GetDetailedCapitalGains(_Portfolio.Holdings.All(date), date);

            return ServiceResult<DetailedUnrealisedGainsResponse>.Ok(result);

        }

        public ServiceResult<DetailedUnrealisedGainsResponse> GetDetailedCapitalGains(Guid stockId, Date date)
        {
            if (_Portfolio == null)
                return ServiceResult<DetailedUnrealisedGainsResponse>.NotFound();

            var holding = _Portfolio.Holdings[stockId];
            if (holding == null)
                return ServiceResult<DetailedUnrealisedGainsResponse>.NotFound();

            var result = GetDetailedCapitalGains(new[] { holding }, date);

            return ServiceResult<DetailedUnrealisedGainsResponse>.Ok(result);
        }

        private SimpleUnrealisedGainsResponse GetCapitalGains(IEnumerable<IReadOnlyHolding> holdings, Date date)
        {
            var response = new SimpleUnrealisedGainsResponse();

            foreach (var holding in holdings)
            {
                foreach (var parcel in holding.Parcels(date))
                {
                    var properties = parcel.Properties[date];

                    var value = properties.Units * _PriceRetriever.GetPrice(holding.Stock.Id , date);
                    var capitalGain = value - properties.CostBase;
                    var discountMethod = CgtUtils.CgtMethodForParcel(parcel.AquisitionDate, date);
                    var discoutedGain = (discountMethod == Domain.Portfolios.CgtMethod.Discount) ? CgtUtils.DiscountedCgt(capitalGain, Domain.Portfolios.CgtMethod.Discount) : capitalGain;

                    var unrealisedGain = new SimpleUnrealisedGainsItem()
                    {
                        Stock = holding.Stock.ToSummaryResponse(date),
                        AquisitionDate = parcel.AquisitionDate,
                        Units = properties.Units,
                        CostBase = properties.CostBase,
                        MarketValue = value,
                        CapitalGain = capitalGain,
                        DiscoutedGain = discoutedGain,
                        DiscountMethod = discountMethod.ToResponse()
                    };

                    response.UnrealisedGains.Add(unrealisedGain);
                }
            } 

            return response;
        }

        private DetailedUnrealisedGainsResponse GetDetailedCapitalGains(IEnumerable<IReadOnlyHolding> holdings, Date date)
        {
            var response = new DetailedUnrealisedGainsResponse();

            foreach (var holding in holdings)
            {
                foreach (var parcel in holding.Parcels(date))
                {
                    var properties = parcel.Properties[date];

                    var value = properties.Units * _PriceRetriever.GetPrice(holding.Stock.Id, date);
                    var capitalGain = value - properties.CostBase;
                    var discountMethod = CgtUtils.CgtMethodForParcel(parcel.AquisitionDate, date);
                    var discoutedGain = (discountMethod == Domain.Portfolios.CgtMethod.Discount) ? CgtUtils.DiscountedCgt(capitalGain, Domain.Portfolios.CgtMethod.Discount) : capitalGain;

                    var unrealisedGain = new DetailedUnrealisedGainsItem()
                    {
                        Stock = holding.Stock.ToSummaryResponse(date),
                        AquisitionDate = parcel.AquisitionDate,
                        Units = properties.Units,
                        CostBase = properties.CostBase,
                        MarketValue = value,
                        CapitalGain = capitalGain,
                        DiscoutedGain = discoutedGain,
                        DiscountMethod = discountMethod.ToResponse()
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
                            UnitChange = auditRecord.UnitCountChange,
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
        } 
    } 
}
