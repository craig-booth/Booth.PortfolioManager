using System;
using System.Collections.Generic;
using System.Text;

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
    public class PortfolioServiceTests
    {
        [Fact]
        public void PortfolioNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var repository = mockRepository.Create<IPortfolioRepository>();

            var service = new PortfolioService(PortfolioTestCreator.PortfolioFactory, repository.Object);

            var result = service.ChangeDrpParticipation(null, Guid.NewGuid(), true);

            result.Should().HaveNotFoundStatus();

            mockRepository.Verify();
        }



        [Fact]
        public void ChangeDrpParticipationStockNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var repository = mockRepository.Create<IPortfolioRepository>();
           
            var service = new PortfolioService(PortfolioTestCreator.PortfolioFactory, repository.Object);

            var result = service.ChangeDrpParticipation(portfolio, Guid.NewGuid(), true);

            result.Should().HaveNotFoundStatus();

            mockRepository.Verify();
        }


        [Fact]
        public void ChangeDrpParticipation()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var repository = mockRepository.Create<IPortfolioRepository>();
            repository.Setup(x => x.Update(It.Is<Portfolio>(x => x.Id == portfolio.Id))).Verifiable();     

            var service = new PortfolioService(PortfolioTestCreator.PortfolioFactory, repository.Object);

            var result = service.ChangeDrpParticipation(portfolio, PortfolioTestCreator.Stock_ARG.Id, true);

            result.Should().HaveOkStatus();

            mockRepository.Verify();
        }

        [Fact]
        public void CreatePortfolio()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolioId = Guid.NewGuid();

            var repository = mockRepository.Create<IPortfolioRepository>();
            repository.Setup(x => x.Add(It.Is<Portfolio>(x => x.Id == portfolioId))).Verifiable();

            var service = new PortfolioService(PortfolioTestCreator.PortfolioFactory, repository.Object);

            var result = service.CreatePortfolio(portfolioId, "My Portfolio", Guid.NewGuid());

            result.Should().HaveOkStatus();

            mockRepository.Verify();
        }

    } 
}
