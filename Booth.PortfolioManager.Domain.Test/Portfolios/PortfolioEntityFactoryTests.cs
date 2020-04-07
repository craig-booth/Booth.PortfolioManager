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

            var stockResolver = mockRepository.Create<IStockResolver>();

            var factory = new PortfolioEntityFactory(stockResolver.Object);

            var id = Guid.NewGuid();
            var portfolio = factory.Create(id, "test");

            Assert.That(portfolio.Id, Is.EqualTo(id));

            mockRepository.Verify();
        }
    }
}
