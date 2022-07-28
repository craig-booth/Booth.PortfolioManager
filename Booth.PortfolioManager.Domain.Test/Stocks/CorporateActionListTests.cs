using System;
using System.Linq;

using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.CorporateActions;

namespace Booth.PortfolioManager.Domain.Test.Stocks
{
    public class CorporateActionListTests
    {

        [Fact]
        public void AddCapitalReturn()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var corporateActionList = new CorporateActionList(stock);

            var id = Guid.NewGuid();
            corporateActionList.Add(new CapitalReturn(id, stock, new Date(2000, 01, 01), "Test Capital Return", new Date(2000, 02, 01), 10.00m));

            corporateActionList.Should().SatisfyRespectively(

                first => first.Should().BeEquivalentTo(new
                {
                    Id = id,
                    Stock = stock,
                    Date = new Date(2000, 01, 01),
                    Description = "Test Capital Return",
                    PaymentDate = new Date(2000, 02, 01),
                    Amount = 10.00m
                })

            );
        }

        [Fact]
        public void AddCapitalReturnWithBlankDescription()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var corporateActionList = new CorporateActionList(stock);

            var id = Guid.NewGuid();
            corporateActionList.Add(new CapitalReturn(id, stock, new Date(2000, 01, 01), "", new Date(2000, 02, 01), 10.00m));

