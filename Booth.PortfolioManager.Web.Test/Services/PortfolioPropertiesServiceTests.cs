using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using FluentAssertions;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Domain.Stocks;
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
        public void PortfolioExists()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockProperties = mockRepository.Create<IEffectiveProperties<StockProperties>>();
            stockProperties.Setup(x => x.ClosestTo(Date.Today)).Returns(new StockProperties("BHP", "BHP Pty Ltd", AssetCategory.AustralianStocks));

            var stockId = Guid.NewGuid();
            var stock = mockRepository.Create<IReadOnlyStock>();
            stock.SetupGet(x => x.Id).Returns(stockId);
            stock.SetupGet(x => x.Properties).Returns(stockProperties.Object);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.SetupGet(x => x.Stock).Returns(stock.Object);
            holding.SetupGet(x => x.EffectivePeriod).Returns(new DateRange(new Date(2001, 01, 01), new Date(2002, 01, 01)));
            holding.SetupGet(x => x.Settings).Returns(new HoldingSettings(true));

            var holdingsCollection = mockRepository.Create<IHoldingCollection>();
            holdingsCollection.Setup(x => x.All()).Returns(new[] { holding.Object });

            var id = Guid.NewGuid();
            var portfolio = mockRepository.Create<IReadOnlyPortfolio>();
            portfolio.SetupGet(x => x.Id).Returns(id);
            portfolio.SetupGet(x => x.Name).Returns("Test");
            portfolio.SetupGet(x => x.StartDate).Returns(new Date(2000, 01, 01));
            portfolio.SetupGet(x => x.EndDate).Returns(new Date(2010, 12, 31));
            portfolio.SetupGet(x => x.Holdings).Returns(holdingsCollection.Object);

            var service = new PortfolioPropertiesService(portfolio.Object);

            var result = service.GetProperties();

            result.Result.Should().BeEquivalentTo(new 
            {
                Id = id,
                Name = "Test",
                StartDate = new Date(2000, 01, 01),
                EndDate = new Date(2010, 12, 31),
                Holdings = new []
                {
                    new RestApi.Portfolios.HoldingProperties()
                    {
                        Stock = new RestApi.Portfolios.Stock() { Id = stockId, AsxCode = "BHP", Name = "BHP Pty Ltd", Category = RestApi.Stocks.AssetCategory.AustralianStocks},
                        StartDate = new Date(2001, 01, 01), 
                        EndDate = new Date(2002, 01, 01),
                        ParticipatingInDrp = true
                    }
                }

            });
        }
    }
}
