using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using FluentAssertions;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Web.Test.Services
{/*
    public class PortfolioSummaryServiceTests
    {
        [Fact]
        public void PortfolioNotFound()
        {
            var service = new PortfolioSummaryService(null);

            var result = service.GetSummary(new Date(2000, 01, 01));

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetSummary()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            //  var stockProperties = mockRepository.Create<IEffectiveProperties<StockProperties>>();
            //  stockProperties.Setup(x => x.ClosestTo(Date.Today)).Returns(new StockProperties("BHP", "BHP Pty Ltd", AssetCategory.AustralianStocks));

            var stockId = Guid.NewGuid();
            var stock = mockRepository.Create<IReadOnlyStock>();
            //           stock.SetupGet(x => x.Id).Returns(stockId);
            //           stock.SetupGet(x => x.Properties).Returns(stockProperties.Object);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            //          holding.SetupGet(x => x.Stock).Returns(stock.Object);
            //          holding.SetupGet(x => x.EffectivePeriod).Returns(new DateRange(new Date(2001, 01, 01), new Date(2002, 01, 01)));
            //          holding.SetupGet(x => x.Settings).Returns(new HoldingSettings(true));

            var holdingsCollection = mockRepository.Create<IHoldingCollection>();
            holdingsCollection.Setup(x => x.All()).Returns(new[] { holding.Object });

            var cashAccount = mockRepository.Create<IReadOnlyCashAccount>();
            cashAccount.Setup(x => x.Balance(new Date(2000, 01, 01))).Returns(120.00m);

     //       var id = Guid.NewGuid();
            var portfolio = mockRepository.Create<IReadOnlyPortfolio>();
            portfolio.SetupGet(x => x.CashAccount).Returns(cashAccount.Object);
            portfolio.SetupGet(x => x.Holdings).Returns(holdingsCollection.Object);

            var service = new PortfolioSummaryService(portfolio.Object);

            var result = service.GetSummary(new Date(2000, 01, 01));

            result.Result.Should().BeEquivalentTo(new
            {
                PortfolioValue = 0.00m,
                PortfolioCost = 0.00m,
                Return1Year = 0.00m,
                Return3Year = 0.00m,
                Return5Year = 0.00m,
                ReturnAll = 0.00m,
                CashBalance = 120.00m,
                Holdings = new[]
                {
                    new RestApi.Portfolios.Holding()
                    {
                        Stock = new RestApi.Portfolios.Stock() { Id = stockId, AsxCode = "BHP", Name = "BHP Pty Ltd", Category = RestApi.Stocks.AssetCategory.AustralianStocks},
                        Units = 0,
                        Value = 0,
                        Cost = 0.00m,
                        CostBase = 0.00m
                    }
                }

            });

    }
    } */
}
