using System;
using System.Linq;

using NUnit.Framework;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.CorporateActions;
using Booth.PortfolioManager.Domain.CorporateActions.Events;

namespace Booth.PortfolioManager.Domain.Test.Stocks
{
    class CorporateActionListTests
    {

        [TestCase]
        public void AddCapitalReturn()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            events.Setup(x => x.Add(It.IsAny<CapitalReturnAddedEvent>())).Verifiable();

            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            corporateActionList.AddCapitalReturn(id, new Date(2000, 01, 01), "Test Capital Return", new Date(2000, 02, 01), 10.00m);

            Assert.Multiple(() =>
            {
                Assert.That(corporateActionList, Has.Count.EqualTo(1));

                var capitalReturn = corporateActionList[0] as CapitalReturn;
                Assert.That(capitalReturn.Id, Is.EqualTo(id));
                Assert.That(capitalReturn.Stock, Is.EqualTo(stock));
                Assert.That(capitalReturn.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(capitalReturn.Type, Is.EqualTo(CorporateActionType.CapitalReturn));
                Assert.That(capitalReturn.Description, Is.EqualTo("Test Capital Return"));
                Assert.That(capitalReturn.PaymentDate, Is.EqualTo(new Date(2000, 02, 01)));
                Assert.That(capitalReturn.Amount, Is.EqualTo(10.00m));
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void AddCapitalReturnWithBlankDescription()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            events.Setup(x => x.Add(It.IsAny<CapitalReturnAddedEvent>())).Verifiable();

            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            corporateActionList.AddCapitalReturn(id, new Date(2000, 01, 01), "", new Date(2000, 02, 01), 10.00m);

            Assert.Multiple(() =>
            {
                Assert.That(corporateActionList, Has.Count.EqualTo(1));

                var capitalReturn = corporateActionList[0] as CapitalReturn;
                Assert.That(capitalReturn.Id, Is.EqualTo(id));
                Assert.That(capitalReturn.Stock, Is.EqualTo(stock));
                Assert.That(capitalReturn.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(capitalReturn.Type, Is.EqualTo(CorporateActionType.CapitalReturn));
                Assert.That(capitalReturn.Description, Is.EqualTo("Capital Return $10.00"));
                Assert.That(capitalReturn.PaymentDate, Is.EqualTo(new Date(2000, 02, 01)));
                Assert.That(capitalReturn.Amount, Is.EqualTo(10.00m));
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void ApplyCapitalReturnAddedEvent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            var @event = new CapitalReturnAddedEvent(stock.Id, 0, id, new Date(2000, 01, 01), "Test Capital Return", new Date(2000, 02, 01), 10.00m);

            corporateActionList.Apply(@event);

            Assert.Multiple(() =>
            {
                Assert.That(corporateActionList, Has.Count.EqualTo(1));

                var capitalReturn = corporateActionList[0] as CapitalReturn;
                Assert.That(capitalReturn.Id, Is.EqualTo(id));
                Assert.That(capitalReturn.Stock, Is.EqualTo(stock));
                Assert.That(capitalReturn.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(capitalReturn.Type, Is.EqualTo(CorporateActionType.CapitalReturn));
                Assert.That(capitalReturn.Description, Is.EqualTo("Test Capital Return"));
                Assert.That(capitalReturn.PaymentDate, Is.EqualTo(new Date(2000, 02, 01)));
                Assert.That(capitalReturn.Amount, Is.EqualTo(10.00m));
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void AddCompositeActionWithoutChildren()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            events.Setup(x => x.Add(It.IsAny<CompositeActionAddedEvent>())).Verifiable();

            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            corporateActionList.StartCompositeAction(id, new Date(2000, 01, 01), "Test Composite Action").Finish();

            Assert.Multiple(() =>
            {
                Assert.That(corporateActionList, Has.Count.EqualTo(1));

                var compositeAction = corporateActionList[0] as CompositeAction;
                Assert.That(compositeAction.Id, Is.EqualTo(id));
                Assert.That(compositeAction.Stock, Is.EqualTo(stock));
                Assert.That(compositeAction.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(compositeAction.Type, Is.EqualTo(CorporateActionType.Composite));
                Assert.That(compositeAction.Description, Is.EqualTo("Test Composite Action"));

                Assert.That(compositeAction.ChildActions.Count(), Is.EqualTo(0));
            });

            mockRepository.Verify(); 
        }

        [TestCase]
        public void AddCompositeActionWithBlankDescription()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            events.Setup(x => x.Add(It.IsAny<CompositeActionAddedEvent>())).Verifiable();

            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            corporateActionList.StartCompositeAction(id, new Date(2000, 01, 01), "").Finish();

            Assert.Multiple(() =>
            {
                Assert.That(corporateActionList, Has.Count.EqualTo(1));

                var compositeAction = corporateActionList[0] as CompositeAction;
                Assert.That(compositeAction.Id, Is.EqualTo(id));
                Assert.That(compositeAction.Stock, Is.EqualTo(stock));
                Assert.That(compositeAction.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(compositeAction.Type, Is.EqualTo(CorporateActionType.Composite));
                Assert.That(compositeAction.Description, Is.EqualTo("Complex corporate action"));

                Assert.That(compositeAction.ChildActions.Count(), Is.EqualTo(0));
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void AddCompositeActionChildActions()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            events.Setup(x => x.Add(It.IsAny<CompositeActionAddedEvent>())).Verifiable();

            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            corporateActionList.StartCompositeAction(id, new Date(2000, 01, 01), "Test CompositeAction")
                .AddCapitalReturn("Capital Return", new Date(2000, 02, 01), 10.00m)
                .AddSplitConsolidation("Split", 1, 2)
                .Finish();

            Assert.Multiple(() =>
            {
                Assert.That(corporateActionList, Has.Count.EqualTo(1));

                var compositeAction = corporateActionList[0] as CompositeAction;
                Assert.That(compositeAction.Id, Is.EqualTo(id));
                Assert.That(compositeAction.Stock, Is.EqualTo(stock));
                Assert.That(compositeAction.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(compositeAction.Type, Is.EqualTo(CorporateActionType.Composite));
                Assert.That(compositeAction.Description, Is.EqualTo("Test CompositeAction"));

                var childActions = compositeAction.ChildActions.ToList();
                Assert.That(childActions, Has.Count.EqualTo(2));

                if (childActions.Count >= 1)
                    Assert.That(childActions[0], Is.TypeOf(typeof(CapitalReturn)), "Child 1");

                if (childActions.Count >= 2)
                    Assert.That(childActions[1], Is.TypeOf(typeof(SplitConsolidation)), "Child 2");

            });

            mockRepository.Verify();
        }

        [TestCase]
        public void ApplyCompositeActionAddedEvent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            var @event = new CompositeActionAddedEvent(stock.Id, 0, id, new Date(2000, 01, 01), "Test Complex Action");
            @event.ChildActions.Add(new SplitConsolidationAddedEvent(stock.Id, 0, id, new Date(2000, 01, 01), "Test Split", 1, 2));

            corporateActionList.Apply(@event);

            Assert.Multiple(() =>
            {
                Assert.That(corporateActionList, Has.Count.EqualTo(1));

                var complexAction = corporateActionList[0] as CompositeAction;
                Assert.That(complexAction.Id, Is.EqualTo(id));
                Assert.That(complexAction.Stock, Is.EqualTo(stock));
                Assert.That(complexAction.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(complexAction.Type, Is.EqualTo(CorporateActionType.Composite));
                Assert.That(complexAction.Description, Is.EqualTo("Test Complex Action"));

                var childActions = complexAction.ChildActions.ToList();
                Assert.That(childActions, Has.Count.EqualTo(1));

                if (childActions.Count >= 1)
                {
                    Assert.That(childActions[0], Is.TypeOf(typeof(SplitConsolidation)), "Child 1");
                    if (childActions[0] is SplitConsolidation splitConsolidation)
                    {
                        Assert.That(splitConsolidation.Id, Is.EqualTo(id), "Child 1");
                        Assert.That(splitConsolidation.Stock, Is.EqualTo(stock), "Child 1");
                        Assert.That(splitConsolidation.Date, Is.EqualTo(new Date(2000, 01, 01)), "Child 1");
                        Assert.That(splitConsolidation.Type, Is.EqualTo(CorporateActionType.SplitConsolidation), "Child 1");
                        Assert.That(splitConsolidation.Description, Is.EqualTo("Test Split"), "Child 1");
                        Assert.That(splitConsolidation.OriginalUnits, Is.EqualTo(1), "Child 1");
                        Assert.That(splitConsolidation.NewUnits, Is.EqualTo(2), "Child 1");
                    }
                }
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void AddDividend()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            events.Setup(x => x.Add(It.IsAny<DividendAddedEvent>())).Verifiable();

            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            corporateActionList.AddDividend(id, new Date(2000, 01, 01), "Test Dividend", new Date(2000, 02, 01), 1.20m, 1.00m, 2.50m);

            Assert.Multiple(() =>
            {
                Assert.That(corporateActionList, Has.Count.EqualTo(1));

                var dividend = corporateActionList[0] as Dividend;
                Assert.That(dividend.Id, Is.EqualTo(id));
                Assert.That(dividend.Stock, Is.EqualTo(stock));
                Assert.That(dividend.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(dividend.Type, Is.EqualTo(CorporateActionType.Dividend));
                Assert.That(dividend.Description, Is.EqualTo("Test Dividend"));
                Assert.That(dividend.PaymentDate, Is.EqualTo(new Date(2000, 02, 01)));
                Assert.That(dividend.DividendAmount, Is.EqualTo(1.20m));
                Assert.That(dividend.PercentFranked, Is.EqualTo(1.00m));
                Assert.That(dividend.DRPPrice, Is.EqualTo(2.50m));
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void AddDividendWithBlankDescription()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            events.Setup(x => x.Add(It.IsAny<DividendAddedEvent>())).Verifiable();

            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            corporateActionList.AddDividend(id, new Date(2000, 01, 01), "", new Date(2000, 02, 01), 1.20m, 1.00m, 2.50m);

            Assert.Multiple(() =>
            {
                Assert.That(corporateActionList, Has.Count.EqualTo(1));

                var dividend = corporateActionList[0] as Dividend;
                Assert.That(dividend.Id, Is.EqualTo(id));
                Assert.That(dividend.Stock, Is.EqualTo(stock));
                Assert.That(dividend.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(dividend.Type, Is.EqualTo(CorporateActionType.Dividend));
                Assert.That(dividend.Description, Is.EqualTo("Dividend $1.20"));
                Assert.That(dividend.PaymentDate, Is.EqualTo(new Date(2000, 02, 01)));
                Assert.That(dividend.DividendAmount, Is.EqualTo(1.20m));
                Assert.That(dividend.PercentFranked, Is.EqualTo(1.00m));
                Assert.That(dividend.DRPPrice, Is.EqualTo(2.50m));
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void ApplyDividendAddedEvent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            var @event = new DividendAddedEvent(stock.Id, 0, id, new Date(2000, 01, 01), "Test Dividend", new Date(2000, 02, 01), 1.20m, 1.00m, 2.20m);

            corporateActionList.Apply(@event);

            Assert.Multiple(() =>
            {
                Assert.That(corporateActionList, Has.Count.EqualTo(1));

                var dividend = corporateActionList[0] as Dividend;
                Assert.That(dividend.Id, Is.EqualTo(id));
                Assert.That(dividend.Stock, Is.EqualTo(stock));
                Assert.That(dividend.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(dividend.Type, Is.EqualTo(CorporateActionType.Dividend));
                Assert.That(dividend.Description, Is.EqualTo("Test Dividend"));
                Assert.That(dividend.PaymentDate, Is.EqualTo(new Date(2000, 02, 01)));
                Assert.That(dividend.DividendAmount, Is.EqualTo(1.20m));
                Assert.That(dividend.PercentFranked, Is.EqualTo(1.00m));
                Assert.That(dividend.DRPPrice, Is.EqualTo(2.20m));
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void AddSplitConsolidation()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            events.Setup(x => x.Add(It.IsAny<SplitConsolidationAddedEvent>())).Verifiable();

            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            corporateActionList.AddSplitConsolidation(id, new Date(2000, 01, 01), "Test Split", 1, 2);

            Assert.Multiple(() =>
            {
                Assert.That(corporateActionList, Has.Count.EqualTo(1));

                var split = corporateActionList[0] as SplitConsolidation;
                Assert.That(split.Id, Is.EqualTo(id));
                Assert.That(split.Stock, Is.EqualTo(stock));
                Assert.That(split.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(split.Type, Is.EqualTo(CorporateActionType.SplitConsolidation));
                Assert.That(split.Description, Is.EqualTo("Test Split"));
                Assert.That(split.OriginalUnits, Is.EqualTo(1));
                Assert.That(split.NewUnits, Is.EqualTo(2));
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void AddSplitConsolidationithBlankDescription()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            events.Setup(x => x.Add(It.IsAny<SplitConsolidationAddedEvent>())).Verifiable();

            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            corporateActionList.AddSplitConsolidation(id, new Date(2000, 01, 01), "", 1, 2);

            Assert.Multiple(() =>
            {
                Assert.That(corporateActionList, Has.Count.EqualTo(1));

                var split = corporateActionList[0] as SplitConsolidation;
                Assert.That(split.Id, Is.EqualTo(id));
                Assert.That(split.Stock, Is.EqualTo(stock));
                Assert.That(split.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(split.Type, Is.EqualTo(CorporateActionType.SplitConsolidation));
                Assert.That(split.Description, Is.EqualTo("1 for 2 Stock Split"));
                Assert.That(split.OriginalUnits, Is.EqualTo(1));
                Assert.That(split.NewUnits, Is.EqualTo(2));
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void ApplySplitConsolidationAddedEvent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            var @event = new SplitConsolidationAddedEvent(stock.Id, 0, id, new Date(2000, 01, 01), "Test Split", 1, 2);

            corporateActionList.Apply(@event);

            Assert.Multiple(() =>
            {
                Assert.That(corporateActionList, Has.Count.EqualTo(1));

                var split = corporateActionList[0] as SplitConsolidation;
                Assert.That(split.Id, Is.EqualTo(id));
                Assert.That(split.Stock, Is.EqualTo(stock));
                Assert.That(split.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(split.Type, Is.EqualTo(CorporateActionType.SplitConsolidation));
                Assert.That(split.Description, Is.EqualTo("Test Split"));
                Assert.That(split.OriginalUnits, Is.EqualTo(1));
                Assert.That(split.NewUnits, Is.EqualTo(2));
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void AddTransformation()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "XYZ Pty Ltd", false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            events.Setup(x => x.Add(It.IsAny<TransformationAddedEvent>())).Verifiable();

            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            var resultStocks = new Transformation.ResultingStock[] {
                new Transformation.ResultingStock(stock2.Id, 1, 2, 0.40m, new Date(2020, 02, 01))
            };
            corporateActionList.AddTransformation(id, new Date(2000, 01, 01), "Test Transformation", new Date(2000, 02, 01), 1.20m, false, resultStocks);

            Assert.Multiple(() =>
            {
                Assert.That(corporateActionList, Has.Count.EqualTo(1));

                var transformation = corporateActionList[0] as Transformation;
                Assert.That(transformation.Id, Is.EqualTo(id));
                Assert.That(transformation.Stock, Is.EqualTo(stock));
                Assert.That(transformation.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(transformation.Type, Is.EqualTo(CorporateActionType.Transformation));
                Assert.That(transformation.Description, Is.EqualTo("Test Transformation"));
                Assert.That(transformation.CashComponent, Is.EqualTo(1.20m));
                Assert.That(transformation.RolloverRefliefApplies, Is.EqualTo(false));

                Assert.That(transformation.ResultingStocks.Count(), Is.EqualTo(1));
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void AddTransformationWithBlankDescription()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "XYZ Pty Ltd", false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            events.Setup(x => x.Add(It.IsAny<TransformationAddedEvent>())).Verifiable();

            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            var resultStocks = new Transformation.ResultingStock[] {
                new Transformation.ResultingStock(stock2.Id, 1, 2, 0.40m, new Date(2020, 02, 01))
            };
            corporateActionList.AddTransformation(id, new Date(2000, 01, 01), "", new Date(2000, 02, 01), 1.20m, false, resultStocks);

            Assert.Multiple(() =>
            {
                Assert.That(corporateActionList, Has.Count.EqualTo(1));

                var transformation = corporateActionList[0] as Transformation;
                Assert.That(transformation.Id, Is.EqualTo(id));
                Assert.That(transformation.Stock, Is.EqualTo(stock));
                Assert.That(transformation.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(transformation.Type, Is.EqualTo(CorporateActionType.Transformation));
                Assert.That(transformation.Description, Is.EqualTo("Transformation"));
                Assert.That(transformation.CashComponent, Is.EqualTo(1.20m));
                Assert.That(transformation.RolloverRefliefApplies, Is.EqualTo(false));

                Assert.That(transformation.ResultingStocks.Count(), Is.EqualTo(1));
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void ApplyTransformationAddedEvent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "XYZ Pty Ltd", false, AssetCategory.AustralianStocks);

            var events = mockRepository.Create<IEventList>();
            var corporateActionList = new CorporateActionList(stock, events.Object);

            var id = Guid.NewGuid();
            var resultStocks = new TransformationAddedEvent.ResultingStock[] {
                new TransformationAddedEvent.ResultingStock(stock2.Id, 1, 2, 0.40m, new Date(2020, 02, 01))
            };
            var @event = new TransformationAddedEvent(stock.Id, 0, id, new Date(2000, 01, 01), "Test Transformation", new Date(2000, 02, 01), 1.20m, false, resultStocks);

            corporateActionList.Apply(@event);

            Assert.Multiple(() =>
            {
                Assert.That(corporateActionList, Has.Count.EqualTo(1));

                var transformation = corporateActionList[0] as Transformation;
                Assert.That(transformation.Id, Is.EqualTo(id));
                Assert.That(transformation.Stock, Is.EqualTo(stock));
                Assert.That(transformation.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(transformation.Type, Is.EqualTo(CorporateActionType.Transformation));
                Assert.That(transformation.Description, Is.EqualTo("Test Transformation"));
                Assert.That(transformation.CashComponent, Is.EqualTo(1.20m));
                Assert.That(transformation.RolloverRefliefApplies, Is.EqualTo(false));

                Assert.That(transformation.ResultingStocks.Count(), Is.EqualTo(1));
            });

            mockRepository.Verify();
        }


    }
}
