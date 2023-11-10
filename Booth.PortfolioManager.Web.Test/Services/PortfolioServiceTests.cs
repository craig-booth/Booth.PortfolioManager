using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Moq;
using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Domain.Users;
using Booth.PortfolioManager.Web.Authentication;
using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Web.Test.Services
{
    [Collection(Services.Collection)]
    public class PortfolioServiceTests
    {

        private readonly ServicesTestFixture _Fixture;

        public PortfolioServiceTests(ServicesTestFixture fixture)
        {
            _Fixture = fixture;
        }

        [Fact]
        public async Task PortfolioNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var repository = mockRepository.Create<IPortfolioRepository>();

            var service = new PortfolioService(_Fixture.PortfolioFactory, repository.Object);

            var result = await service.ChangeDrpParticipationAsync(null, Guid.NewGuid(), true);

            result.Should().HaveNotFoundStatus();

            mockRepository.Verify();
        }



        [Fact]
        public async Task ChangeDrpParticipationStockNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolio = _Fixture.CreateDefaultPortfolio();

            var repository = mockRepository.Create<IPortfolioRepository>();
           
            var service = new PortfolioService(_Fixture.PortfolioFactory, repository.Object);

            var result = await service.ChangeDrpParticipationAsync(portfolio, Guid.NewGuid(), true);

            result.Should().HaveNotFoundStatus();

            mockRepository.Verify();
        }


        [Fact]
        public async Task ChangeDrpParticipation()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolio = _Fixture.CreateDefaultPortfolio();

            var repository = mockRepository.Create<IPortfolioRepository>();
            repository.Setup(x => x.UpdateAsync(It.Is<Portfolio>(x => x.Id == portfolio.Id))).Returns(Task.CompletedTask).Verifiable();     

            var service = new PortfolioService(_Fixture.PortfolioFactory, repository.Object);

            var result = await service.ChangeDrpParticipationAsync(portfolio, _Fixture.Stock_ARG.Id, true);

            result.Should().HaveOkStatus();

            mockRepository.Verify();
        }

        [Fact]
        public async Task CreatePortfolio()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolioId = Guid.NewGuid();

            var repository = mockRepository.Create<IPortfolioRepository>();
            repository.Setup(x => x.AddAsync(It.Is<Portfolio>(x => x.Id == portfolioId))).Returns(Task.CompletedTask).Verifiable();

            var service = new PortfolioService(_Fixture.PortfolioFactory, repository.Object);

            var result = await service.CreatePortfolioAsync(portfolioId, "My Portfolio", Guid.NewGuid());

            result.Should().HaveOkStatus();

            mockRepository.Verify();
        }

    } 
}