            corporateActionList.Should().SatisfyRespectively(

                first => first.Should().BeEquivalentTo(new
                {
                    Id = id,
                    Stock = stock,
                    Date = new Date(2000, 01, 01),
                    Description = "Capital Return $10.00",
                    PaymentDate = new Date(2000, 02, 01),
                    Amount = 10.00m
                })

            );
        }


        [Fact]
        public void AddCompositeActionWithoutChildren()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var corporateActionList = new CorporateActionList(stock);

            var id = Guid.NewGuid();
            corporateActionList.Add(new CompositeAction(id, stock, new Date(2000, 01, 01), "Test Composite Action", new ICorporateAction[] { }));

            corporateActionList.Should().SatisfyRespectively(

                first =>
                {
                    first.Should().BeEquivalentTo(new
                    {
                        Id = id,
                        Stock = stock,
                        Date = new Date(2000, 01, 01),
                        Description = "Test Composite Action"
                    });
                    first.Should().BeOfType<CompositeAction>().Which.ChildActions.Should().BeEmpty();
                }

            );
        }

        [Fact]
        public void AddCompositeActionWithBlankDescription()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var corporateActionList = new CorporateActionList(stock);

            var id = Guid.NewGuid();
            corporateActionList.Add(new CompositeAction(id, stock, new Date(2000, 01, 01), "", new ICorporateAction[] { }));

            corporateActionList.Should().SatisfyRespectively(

                first =>
                {
                    first.Should().BeEquivalentTo(new
                    {
                        Id = id,
                        Stock = stock,
                        Date = new Date(2000, 01, 01),
                        Description = "Complex corporate action"
                    });
                    first.Should().BeOfType<CompositeAction>().Which.ChildActions.Should().BeEmpty();
                }

            );
        }

        [Fact]
        public void AddCompositeActionChildActions()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var corporateActionList = new CorporateActionList(stock);

            var id = Guid.NewGuid();
            corporateActionList.Add(new CompositeAction(id, stock, new Date(2000, 01, 01), "Test CompositeAction", new ICorporateAction[] {
                new CapitalReturn(id, stock, new Date(2000, 01, 01),"Capital Return", new Date(2000, 02, 01), 10.00m),
                new SplitConsolidation(id, stock, new Date(2000, 01, 01), "Split",  1, 2)
            }));


            corporateActionList.Should().SatisfyRespectively(

                first =>
                {
                    first.Should().BeEquivalentTo(new
                    {
                        Id = id,
                        Stock = stock,
                        Date = new Date(2000, 01, 01),
                        Description = "Test CompositeAction"
                    });
                    first.Should().BeOfType<CompositeAction>().Which.ChildActions.Should().SatisfyRespectively(
                        child1 => child1.Should().BeOfType<CapitalReturn>().Which.Amount.Should().Be(10.00m),
                        child1 => child1.Should().BeOfType<SplitConsolidation>().Which.NewUnits.Should().Be(2)
                        );
                }

            );
        }

        [Fact]
        public void AddDividend()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var corporateActionList = new CorporateActionList(stock);

            var id = Guid.NewGuid();
            corporateActionList.Add(new Dividend(id, stock, new Date(2000, 01, 01), "Test Dividend", new Date(2000, 02, 01), 1.20m, 1.00m, 2.50m));


            corporateActionList.Should().SatisfyRespectively(

                first => first.Should().BeEquivalentTo(new
                {
                    Id = id,
                    Stock = stock,
                    Date = new Date(2000, 01, 01),
                    Description = "Test Dividend",
                    PaymentDate = new Date(2000, 02, 01),
                    DividendAmount = 1.20m,
                    PercentFranked = 1.00m,
                    DrpPrice = 2.50m
                })

            );
        }

        [Fact]
        public void AddDividendWithBlankDescription()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var corporateActionList = new CorporateActionList(stock);

            var id = Guid.NewGuid();
            corporateActionList.Add(new Dividend(id, stock, new Date(2000, 01, 01), "", new Date(2000, 02, 01), 1.20m, 1.00m, 2.50m));

            corporateActionList.Should().SatisfyRespectively(

                first => first.Should().BeEquivalentTo(new
                {
                    Id = id,
                    Stock = stock,
                    Date = new Date(2000, 01, 01),
                    Description = "Dividend $1.20",
                    PaymentDate = new Date(2000, 02, 01),
                    DividendAmount = 1.20m,
                    PercentFranked = 1.00m,
                    DrpPrice = 2.50m
                })

            );
        }

        [Fact]
        public void AddSplitConsolidation()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var corporateActionList = new CorporateActionList(stock);

            var id = Guid.NewGuid();
            corporateActionList.Add(new SplitConsolidation(id, stock, new Date(2000, 01, 01), "Test Split", 1, 2));

            corporateActionList.Should().SatisfyRespectively(

                first => first.Should().BeEquivalentTo(new
                {
                    Id = id,
                    Stock = stock,
                    Date = new Date(2000, 01, 01),
                    Description = "Test Split",
                    OriginalUnits = 1,
                    NewUnits = 2
                })

            );
        }

        [Fact]
        public void AddSplitConsolidationithBlankDescription()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var corporateActionList = new CorporateActionList(stock);

            var id = Guid.NewGuid();
            corporateActionList.Add(new SplitConsolidation(id, stock, new Date(2000, 01, 01), "", 1, 2));

           corporateActionList.Should().SatisfyRespectively(

                first => first.Should().BeEquivalentTo(new
                {
                    Id = id,
                    Stock = stock,
                    Date = new Date(2000, 01, 01),
                    Description = "1 for 2 Stock Split",
                    OriginalUnits = 1,
                    NewUnits = 2
                })

            );
        }


        [Fact]
        public void AddTransformation()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "XYZ Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var corporateActionList = new CorporateActionList(stock);

            var id = Guid.NewGuid();
            var resultStocks = new Transformation.ResultingStock[] {
                new Transformation.ResultingStock(stock2.Id, 1, 2, 0.40m, new Date(2020, 02, 01))
            };
            corporateActionList.Add(new Transformation(id, stock, new Date(2000, 01, 01), "Test Transformation", new Date(2000, 02, 01), 1.20m, false, resultStocks));

            corporateActionList.Should().SatisfyRespectively(

                first => {
                    first.Should().BeEquivalentTo(new
                    {
                        Id = id,
                        Stock = stock,
                        Date = new Date(2000, 01, 01),
                        Description = "Test Transformation",
                        CashComponent = 1.20M,
                        RolloverRefliefApplies = false
                    });
                    first.Should().BeOfType<Transformation>().Which.ResultingStocks.Should().HaveCount(1);
                }
            );
        }

        [Fact]
        public void AddTransformationWithBlankDescription()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "XYZ Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var corporateActionList = new CorporateActionList(stock);

            var id = Guid.NewGuid();
            var resultStocks = new Transformation.ResultingStock[] {
                new Transformation.ResultingStock(stock2.Id, 1, 2, 0.40m, new Date(2020, 02, 01))
            };
            corporateActionList.Add(new Transformation(id, stock, new Date(2000, 01, 01), "", new Date(2000, 02, 01), 1.20m, false, resultStocks));

            corporateActionList.Should().SatisfyRespectively(

                first =>
                {
                    first.Should().BeEquivalentTo(new
                    {
                        Id = id,
                        Stock = stock,
                        Date = new Date(2000, 01, 01),
                        Description = "Transformation",
                        CashComponent = 1.20M,
                        RolloverRefliefApplies = false
                    });
                    first.Should().BeOfType<Transformation>().Which.ResultingStocks.Should().HaveCount(1);
                }
            );

        }

    }
}
