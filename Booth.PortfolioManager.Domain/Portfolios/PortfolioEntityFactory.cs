using System;
using System.Collections.Generic;
using System.Text;

using Booth.EventStore;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Portfolios
{
    public class PortfolioEntityFactory : ITrackedEntityFactory<Portfolio>
    {
        private IPortfolioFactory _PortfolioFactory;

        public PortfolioEntityFactory(IPortfolioFactory portfolioFactory)
        {
            _PortfolioFactory = portfolioFactory;
        }

        public Portfolio Create(Guid id, string storedEntityType)
        {
            return _PortfolioFactory.CreatePortfolio(id);
        }
    }
}
