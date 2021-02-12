﻿using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.Web.Services;

namespace Booth.PortfolioManager.Web.Test.Services
{
    public class PortfolioHoldingServiceTests
    {
        [Fact]
        public void PortfolioNotFound()
        {
            var service = new PortfolioHoldingService(null);

            var result = service.GetHoldings(new Date(2000, 01, 01));

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetHoldingsAtDate()
        {
            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var service = new PortfolioHoldingService(portfolio);

            var result = service.GetHoldings(new Date(2000, 01, 01));

            result.Result.Should().BeEquivalentTo(new []
            {
                new RestApi.Portfolios.Holding()
                {
                    Stock = PortfolioTestCreator.Stock_ARG,
                    Units = 100,
                    Value = 100.00m,
                    Cost = 119.95m,
                    CostBase = 119.95m,
                },
                new RestApi.Portfolios.Holding()
                {
                    Stock = PortfolioTestCreator.Stock_WAM,
                    Units = 200,
                    Value = 240.00m,
                    Cost = 259.95m,
                    CostBase = 259.95m,
                }
            });
        }


        [Fact]
        public void GetHoldingsInDateRange()
        {
            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var service = new PortfolioHoldingService(portfolio);

            var result = service.GetHoldings(new DateRange(new Date(2000, 01, 01), new Date(2003, 01, 01)));

            result.Result.Should().BeEquivalentTo(new[]
            {
                new RestApi.Portfolios.Holding()
                {
                    Stock = PortfolioTestCreator.Stock_ARG,
                    Units = 200,
                    Value = 198.00m,
                    Cost = 239.90m,
                    CostBase = 239.90m,
                },
                new RestApi.Portfolios.Holding()
                {
                    Stock = PortfolioTestCreator.Stock_WAM,
                    Units = 200,
                    Value = 254.00m,
                    Cost = 259.95m,
                    CostBase = 259.95m,
                }
            });
        }

        [Fact]
        public void GetHoldingAtDateStockNotFound()
        {
            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var service = new PortfolioHoldingService(portfolio);

            var result = service.GetHolding(Guid.NewGuid(), new Date(2000, 01, 01));

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetHoldingAtDate()
        {
            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var service = new PortfolioHoldingService(portfolio);

            var result = service.GetHolding(PortfolioTestCreator.Stock_ARG.Id, new Date(2000, 01, 01));

            result.Result.Should().BeEquivalentTo(new RestApi.Portfolios.Holding()
            {
                Stock = PortfolioTestCreator.Stock_ARG,
                Units = 100,
                Value = 100.00m,
                Cost = 119.95m,
                CostBase = 119.95m,
            });
        }


    } 
}
