using System;
using System.Linq;

using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;

using Booth.Common;
using Booth.EventStore;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.CorporateActions;
using Booth.PortfolioManager.Domain.CorporateActions.Events;
using System.Runtime.ExceptionServices;

namespace Booth.PortfolioManager.Domain.Test.Stocks
{
    public class CorporateActionListTests
    {

        [Fact]
        public void AddCapitalReturn()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            events.Setup(x => x.Add(It.IsAny<CapitalReturnAddedEvent>())).Verifiable();

            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            corporateActionList.AddCapitalReturn(id, new Date(2000, 01, 01), "Test Capital Return", new Date(2000, 02, 01), 10.00m);

            corporateActionList.Should().SatisfyRespectively(

                first => first.Should().BeEquivalentTo(new
                {
                    Id = id,
                    Stock = stock,
                    Date = new Date(2000, 01, 01),
                    Type = CorporateActionType.CapitalReturn,
                    Description = "Test Capital Return",
                    PaymentDate = new Date(2000, 02, 01),
                    Amount = 10.00m
                })

            );
         
            mockRepository.Verify();
        }

        [Fact]
        public void AddCapitalReturnWithBlankDescription()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            events.Setup(x => x.Add(It.IsAny<CapitalReturnAddedEvent>())).Verifiable();

            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            corporateActionList.AddCapitalReturn(id, new Date(2000, 01, 01), "", new Date(2000, 02, 01), 10.00m);

            corporateActionList.Should().SatisfyRespectively(

                first => first.Should().BeEquivalentTo(new
                {
                    Id = id,
                    Stock = stock,
                    Date = new Date(2000, 01, 01),
                    Type = CorporateActionType.CapitalReturn,
                    Description = "Capital Return $10.00",
                    PaymentDate = new Date(2000, 02, 01),
                    Amount = 10.00m
                })

            );

