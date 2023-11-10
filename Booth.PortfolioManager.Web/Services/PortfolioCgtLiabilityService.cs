using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.RestApi.Portfolios;
using Booth.PortfolioManager.Web.Mappers;

namespace Booth.PortfolioManager.Web.Services
{
    public interface IPortfolioCgtLiabilityService
    {
        ServiceResult<CgtLiabilityResponse> GetCGTLiability(DateRange dateRange);
    }

    
    public class PortfolioCgtLiabilityService : IPortfolioCgtLiabilityService
    {
        private readonly IReadOnlyPortfolio _Portfolio;
        private readonly IStockMapper _Mapper;

        public PortfolioCgtLiabilityService(IReadOnlyPortfolio portfolio, IStockMapper mapper)
        {
            _Portfolio = portfolio;
            _Mapper = mapper;
        }

        public ServiceResult<CgtLiabilityResponse> GetCGTLiability(DateRange dateRange)
        {
            if (_Portfolio == null)
                return ServiceResult<CgtLiabilityResponse>.NotFound();

            var response = new CgtLiabilityResponse();

            // Get a list of all the cgt events for the year
            var cgtEvents = _Portfolio.CgtEvents.InDateRange(dateRange);
            foreach (var cgtEvent in cgtEvents)
            {
                var item = new CgtLiabilityResponse.CgtLiabilityEvent()
                {
                    Stock = cgtEvent.Stock.ToSummaryResponse(cgtEvent.Date),
                    EventDate = cgtEvent.Date,
                    CostBase = cgtEvent.CostBase,
                    AmountReceived = cgtEvent.AmountReceived,
                    CapitalGain = cgtEvent.CapitalGain,
                    Method = cgtEvent.CgtMethod.ToResponse()
                };

                response.Events.Add(item);

                // Apportion capital gains
                if (cgtEvent.CapitalGain < 0)
                    response.CurrentYearCapitalLossesTotal += -cgtEvent.CapitalGain;
                else if (cgtEvent.CgtMethod == Domain.Portfolios.CgtMethod.Discount)
                    response.CurrentYearCapitalGainsDiscounted += cgtEvent.CapitalGain;
                else
                    response.CurrentYearCapitalGainsOther += cgtEvent.CapitalGain;
            } 

            response.CurrentYearCapitalGainsTotal = response.CurrentYearCapitalGainsOther + response.CurrentYearCapitalGainsDiscounted;
          
            if (response.CurrentYearCapitalGainsOther > response.CurrentYearCapitalLossesTotal)
                response.CurrentYearCapitalLossesOther = response.CurrentYearCapitalLossesTotal;
            else
                response.CurrentYearCapitalLossesOther = response.CurrentYearCapitalGainsOther;

            if (response.CurrentYearCapitalGainsOther > response.CurrentYearCapitalLossesTotal)
                response.CurrentYearCapitalLossesDiscounted = 0.00m;
            else
                response.CurrentYearCapitalLossesDiscounted = response.CurrentYearCapitalLossesTotal - response.CurrentYearCapitalGainsOther;
         
            response.GrossCapitalGainOther = response.CurrentYearCapitalGainsOther - response.CurrentYearCapitalLossesOther;
            response.GrossCapitalGainDiscounted = response.CurrentYearCapitalGainsDiscounted - response.CurrentYearCapitalLossesDiscounted;
            response.GrossCapitalGainTotal = response.GrossCapitalGainOther + response.GrossCapitalGainDiscounted;
            if (response.GrossCapitalGainDiscounted > 0)
                response.Discount = (response.GrossCapitalGainDiscounted / 2).ToCurrency(RoundingRule.Round);
            else
                response.Discount = 0.00m;
            response.NetCapitalGainOther = response.GrossCapitalGainOther;
            response.NetCapitalGainDiscounted = response.GrossCapitalGainDiscounted - response.Discount;
            response.NetCapitalGainTotal = response.NetCapitalGainOther + response.NetCapitalGainDiscounted;        

            return ServiceResult<CgtLiabilityResponse>.Ok(response); 
        }
    } 
}
