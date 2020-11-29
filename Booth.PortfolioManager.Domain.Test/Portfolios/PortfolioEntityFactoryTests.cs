using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using Moq;

using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Stocks;
using FluentAssertions;

namespace Booth.PortfolioManager.Domain.Test.Portfolios
{
    public class PortfolioEntityFactoryTests
    {

        [Fact]
        public void CreatePortfolio()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var createdPortfolio = new Portfolio(id, null, null);
            var portfolioFactory = mockRepository.Create<IPortfolioFactory>();
            portfolioFactory.Setup(x => x.CreatePortfolio(id)).Returns(createdPortfolio).Verifiable();
            var entityFactory = new PortfolioEntityFactory(portfolioFactory.Object);

            var portfolio = entityFactory.Create(id, "test");

            portfolio.Id.Should().Be(id);

            mockRepository.Verify();
        }
    }
}