            mockRepository.Verify();
        }

        [Fact]
        public void ApplyCapitalReturnAddedEvent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            var @event = new CapitalReturnAddedEvent(stock.Id, 0, id, new Date(2000, 01, 01), "Test Capital Return", new Date(2000, 02, 01), 10.00m);

            corporateActionList.Apply(@event);

            corporateActionList.Should().SatisfyRespectively(

                first => first.Should().BeEquivalentTo(new
                {
                    Id = id,
                    Stock = stock,
                    Date = new Date(2000, 01, 01),
                    Type = CorporateActionType.CapitalReturn,
                    Description = "Test Capital Return",
                    PaymentDate = new Date(2000, 02, 01),
                    Amount = 10.00m
                })

            );

            mockRepository.Verify();
        }

        [Fact]
        public void AddCompositeActionWithoutChildren()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            events.Setup(x => x.Add(It.IsAny<CompositeActionAddedEvent>())).Verifiable();

            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            corporateActionList.StartCompositeAction(id, new Date(2000, 01, 01), "Test Composite Action").Finish();

            corporateActionList.Should().SatisfyRespectively(

                first =>
                {
                    first.Should().BeEquivalentTo(new
                    {
                        Id = id,
                        Stock = stock,
                        Date = new Date(2000, 01, 01),
                        Type = CorporateActionType.Composite,
                        Description = "Test Composite Action"
                    });
                    first.Should().BeOfType<CompositeAction>().Which.ChildActions.Should().BeEmpty();
                }

            );

            mockRepository.Verify(); 
        }

        [Fact]
        public void AddCompositeActionWithBlankDescription()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            events.Setup(x => x.Add(It.IsAny<CompositeActionAddedEvent>())).Verifiable();

            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            corporateActionList.StartCompositeAction(id, new Date(2000, 01, 01), "").Finish();

            corporateActionList.Should().SatisfyRespectively(

                first =>
                {
                    first.Should().BeEquivalentTo(new
                    {
                        Id = id,
                        Stock = stock,
                        Date = new Date(2000, 01, 01),
                        Type = CorporateActionType.Composite,
                        Description = "Complex corporate action"
                    });
                    first.Should().BeOfType<CompositeAction>().Which.ChildActions.Should().BeEmpty();
                }

            );

            mockRepository.Verify();
        }

        [Fact]
        public void AddCompositeActionChildActions()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            events.Setup(x => x.Add(It.IsAny<CompositeActionAddedEvent>())).Verifiable();

            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            corporateActionList.StartCompositeAction(id, new Date(2000, 01, 01), "Test CompositeAction")
                .AddCapitalReturn("Capital Return", new Date(2000, 02, 01), 10.00m)
                .AddSplitConsolidation("Split", 1, 2)
                .Finish();

            corporateActionList.Should().SatisfyRespectively(

                first =>
                {
                    first.Should().BeEquivalentTo(new
                    {
                        Id = id,
                        Stock = stock,
                        Date = new Date(2000, 01, 01),
                        Type = CorporateActionType.Composite,
                        Description = "Test CompositeAction"
                    });
                    first.Should().BeOfType<CompositeAction>().Which.ChildActions.Should().SatisfyRespectively(
                        child1 => child1.Should().BeOfType<CapitalReturn>().Which.Amount.Should().Be(10.00m),
                        child1 => child1.Should().BeOfType<SplitConsolidation>().Which.NewUnits.Should().Be(2)
                        );
                }

            );

            mockRepository.Verify();
        }

        [Fact]
        public void ApplyCompositeActionAddedEvent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            var @event = new CompositeActionAddedEvent(stock.Id, 0, id, new Date(2000, 01, 01), "Test Complex Action");
            @event.ChildActions.Add(new SplitConsolidationAddedEvent(stock.Id, 0, id, new Date(2000, 01, 01), "Test Split", 1, 2));

            corporateActionList.Apply(@event);

            corporateActionList.Should().SatisfyRespectively(

                first =>
                {
                    first.Should().BeEquivalentTo(new
                    {
                        Id = id,
                        Stock = stock,
                        Date = new Date(2000, 01, 01),
                        Type = CorporateActionType.Composite,
                        Description = "Test Complex Action"
                    });
                    first.Should().BeOfType<CompositeAction>().Which.ChildActions.Should().SatisfyRespectively(
                        child1 => child1.Should().BeOfType<SplitConsolidation>().Which.Should().BeEquivalentTo(new 
                        { 
                            Date = new Date(2000, 01, 01), 
                            Type = CorporateActionType.SplitConsolidation,
                            Description = "Test Split",
                            OriginalUnits = 1,
                            NewUnits = 2
                        })
                    );
                }

            );

            mockRepository.Verify();
        }

        [Fact]
        public void AddDividend()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            events.Setup(x => x.Add(It.IsAny<DividendAddedEvent>())).Verifiable();

            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            corporateActionList.AddDividend(id, new Date(2000, 01, 01), "Test Dividend", new Date(2000, 02, 01), 1.20m, 1.00m, 2.50m);


            corporateActionList.Should().SatisfyRespectively(

                first => first.Should().BeEquivalentTo(new
                {
                    Id = id,
                    Stock = stock,
                    Date = new Date(2000, 01, 01),
                    Type = CorporateActionType.Dividend,
                    Description = "Test Dividend",
                    PaymentDate = new Date(2000, 02, 01),
                    DividendAmount = 1.20m,
                    PercentFranked = 1.00m,
                    DrpPrice = 2.50m
                })

            );

            mockRepository.Verify();
        }

        [Fact]
        public void AddDividendWithBlankDescription()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            events.Setup(x => x.Add(It.IsAny<DividendAddedEvent>())).Verifiable();

            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            corporateActionList.AddDividend(id, new Date(2000, 01, 01), "", new Date(2000, 02, 01), 1.20m, 1.00m, 2.50m);

            corporateActionList.Should().SatisfyRespectively(

                first => first.Should().BeEquivalentTo(new
                {
                    Id = id,
                    Stock = stock,
                    Date = new Date(2000, 01, 01),
                    Type = CorporateActionType.Dividend,
                    Description = "Dividend $1.20",
                    PaymentDate = new Date(2000, 02, 01),
                    DividendAmount = 1.20m,
                    PercentFranked = 1.00m,
                    DrpPrice = 2.50m
                })

            );

            mockRepository.Verify();
        }

        [Fact]
        public void ApplyDividendAddedEvent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            var @event = new DividendAddedEvent(stock.Id, 0, id, new Date(2000, 01, 01), "Test Dividend", new Date(2000, 02, 01), 1.20m, 1.00m, 2.20m);

            corporateActionList.Apply(@event);

            corporateActionList.Should().SatisfyRespectively(

                first => first.Should().BeEquivalentTo(new
                {
                    Id = id,
                    Stock = stock,
                    Date = new Date(2000, 01, 01),
                    Type = CorporateActionType.Dividend,
                    Description = "Test Dividend",
                    PaymentDate = new Date(2000, 02, 01),
                    DividendAmount = 1.20m,
                    PercentFranked = 1.00m,
                    DrpPrice = 2.20m
                })

            );

            mockRepository.Verify();
        }

        [Fact]
        public void AddSplitConsolidation()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            events.Setup(x => x.Add(It.IsAny<SplitConsolidationAddedEvent>())).Verifiable();

            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            corporateActionList.AddSplitConsolidation(id, new Date(2000, 01, 01), "Test Split", 1, 2);

            corporateActionList.Should().SatisfyRespectively(

                first => first.Should().BeEquivalentTo(new
                {
                    Id = id,
                    Stock = stock,
                    Date = new Date(2000, 01, 01),
                    Type = CorporateActionType.SplitConsolidation,
                    Description = "Test Split",
                    OriginalUnits = 1,
                    NewUnits = 2
                })

            );

            mockRepository.Verify();
        }

        [Fact]
        public void AddSplitConsolidationithBlankDescription()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            events.Setup(x => x.Add(It.IsAny<SplitConsolidationAddedEvent>())).Verifiable();

            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            corporateActionList.AddSplitConsolidation(id, new Date(2000, 01, 01), "", 1, 2);

           corporateActionList.Should().SatisfyRespectively(

                first => first.Should().BeEquivalentTo(new
                {
                    Id = id,
                    Stock = stock,
                    Date = new Date(2000, 01, 01),
                    Type = CorporateActionType.SplitConsolidation,
                    Description = "1 for 2 Stock Split",
                    OriginalUnits = 1,
                    NewUnits = 2
                })

            );

            mockRepository.Verify();
        }

        [Fact]
        public void ApplySplitConsolidationAddedEvent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            var @event = new SplitConsolidationAddedEvent(stock.Id, 0, id, new Date(2000, 01, 01), "Test Split", 1, 2);

            corporateActionList.Apply(@event);

            corporateActionList.Should().SatisfyRespectively(

                first => first.Should().BeEquivalentTo(new
                {
                    Id = id,
                    Stock = stock,
                    Date = new Date(2000, 01, 01),
                    Type = CorporateActionType.SplitConsolidation,
                    Description = "Test Split",
                    OriginalUnits = 1,
                    NewUnits = 2
                })

            );

            mockRepository.Verify();
        }

        [Fact]
        public void AddTransformation()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "XYZ Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            events.Setup(x => x.Add(It.IsAny<TransformationAddedEvent>())).Verifiable();

            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            var resultStocks = new Transformation.ResultingStock[] {
                new Transformation.ResultingStock(stock2.Id, 1, 2, 0.40m, new Date(2020, 02, 01))
            };
            corporateActionList.AddTransformation(id, new Date(2000, 01, 01), "Test Transformation", new Date(2000, 02, 01), 1.20m, false, resultStocks);

            corporateActionList.Should().SatisfyRespectively(

                first => first.Should().BeEquivalentTo(new
                {
                    Id = id,
                    Stock = stock,
                    Date = new Date(2000, 01, 01),
                    Type = CorporateActionType.Transformation,
                    Description = "Test Transformation",
                    CashComponent = 1.20M,
                    RolloverRefliefApplies = false
                })

            );

            mockRepository.Verify();
        }

        [Fact]
        public void AddTransformationWithBlankDescription()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "XYZ Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            events.Setup(x => x.Add(It.IsAny<TransformationAddedEvent>())).Verifiable();

            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            var resultStocks = new Transformation.ResultingStock[] {
                new Transformation.ResultingStock(stock2.Id, 1, 2, 0.40m, new Date(2020, 02, 01))
            };
            corporateActionList.AddTransformation(id, new Date(2000, 01, 01), "", new Date(2000, 02, 01), 1.20m, false, resultStocks);

            corporateActionList.Should().SatisfyRespectively(

                first => first.Should().BeEquivalentTo(new
                {
                    Id = id,
                    Stock = stock,
                    Date = new Date(2000, 01, 01),
                    Type = CorporateActionType.Transformation,
                    Description = "Transformation",
                    CashComponent = 1.20M,
                    RolloverRefliefApplies = false
                })

            );

            mockRepository.Verify();
        }

        [Fact]
        public void ApplyTransformationAddedEvent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "XYZ Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            var resultStocks = new TransformationAddedEvent.ResultingStock[] {
                new TransformationAddedEvent.ResultingStock(stock2.Id, 1, 2, 0.40m, new Date(2020, 02, 01))
            };
            var @event = new TransformationAddedEvent(stock.Id, 0, id, new Date(2000, 01, 01), "Test Transformation", new Date(2000, 02, 01), 1.20m, false, resultStocks);

            corporateActionList.Apply(@event);

            corporateActionList.Should().SatisfyRespectively(

                first => {
                    first.Should().BeEquivalentTo(new
                    {
                        Id = id,
                        Stock = stock,
                        Date = new Date(2000, 01, 01),
                        Type = CorporateActionType.Transformation,
                        Description = "Test Transformation",
                        CashComponent = 1.20M,
                        RolloverRefliefApplies = false
                    });
                    first.Should().BeOfType<Transformation>().Which.ResultingStocks.Should().HaveCount(1);
                    }

            );
            mockRepository.Verify();
        }


    }
}
