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

    public interface IPortfolioService
    {
        ServiceResult ChangeDrpParticipation(Guid stockId, bool participation);
    }
    
    public class PortfolioService : IPortfolioService
    {
        private readonly IPortfolio _Portfolio;

        public PortfolioService(IPortfolio portfolio)
        {
            _Portfolio = portfolio;
        }

        public ServiceResult ChangeDrpParticipation(Guid stockId, bool participation)
        {
            if (_Portfolio == null)
                return ServiceResult<List<Holding>>.NotFound();

            var holding = _Portfolio.Holdings[stockId];
            if (holding == null)
                return ServiceResult.NotFound();

            _Portfolio.ChangeDrpParticipation(stockId, participation);

            return ServiceResult.Ok();
        }
    } 
} 
