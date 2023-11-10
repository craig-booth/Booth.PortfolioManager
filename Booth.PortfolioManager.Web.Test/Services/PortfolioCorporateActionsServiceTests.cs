using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using FluentAssertions;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Web.Utilities;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.RestApi.Portfolios;
using Booth.PortfolioManager.Web.Mappers;

namespace Booth.PortfolioManager.Web.Test.Services
{
    [Collection(Services.Collection)]
    public class PortfolioCorporateActionsServiceTests
    {
      
        private readonly ServicesTestFixture _Fixture;

        public PortfolioCorporateActionsServiceTests(ServicesTestFixture fixture)
        {
            _Fixture = fixture;
        }

        [Fact]
        public void PortfolioNotFound()
        {
            var service = new PortfolioCorporateActionsService(null, _Fixture.StockResolver, _Fixture.TransactionMapper, _Fixture.StockMapper);

            var result = service.GetCorporateActions();

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetCorporateActions()
        {
            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioCorporateActionsService(portfolio, _Fixture.StockResolver, _Fixture.TransactionMapper, _Fixture.StockMapper);

            var result = service.GetCorporateActions();

            result.Result.Should().BeEquivalentTo(new
            {
                CorporateActions = new[]
                {
                    new CorporateActionsResponse.CorporateActionItem() { Id = _Fixture.ARG_CapitalReturn, ActionDate = new Date(2001, 01, 01), Stock = _Fixture.Stock_ARG, Description = "ARG Capital Return" },
                    new CorporateActionsResponse.CorporateActionItem() { Id = _Fixture.WAM_Split, ActionDate = new Date(2002, 01, 01), Stock = _Fixture.Stock_WAM, Description = "WAM Split" },
                }

            });
        }


        [Fact]
        public void GetCorporateActionsStockNotOwned()
        {
            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioCorporateActionsService(portfolio, _Fixture.StockResolver, _Fixture.TransactionMapper, _Fixture.StockMapper);

            var result = service.GetCorporateActions(Guid.NewGuid());

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetCorporateActionsForStock()
        {
            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioCorporateActionsService(portfolio, _Fixture.StockResolver, _Fixture.TransactionMapper, _Fixture.StockMapper);

            var result = service.GetCorporateActions(_Fixture.Stock_WAM.Id);

            result.Result.Should().BeEquivalentTo(new
            {
                CorporateActions = new[]
                {
                   new CorporateActionsResponse.CorporateActionItem() { Id = _Fixture.WAM_Split, ActionDate = new Date(2002, 01, 01), Stock = _Fixture.Stock_WAM, Description = "WAM Split" },
                }

            });
        }


        [Fact]
        public void GetTransactionsForCorporateActionStockNotOwned()
        {
            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioCorporateActionsService(portfolio, _Fixture.StockResolver, _Fixture.TransactionMapper, _Fixture.StockMapper);

            var result = service.GetTransactionsForCorporateAction(Guid.NewGuid(), Guid.NewGuid());

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetTransactionsForCorporateActionActionNotFound()
        {
            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioCorporateActionsService(portfolio, _Fixture.StockResolver, _Fixture.TransactionMapper, _Fixture.StockMapper);

            var result = service.GetTransactionsForCorporateAction(Guid.NewGuid(), Guid.NewGuid());

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetTransactionsForCorporateAction()
        {
            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioCorporateActionsService(portfolio, _Fixture.StockResolver, _Fixture.TransactionMapper, _Fixture.StockMapper);

            var result = service.GetTransactionsForCorporateAction(_Fixture.Stock_WAM.Id, _Fixture.WAM_Split);

            result.Result.Should().BeEquivalentTo(new []
            {
                new RestApi.Transactions.UnitCountAdjustment()
                {
                    Stock = _Fixture.Stock_WAM.Id,
                    TransactionDate = new Date(2002, 01, 01),
                    Description = "Adjust unit count using ratio 1:2",
                    Comment= "WAM Split",
                    OriginalUnits = 1,
                    NewUnits = 2
                }
            }, options => options.Excluding(x => x.Id));
        } 
    }

}
