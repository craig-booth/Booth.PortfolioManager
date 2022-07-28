using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.IntegrationTest.TestFixture
{
    internal class InMemoryPortfolioRepository : InMemoryRepository<Portfolio>, IPortfolioRepository
    {
        public void AddTransaction(Portfolio portfolio, Guid id)
        {
            throw new NotImplementedException();
        }

        public void DeleteTransaction(Portfolio portfolio, Guid id)
        {
            throw new NotImplementedException();
        }

        public void UpdateTransaction(Portfolio portfolio, Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
