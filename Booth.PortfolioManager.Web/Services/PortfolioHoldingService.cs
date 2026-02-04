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

    public interface IPortfolioHoldingService
    {
        ServiceResult<List<Holding>> GetHoldings(Date date);
        ServiceResult<List<Holding>> GetHoldings(DateRange dateRange);
        ServiceResult<Holding> GetHolding(Guid stockId, Date date);
    }
    
    public class PortfolioHoldingService : IPortfolioHoldingService
    {
        private readonly IReadOnlyPortfolio _Portfolio;
        private readonly IHoldingMapper _Mapper;

        public PortfolioHoldingService(IReadOnlyPortfolio portfolio, IHoldingMapper mapper)
        {
            _Portfolio = portfolio;
            _Mapper = mapper;
        }

        public ServiceResult<List<Holding>> GetHoldings(Date date)
        {
            if (_Portfolio == null)
                return ServiceResult<List<Holding>>.NotFound();

            var holdings = _Portfolio.Holdings.All(date);

            var result = holdings.Select(x => _Mapper.ToApi(x, date)).ToList();

            return ServiceResult<List<Holding>>.Ok(result);
        }

        public ServiceResult<List<Holding>> GetHoldings(DateRange dateRange)
        {
            if (_Portfolio == null)
                return ServiceResult<List<Holding>>.NotFound();

            var holdings = _Portfolio.Holdings.All(dateRange);

            var result = holdings.Select(x => _Mapper.ToApi(x, dateRange.ToDate)).ToList();

            return ServiceResult<List<Holding>>.Ok(result);
        }

        public ServiceResult<Holding> GetHolding(Guid stockId, Date date)
        {
            if (_Portfolio == null)
                return ServiceResult<Holding>.NotFound();

            var holding = _Portfolio.Holdings[stockId];
            if (holding == null)
                return ServiceResult<Holding>.NotFound();

            var result = _Mapper.ToApi(holding, date);

            return ServiceResult<Holding>.Ok(result);
        }
    } 
} 
