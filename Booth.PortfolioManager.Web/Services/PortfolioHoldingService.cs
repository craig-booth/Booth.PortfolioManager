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

    public interface IPortfolioHoldingService
    {
        ServiceResult<List<Holding>> GetHoldings(Date date);
        ServiceResult<List<Holding>> GetHoldings(DateRange dateRange);
        ServiceResult<Holding> GetHolding(Guid stockId, Date date);
    }
    
    public class PortfolioHoldingService : IPortfolioHoldingService
    {
        private readonly IReadOnlyPortfolio _Portfolio;

        public PortfolioHoldingService(IReadOnlyPortfolio portfolio)
        {
            _Portfolio = portfolio;
        }

        public ServiceResult<List<Holding>> GetHoldings(Date date)
        {
            if (_Portfolio == null)
                return ServiceResult<List<Holding>>.NotFound();

            var holdings = _Portfolio.Holdings.All(date);

            var result = holdings.Select(x => x.ToResponse(date)).ToList();

            return ServiceResult<List<Holding>>.Ok(result);
        }

        public ServiceResult<List<Holding>> GetHoldings(DateRange dateRange)
        {
            if (_Portfolio == null)
                return ServiceResult<List<Holding>>.NotFound();

            var holdings = _Portfolio.Holdings.All(dateRange);

            var result = holdings.Select(x => x.ToResponse(dateRange.ToDate)).ToList();

            return ServiceResult<List<Holding>>.Ok(result);
        }

        public ServiceResult<Holding> GetHolding(Guid stockId, Date date)
        {
            if (_Portfolio == null)
                return ServiceResult<Holding>.NotFound();

            var holding = _Portfolio.Holdings[stockId];
            if (holding == null)
                return ServiceResult<Holding>.NotFound();

            var result = holding.ToResponse(date);

            return ServiceResult<Holding>.Ok(result);
        }
    } 
} 
