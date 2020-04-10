using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using Moq;

using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Stocks; 

namespace Booth.PortfolioManager.Domain.Test.Portfolios
{
    class PortfolioEntityFactoryTests
    {

        [TestCase]
        public void CreatePortfolio()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var createdPortfolio = new Portfolio(id, null, null);
            var portfolioFactory = mockRepository.Create<IPortfolioFactory>();
            portfolioFactory.Setup(x => x.CreatePortfolio(id, "", Guid.Empty)).Returns(createdPortfolio).Verifiable();
            var entityFactory = new PortfolioEntityFactory(portfolioFactory.Object);

            var portfolio = entityFactory.Create(id, "test");

            Assert.That(portfolio.Id, Is.EqualTo(id));

            mockRepository.Verify();
        }
    }
}
