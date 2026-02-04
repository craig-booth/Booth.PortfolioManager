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
using Booth.PortfolioManager.Domain.CorporateActions;
using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Web.Test.Mappers
{
    public class CorporateActionMapperTests
    {


        [Fact]
        public void CapitalReturnToApi()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);
         
            var capitalReturn = new CapitalReturn(Guid.NewGuid(), stock, new Date(2001, 01, 01), "Test", new Date(2001, 01, 15), 10.30m);

            var mapper = new CorporateActionMapper(stockResolver.Object);
            var response = mapper.ToApi(capitalReturn);

            response.Should().BeEquivalentTo(new
            {
                Id = capitalReturn.Id,
                Type = Models.CorporateAction.CorporateActionType.CapitalReturn,
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
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var capitalReturn = new Models.CorporateAction.CapitalReturn()
            {
                Id = Guid.NewGuid(),
                Stock = stock.Id,
                ActionDate = new Date(2001, 01, 01),
                Description = "Test",
                PaymentDate = new Date(2001, 01, 15),
                Amount = 10.30m
            };

            var mapper = new CorporateActionMapper(stockResolver.Object);
            var response = mapper.FromApi(capitalReturn);

            response.Should().BeEquivalentTo(new
            {
                Id = capitalReturn.Id,
                Stock = stock,
                Date = capitalReturn.ActionDate,
                Description = capitalReturn.Description,
                PaymentDate = capitalReturn.PaymentDate,
                Amount = capitalReturn.Amount
            });

        }

        [Fact]
        public void CompositeActionToApi()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var id = Guid.NewGuid();
            var action = new CompositeAction(id, stock, new Date(2001, 01, 01), "Composite Action", new ICorporateAction[]
            {
                new CapitalReturn(id, stock, new Date(2001, 01, 01), "Capital Return", new Date(2001, 01, 15), 10.00m),
                new Dividend(id, stock, new Date(2001, 01, 01), "Dividend", new Date(2001, 01, 15), 10.00m, 0.50m, 1.00m)
            });

            var mapper = new CorporateActionMapper(stockResolver.Object);
            var response = mapper.ToApi(action);

            response.Should().BeEquivalentTo(new
            {
                Id = id,
                Type = Models.CorporateAction.CorporateActionType.CompositeAction,
                Stock = stock.Id,
                ActionDate = new Date(2001, 01, 01),
                Description = "Composite Action",
                ChildActions = new object[]
                {
                    new
                    {
                        Type = Models.CorporateAction.CorporateActionType.CapitalReturn,
                        Stock = stock.Id,
                        ActionDate = new Date(2001, 01, 01),
                        Description = "Capital Return",
                        PaymentDate = new Date(2001, 01, 15),
                        Amount = 10.00m
                    },
                    new
                    {
                        Type = Models.CorporateAction.CorporateActionType.Dividend,
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
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var id = Guid.NewGuid();
            var compositeAction = new Models.CorporateAction.CompositeAction()
            {
                Id = id,
                Stock = stock.Id,
                ActionDate = new Date(2001, 01, 01),
                Description = "Composite Action",
            };
            compositeAction.ChildActions.Add(new Models.CorporateAction.CapitalReturn()
            {
                Id = id,
                Stock = stock.Id,
                ActionDate = new Date(2001, 01, 01),
                Description = "Capital Return",
                PaymentDate = new Date(2001, 01, 15),
                Amount = 10.00m
            });
            compositeAction.ChildActions.Add(new Models.CorporateAction.Dividend()
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


            var mapper = new CorporateActionMapper(stockResolver.Object);
            var response = mapper.FromApi(compositeAction);

            response.Should().BeEquivalentTo(new
            {
                Id = id,
                Stock = stock,
                Date = new Date(2001, 01, 01),
                Description = "Composite Action",
                ChildActions = new object[]
                {
                    new
                    {
                        Stock = stock,
                        Date = new Date(2001, 01, 01),
                        Description = "Capital Return",
                        PaymentDate = new Date(2001, 01, 15),
                        Amount = 10.00m
                    },
                    new
                    {
                        Stock = stock,
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
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var dividend = new Dividend(Guid.NewGuid(), stock, new Date(2001, 01, 01), "Test", new Date(2001, 01, 15), 10.30m, 0.30m, 1.45m);

            var mapper = new CorporateActionMapper(stockResolver.Object);
            var response = mapper.ToApi(dividend);

            response.Should().BeEquivalentTo(new
            {
                Id = dividend.Id,
                Type = Models.CorporateAction.CorporateActionType.Dividend,
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
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var dividend = new Models.CorporateAction.Dividend()
            {
                Id = Guid.NewGuid(),
                Stock = stock.Id,
                ActionDate = new Date(2001, 01, 01),
                Description = "Test",
                PaymentDate = new Date(2001, 01, 15),
                Amount = 10.30m,
                PercentFranked = 0.50m,
                DrpPrice = 40.45m
            };

            var mapper = new CorporateActionMapper(stockResolver.Object);
            var response = mapper.FromApi(dividend);

            response.Should().BeEquivalentTo(new
            {
                Id = dividend.Id,
                Stock = stock,
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
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var split = new SplitConsolidation(Guid.NewGuid(), stock, new Date(2001, 01, 01), "Test", 1, 2);

            var mapper = new CorporateActionMapper(stockResolver.Object);
            var response = mapper.ToApi(split);

            response.Should().BeEquivalentTo(new
            {
                Id = split.Id,
                Type = Models.CorporateAction.CorporateActionType.SplitConsolidation,
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
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var split = new Models.CorporateAction.SplitConsolidation()
            {
                Id = Guid.NewGuid(),
                Stock = stock.Id,
                ActionDate = new Date(2001, 01, 01),
                Description = "Test",
                OriginalUnits = 1,
                NewUnits = 2
            };

            var mapper = new CorporateActionMapper(stockResolver.Object);
            var response = mapper.FromApi(split);

            response.Should().BeEquivalentTo(new
            {
                Id = split.Id,
                Stock = stock,
                Date = split.ActionDate,
                Description = split.Description,
                OriginalUnits = 1,
                NewUnits = 2
            });
        }

        [Fact]
        public void TransformationToApi()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var stockId = Guid.NewGuid();
            var resultingStocks = new Transformation.ResultingStock[]
            {
                new Transformation.ResultingStock(stockId, 1, 2, 0.50m, new Date(2010, 01, 01))
            };
            var transformation = new Transformation(Guid.NewGuid(), stock, new Date(2001, 01, 01), "Test", new Date(2001, 01, 15), 1.00m,true, resultingStocks);

            var mapper = new CorporateActionMapper(stockResolver.Object);
            var response = mapper.ToApi(transformation);

            response.Should().BeEquivalentTo(new
            {
                Id = transformation.Id,
                Type = Models.CorporateAction.CorporateActionType.Transformation,
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
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            var stock2 = new Stock(Guid.NewGuid());

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);
            stockResolver.Setup(x => x.GetStock(stock2.Id)).Returns(stock2);

            var transformation = new Models.CorporateAction.Transformation()
            {
                Id = Guid.NewGuid(),
                Stock = stock.Id,
                ActionDate = new Date(2001, 01, 01),
                Description = "Test",
                ImplementationDate = new Date(2002, 02, 01),
                CashComponent = 6.50m,
            };
            transformation.ResultingStocks.Add(new Models.CorporateAction.Transformation.ResultingStock()
            {
                Stock = stock2.Id,
                OriginalUnits = 1,
                NewUnits = 2,
                CostBase = 0.50m,
                AquisitionDate = new Date(2010, 01, 01)
            });

            var mapper = new CorporateActionMapper(stockResolver.Object);
            var response = mapper.FromApi(transformation);

            response.Should().BeEquivalentTo(new
            {
                Id = transformation.Id,
                Stock = stock,
                Date = transformation.ActionDate,
                Description = transformation.Description,
                ImplementationDate = transformation.ImplementationDate,
                CashComponent = transformation.CashComponent,
                ResultingStocks = new[]
                {
                    new {
                        Stock = stock2.Id,
                        OriginalUnits = 1,
                        NewUnits = 2,
                        CostBasePercentage = 0.50m,
                        AquisitionDate = new Date(2010, 01, 01)
                    },

                }
            });
        }

    }
}
