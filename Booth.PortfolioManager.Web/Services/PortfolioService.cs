using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.RestApi.Portfolios;

namespace Booth.PortfolioManager.Web.Services
{

    public interface IPortfolioService
    {
        ServiceResult<List<Holding>> GetHoldings(Date date);
        ServiceResult<List<Holding>> GetHoldings(DateRange dateRange);
        ServiceResult<Holding> GetHolding(Guid stockId, Date date);
        ServiceResult ChangeDrpParticipation(Guid stockId, bool participation);
    }
    
    public class PortfolioService : IPortfolioService
    {
        private readonly IReadOnlyPortfolio _Portfolio;

        public PortfolioService(IReadOnlyPortfolio portfolio)
        {
            _Portfolio = portfolio;
        }

        public ServiceResult<List<Holding>> GetHoldings(Date date)
        {
            /*  var portfolio = _PortfolioCache.Get(portfolioId);

              var holdings = portfolio.Holdings.All(date);

              return _Mapper.Map<List<RestApi.Portfolios.Holding>>(holdings, opts => opts.Items["date"] = date); */
            throw new NotSupportedException();
        }

        public ServiceResult<List<Holding>> GetHoldings(DateRange dateRange)
        {
            /*  var portfolio = _PortfolioCache.Get(portfolioId);

              var holdings = portfolio.Holdings.All(dateRange);

              return _Mapper.Map<List<RestApi.Portfolios.Holding>>(holdings, opts => opts.Items["date"] = dateRange.ToDate); */

            throw new NotSupportedException();
        }

        public ServiceResult<Holding> GetHolding(Guid stockId, Date date)
        {
            /*     var portfolio = _PortfolioCache.Get(portfolioId);

                 var holding = portfolio.Holdings.Get(id);
                 if (holding == null)
                     throw new HoldingNotFoundException(id);

                 return _Mapper.Map<RestApi.Portfolios.Holding>(holding, opts => opts.Items["date"] = date); */
            throw new NotSupportedException();
        }

        public ServiceResult ChangeDrpParticipation(Guid stockId, bool participation)
        {
            /*   var portfolio = _PortfolioCache.Get(portfolioId);

               portfolio.ChangeDrpParticipation(holding, participation);

               _PortfolioRepository.Update(portfolio); */
            throw new NotSupportedException();
        }
    } 
} 
