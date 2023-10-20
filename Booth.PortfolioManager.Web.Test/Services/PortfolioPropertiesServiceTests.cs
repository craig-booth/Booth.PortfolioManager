using System.Collections.Generic;
using System.Text;

using Xunit;
using FluentAssertions;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Domain;

namespace Booth.PortfolioManager.Web.Test.Services
{
    [Collection(Services.Collection)]
    public class PortfolioPropertiesServiceTests
    {
        private readonly ServicesTestFixture _Fixture;

        public PortfolioPropertiesServiceTests(ServicesTestFixture fixture)
        {
            _Fixture = fixture;
        }

        [Fact]
        public void PortfolioNotFound()
        {
            var service = new PortfolioPropertiesService(null);

            var result = service.GetProperties();

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetProperties()
        {
            var portfolio = _Fixture.CreateDefaultPortfolio();

            portfolio.ChangeDrpParticipation(_Fixture.Stock_WAM.Id, true);

            var service = new PortfolioPropertiesService(portfolio);

            var result = service.GetProperties();

            result.Result.Should().BeEquivalentTo(new
            {
                Id = portfolio.Id,
                Name = "Test",
                StartDate = new Date(2000, 01, 01),
                EndDate = Date.MaxValue,
                Holdings = new[]
                {
                    new RestApi.Portfolios.HoldingProperties()
                    {
                        Stock = _Fixture.Stock_ARG,
                        StartDate = new Date(2000, 01, 01),
                        EndDate = Date.MaxValue,
                        ParticipatingInDrp = false
                    },
                    new RestApi.Portfolios.HoldingProperties()
                    {
                        Stock = _Fixture.Stock_WAM,
                        StartDate = new Date(2000, 01, 01),
                        EndDate = Date.MaxValue,
                        ParticipatingInDrp = true
                    }
                }

            });

        }
    }
}
