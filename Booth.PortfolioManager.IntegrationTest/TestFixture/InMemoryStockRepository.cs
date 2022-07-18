using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Repository;

namespace Booth.PortfolioManager.IntegrationTest.TestFixture
{
    internal class InMemoryStockRepository : InMemoryRepository<Stock>, IStockRepository
    {
        public void AddCorporateAction(Stock stock, Guid id)
        {
            // No action required
        }

        public void DeleteCorporateAction(Stock stock, Guid id)
        {
            // No action required
        }

        public void UpdateCorporateAction(Stock stock, Guid id)
        {
            // No action required
        }

        public void UpdateDividendRules(Stock stock, Date date)
        {
            // No action required
        }

        public void UpdateProperties(Stock stock, Date date)
        {
            // No action required
        }

        public void UpdateRelativeNTAs(Stock stock, Date date)
        {
            // No action required
        }
    }
}
