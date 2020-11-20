using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.Web.Services;

namespace Booth.PortfolioManager.Web.Test.Services
{
    public class PortfolioServiceTests
    {
        [Fact]
        public void PortfolioNotFound()
        {
            var service = new PortfolioService(null);

            var result = service.ChangeDrpParticipation(Guid.NewGuid(), true);

            result.Should().HaveNotFoundStatus();
        }



        [Fact]
        public void ChangeDrpParticipationStockNotFound()
        {
            var portfolio = PortfolioTestCreator.CreatePortfolio();

            var service = new PortfolioService(portfolio);

            var result = service.ChangeDrpParticipation(Guid.NewGuid(), true);

            result.Should().HaveNotFoundStatus();
        }


        [Fact]
        public void ChangeDrpParticipation()
        {
            var portfolio = PortfolioTestCreator.CreatePortfolio();

            var service = new PortfolioService(portfolio);

            var result = service.ChangeDrpParticipation(PortfolioTestCreator.Stock_ARG.Id, true);

            result.Should().HaveOkStatus();
        }

    } 
}
