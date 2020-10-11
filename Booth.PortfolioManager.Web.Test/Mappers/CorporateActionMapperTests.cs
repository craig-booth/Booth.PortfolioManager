using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Web.Mappers;
using Booth.PortfolioManager.RestApi.Stocks;
using System.Linq;
using Booth.PortfolioManager.Domain.CorporateActions;

namespace Booth.PortfolioManager.Web.Test.Mappers
{
    public class CorporateActionMapperTests
    {


        [Fact]
        public void CapitalReturnToResponse()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = mockRepository.Create<IReadOnlyStock>();
            stock.SetupGet(x => x.Id).Returns(Guid.NewGuid());

            var capitalReturn = new CapitalReturn(Guid.NewGuid(), stock.Object, new Date(2001, 01, 01), "Test", new Date(2001, 01, 15), 10.30m);

            var response = capitalReturn.ToResponse();

            response.Should().BeEquivalentTo(new
            {
                Id = capitalReturn.Id,
                Type = RestApi.CorporateActions.CorporateActionType.CapitalReturn,
                Stock = capitalReturn.Stock.Id,
                ActionDate = capitalReturn.Date,
                Description = capitalReturn.Description,
                PaymentDate = capitalReturn.PaymentDate,
                Amount = capitalReturn.Amount
            });

        }

        [Fact]
        public void CompositeActionToResponse()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());

            var id = Guid.NewGuid();
            stock.CorporateActions.StartCompositeAction(id, new Date(2001, 01, 01), "Composite Action")
                .AddCapitalReturn("Capital Return", new Date(2001, 01, 15), 10.00m)
                .AddDividend("Dividend", new Date(2001, 01, 15), 10.00m, 0.50m, 1.00m)
                .Finish();

            var action = stock.CorporateActions[id];

            var response = action.ToResponse();

            response.Should().BeEquivalentTo(new
            {
                Id = id,
                Type = RestApi.CorporateActions.CorporateActionType.CompositeAction,
                Stock = stock.Id,
                ActionDate = new Date(2001, 01, 01),
                Description = "Composite Action",
                ChildActions = new object[]
                {
                    new
                    {
                        Type = RestApi.CorporateActions.CorporateActionType.CapitalReturn,
                        Stock = stock.Id,
                        ActionDate = new Date(2001, 01, 01),
                        Description = "Capital Return",
                        PaymentDate = new Date(2001, 01, 15),
                        Amount = 10.00m
                    },
                    new
                    {
                        Type = RestApi.CorporateActions.CorporateActionType.Dividend,
                        Stock = stock.Id,
                        ActionDate = new Date(2001, 01, 01),
                        Description = "Dividend",
                        PaymentDate = new Date(2001, 01, 15),
                        Amount = 10.00m,
                        PercentFranked = 0.50m,
                        DrpPrice = 1.00m
                    }
                }
            });
        }

        [Fact]
        public void DividendToResponse()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = mockRepository.Create<IReadOnlyStock>();
            stock.SetupGet(x => x.Id).Returns(Guid.NewGuid());

            var dividend = new Dividend(Guid.NewGuid(), stock.Object, new Date(2001, 01, 01), "Test", new Date(2001, 01, 15), 10.30m, 0.30m, 1.45m);

            var response = dividend.ToResponse();

            response.Should().BeEquivalentTo(new
            {
                Id = dividend.Id,
                Type = RestApi.CorporateActions.CorporateActionType.Dividend,
                Stock = dividend.Stock.Id,
                ActionDate = dividend.Date,
                Description = dividend.Description,
                PaymentDate = dividend.PaymentDate,
                Amount = dividend.DividendAmount,
                PercentFranked = dividend.PercentFranked,
                DrpPrice = dividend.DrpPrice
            });
        }

        [Fact]
        public void SplitConsolidationToResponse()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = mockRepository.Create<IReadOnlyStock>();
            stock.SetupGet(x => x.Id).Returns(Guid.NewGuid());

            var split = new SplitConsolidation(Guid.NewGuid(), stock.Object, new Date(2001, 01, 01), "Test", 1, 2);

            var response = split.ToResponse();

            response.Should().BeEquivalentTo(new
            {
                Id = split.Id,
                Type = RestApi.CorporateActions.CorporateActionType.SplitConsolidation,
                Stock = split.Stock.Id,
                ActionDate = split.Date,
                Description = split.Description,
                OriginalUnits = 1,
                NewUnits = 2
            });
        }

        [Fact]
        public void TransformationToResponse()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = mockRepository.Create<IReadOnlyStock>();
            stock.SetupGet(x => x.Id).Returns(Guid.NewGuid());

            var stockId = Guid.NewGuid();
            var resultingStocks = new Transformation.ResultingStock[]
            {
                new Transformation.ResultingStock(stockId, 1, 2, 0.50m, new Date(2010, 01, 01))
            };
            var transformation = new Transformation(Guid.NewGuid(), stock.Object, new Date(2001, 01, 01), "Test", new Date(2001, 01, 15), 1.00m,true, resultingStocks);

            var response = transformation.ToResponse();

            response.Should().BeEquivalentTo(new
            {
                Id = transformation.Id,
                Type = RestApi.CorporateActions.CorporateActionType.Transformation,
                Stock = transformation.Stock.Id,
                ActionDate = transformation.Date,
                Description = transformation.Description,
                CashComponent = transformation.CashComponent,
                RolloverRefliefApplies = transformation.RolloverRefliefApplies,
                ResultingStocks = new []
                {
                    new {
                        Stock = stockId,
                        OriginalUnits = 1,
                        NewUnits = 2,
                        CostBase = 0.50m,
                        AquisitionDate = new Date(2010, 01, 01)
                    }
                }
            }) ;
        }

    }
}
