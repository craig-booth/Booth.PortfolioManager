using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.RestApi.Portfolios;
using Booth.PortfolioManager.Web.Mappers;

namespace Booth.PortfolioManager.Web.Services
{

    public interface IPortfolioService
    {
        ServiceResult CreatePortfolio(Guid id, string name, Guid owner);
        ServiceResult ChangeDrpParticipation(IPortfolio portfolio, Guid stockId, bool participation);
    }
    
    public class PortfolioService : IPortfolioService
    {
        private readonly IPortfolioFactory _Factory;
        private readonly IPortfolioRepository _Repository;

        public PortfolioService(IPortfolioFactory factory, IPortfolioRepository repository)
        {
            _Factory = factory;
            _Repository = repository;
        }

        public ServiceResult CreatePortfolio(Guid id, string name, Guid owner)
        {
            var portfolio = _Factory.CreatePortfolio(id);
            portfolio.Create(name, owner);

            _Repository.Add(portfolio);

            return ServiceResult.Ok();
        }

        public ServiceResult ChangeDrpParticipation(IPortfolio portfolio, Guid stockId, bool participation)
        {
            if (portfolio == null)
                return ServiceResult<List<Holding>>.NotFound();

            var holding = portfolio.Holdings[stockId];
            if (holding == null)
                return ServiceResult.NotFound();

            portfolio.ChangeDrpParticipation(stockId, participation);

            _Repository.Update((Portfolio)portfolio);

            return ServiceResult.Ok();
        }
    } 
} 
