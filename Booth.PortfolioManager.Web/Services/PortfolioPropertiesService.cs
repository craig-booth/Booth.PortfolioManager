using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Web.Models.Portfolio;
using Booth.PortfolioManager.Web.Mappers;

namespace Booth.PortfolioManager.Web.Services
{

    public interface IPortfolioPropertiesService
    { 
        ServiceResult<PortfolioPropertiesResponse> GetProperties();
    }

    public class PortfolioPropertiesService : IPortfolioPropertiesService
    {

        private readonly IReadOnlyPortfolio _Portfolio;

        public PortfolioPropertiesService(IReadOnlyPortfolio portfolio)
        {
            _Portfolio = portfolio;
        }

        public ServiceResult<PortfolioPropertiesResponse> GetProperties()
        {
            if (_Portfolio == null)
                return ServiceResult<PortfolioPropertiesResponse>.NotFound();

            var response = new PortfolioPropertiesResponse();

            response.Id = _Portfolio.Id;
            response.Name = _Portfolio.Name;
            response.StartDate = _Portfolio.StartDate;
            response.EndDate = _Portfolio.EndDate;

            foreach (var holding in _Portfolio.Holdings.All())
            {
                var holdingProperty = new Models.Portfolio.HoldingProperties()
                {
                    Stock = holding.Stock.ToSummaryResponse(Date.Today),
                    StartDate = holding.EffectivePeriod.FromDate,
                    EndDate = holding.EffectivePeriod.ToDate,
                    ParticipatingInDrp = holding.Settings.ParticipateInDrp
                };
                response.Holdings.Add(holdingProperty);
            }
            
            return ServiceResult<PortfolioPropertiesResponse>.Ok(response);
        }
    } 
}
