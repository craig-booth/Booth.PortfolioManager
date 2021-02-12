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
    public class PortfolioPropertiesServiceTests
    {
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
            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            portfolio.ChangeDrpParticipation(PortfolioTestCreator.Stock_WAM.Id, true);

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
                        Stock = PortfolioTestCreator.Stock_ARG,
                        StartDate = new Date(2000, 01, 01),
                        EndDate = Date.MaxValue,
                        ParticipatingInDrp = false
                    },
                    new RestApi.Portfolios.HoldingProperties()
                    {
                        Stock = PortfolioTestCreator.Stock_WAM,
                        StartDate = new Date(2000, 01, 01),
                        EndDate = Date.MaxValue,
                        ParticipatingInDrp = true
                    }
                }

            });

        }
    }
}
