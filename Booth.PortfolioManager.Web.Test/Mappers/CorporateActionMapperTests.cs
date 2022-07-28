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
        public void CapitalReturnToApi()
        {
            var stock = new Stock(Guid.NewGuid());

            var capitalReturn = new CapitalReturn(Guid.NewGuid(), stock, new Date(2001, 01, 01), "Test", new Date(2001, 01, 15), 10.30m);

            var response = capitalReturn.ToApi();

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
        public void CapitalReturnFromApi()
        {
            var capitalReturn = new RestApi.CorporateActions.CapitalReturn()
            {
                Id = Guid.NewGuid(),
                Stock = Guid.NewGuid(),
                ActionDate = new Date(2001, 01, 01),
                Description = "Test",
                PaymentDate = new Date(2001, 01, 15),
                Amount = 10.30m
            };

            var response = capitalReturn.FromApi();

            response.Should().BeEquivalentTo(new
            {
                Id = capitalReturn.Id,
                Date = capitalReturn.ActionDate,
                Description = capitalReturn.Description,
                PaymentDate = capitalReturn.PaymentDate,
                Amount = capitalReturn.Amount
            });

        }

        [Fact]
        public void CompositeActionToApi()
        {
            var stock = new Stock(Guid.NewGuid());

            var id = Guid.NewGuid();
            stock.CorporateActions.Add(new CompositeAction(id, stock, new Date(2001, 01, 01), "Composite Action", new ICorporateAction[]
            {
                new CapitalReturn(id, stock, new Date(2001, 01, 01), "Capital Return", new Date(2001, 01, 15), 10.00m),
                new Dividend(id, stock, new Date(2001, 01, 01), "Dividend", new Date(2001, 01, 15), 10.00m, 0.50m, 1.00m)
            }));


            var action = stock.CorporateActions[id];

            var response = action.ToApi();

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
        public void CompositeActionFromApi()
        {
            var stock = new Stock(Guid.NewGuid());

            var id = Guid.NewGuid();
            var compositeAction = new RestApi.CorporateActions.CompositeAction()
            {
                Id = id,
                Stock = Guid.NewGuid(),
                ActionDate = new Date(2001, 01, 01),
                Description = "Composite Action",
            };
            compositeAction.ChildActions.Add(new RestApi.CorporateActions.CapitalReturn()
            {
                Id = id,
                Stock = stock.Id,
                ActionDate = new Date(2001, 01, 01),
                Description = "Capital Return",
                PaymentDate = new Date(2001, 01, 15),
                Amount = 10.00m
            });
            compositeAction.ChildActions.Add(new RestApi.CorporateActions.Dividend()
            {
                Id = id,
                Stock = stock.Id,
                ActionDate = new Date(2001, 01, 01),
                Description = "Dividend",
                PaymentDate = new Date(2001, 01, 15),
                Amount = 10.00m,
                PercentFranked = 0.50m,
                DrpPrice = 1.00m
            });


            var response = compositeAction.FromApi();

            response.Should().BeEquivalentTo(new
            {
                Id = id,
                Date = new Date(2001, 01, 01),
                Description = "Composite Action",
                ChildActions = new object[]
                {
                    new
                    {
                        Date = new Date(2001, 01, 01),
                        Description = "Capital Return",
                        PaymentDate = new Date(2001, 01, 15),
                        Amount = 10.00m
                    },
                    new
                    {
                        Date = new Date(2001, 01, 01),
                        Description = "Dividend",
                        PaymentDate = new Date(2001, 01, 15),
                        DividendAmount = 10.00m,
                        PercentFranked = 0.50m,
                        DrpPrice = 1.00m
                    }
                }
            });
        }

        [Fact]
        public void DividendToApi()
        {
            var stock = new Stock(Guid.NewGuid());

            var dividend = new Dividend(Guid.NewGuid(), stock, new Date(2001, 01, 01), "Test", new Date(2001, 01, 15), 10.30m, 0.30m, 1.45m);

            var response = dividend.ToApi();

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
        public void DividendFromApi()
        {
            var dividend = new RestApi.CorporateActions.Dividend()
            {
                Id = Guid.NewGuid(),
                Stock = Guid.NewGuid(),
                ActionDate = new Date(2001, 01, 01),
                Description = "Test",
                PaymentDate = new Date(2001, 01, 15),
                Amount = 10.30m,
                PercentFranked = 0.50m,
                DrpPrice = 40.45m
            };

            var response = dividend.FromApi();

            response.Should().BeEquivalentTo(new
            {
                Id = dividend.Id,
                Date = dividend.ActionDate,
                Description = dividend.Description,
                PaymentDate = dividend.PaymentDate,
                DividendAmount = dividend.Amount,
                PercentFranked = 0.50m,
                DrpPrice = 40.45m
            });
        }

        [Fact]
        public void SplitConsolidationToApi()
        {
            var stock = new Stock(Guid.NewGuid());

            var split = new SplitConsolidation(Guid.NewGuid(), stock, new Date(2001, 01, 01), "Test", 1, 2);

            var response = split.ToApi();

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
        public void SplitConsolidationFromApi()
        {
            var split = new RestApi.CorporateActions.SplitConsolidation()
            {
                Id = Guid.NewGuid(),
                Stock = Guid.NewGuid(),
                ActionDate = new Date(2001, 01, 01),
                Description = "Test",
                OriginalUnits = 1,
                NewUnits = 2
            };

            var response = split.FromApi();

            response.Should().BeEquivalentTo(new
            {
                Id = split.Id,
                Date = split.ActionDate,
                Description = split.Description,
                OriginalUnits = 1,
                NewUnits = 2
            });
        }

        [Fact]
        public void TransformationToApi()
        {
            var stock = new Stock(Guid.NewGuid());

            var stockId = Guid.NewGuid();
            var resultingStocks = new Transformation.ResultingStock[]
            {
                new Transformation.ResultingStock(stockId, 1, 2, 0.50m, new Date(2010, 01, 01))
            };
            var transformation = new Transformation(Guid.NewGuid(), stock, new Date(2001, 01, 01), "Test", new Date(2001, 01, 15), 1.00m,true, resultingStocks);

            var response = transformation.ToApi();

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

        [Fact]
        public void TransformationFromApi()
        {
            var stockId = Guid.NewGuid();

            var transformation = new RestApi.CorporateActions.Transformation()
            {
                Id = Guid.NewGuid(),
                Stock = Guid.NewGuid(),
                ActionDate = new Date(2001, 01, 01),
                Description = "Test",
                ImplementationDate = new Date(2002, 02, 01),
                CashComponent = 6.50m,
            };
            transformation.ResultingStocks.Add(new RestApi.CorporateActions.Transformation.ResultingStock()
            {
                Stock = stockId,
                OriginalUnits = 1,
                NewUnits = 2,
                CostBase = 0.50m,
                AquisitionDate = new Date(2010, 01, 01)
            });

            var response = transformation.FromApi();

            response.Should().BeEquivalentTo(new
            {
                Id = transformation.Id,
                Date = transformation.ActionDate,
                Description = transformation.Description,
                ImplementationDate = transformation.ImplementationDate,
                CashComponent = transformation.CashComponent,
                ResultingStocks = new[]
                {
                    new {
                        Stock = stockId,
                        OriginalUnits = 1,
                        NewUnits = 2,
                        CostBasePercentage = 0.50m,
                        AquisitionDate = new Date(2010, 01, 01)
                    }
                }
            });
        }

    }
}
