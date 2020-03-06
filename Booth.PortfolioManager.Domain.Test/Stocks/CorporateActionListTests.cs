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
            corporateActionList.StartCompositeAction(id, new Date(2000, 01, 01), "Test Composite Action");

            Assert.Multiple(() =>
            {
                Assert.That(corporateActionList, Has.Count.EqualTo(1));

                var compositeAction = corporateActionList[0] as CompositeAction;
                Assert.That(compositeAction.Id, Is.EqualTo(id));
                Assert.That(compositeAction.Stock, Is.EqualTo(stock));
                Assert.That(compositeAction.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(compositeAction.Type, Is.EqualTo(CorporateActionType.CapitalReturn));
                Assert.That(compositeAction.Description, Is.EqualTo("Test Composite Action"));

                Assert.That(compositeAction.ChildActions, Has.Count.EqualTo(0));
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
            corporateActionList.StartCompositeAction(id, new Date(2000, 01, 01), "");

            Assert.Multiple(() =>
            {
                Assert.That(corporateActionList, Has.Count.EqualTo(1));

                var compositeAction = corporateActionList[0] as CompositeAction;
                Assert.That(compositeAction.Id, Is.EqualTo(id));
                Assert.That(compositeAction.Stock, Is.EqualTo(stock));
                Assert.That(compositeAction.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(compositeAction.Type, Is.EqualTo(CorporateActionType.CapitalReturn));
                Assert.That(compositeAction.Description, Is.EqualTo(""));

                Assert.That(compositeAction.ChildActions, Has.Count.EqualTo(0));
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
                .AddDividend("Dividend", new Date(2000, 03, 01), 3.00m, 1.00m, 5.00m)
                .AddSplitConsolidation("Split", 1, 2)
                .AddTransformation("Transformation", new Date(2000, 04, 01), 1.00m, true, null)
                .Finish();

            Assert.Multiple(() =>
            {
                Assert.That(corporateActionList, Has.Count.EqualTo(1));

                var compositeAction = corporateActionList[0] as CompositeAction;
                Assert.That(compositeAction.Id, Is.EqualTo(id));
                Assert.That(compositeAction.Stock, Is.EqualTo(stock));
                Assert.That(compositeAction.Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(compositeAction.Type, Is.EqualTo(CorporateActionType.CapitalReturn));
                Assert.That(compositeAction.Description, Is.EqualTo(""));

                Assert.That(compositeAction.ChildActions, Has.Count.EqualTo(0));
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void ApplyCompositeActionAddedEvent()
        {
            Assert.Inconclusive();
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
            Assert.Inconclusive();
        }

        [TestCase]
        public void AddSplitConsolidationithBlankDescription()
        {
            Assert.Inconclusive();
        }

        [TestCase]
        public void ApplySplitConsolidationAddedEvent()
        {
            Assert.Inconclusive();
        }

        [TestCase]
        public void AddTransformation()
        {
            Assert.Inconclusive();
        }

        [TestCase]
        public void AddTransformationWithBlankDescription()
        {
            Assert.Inconclusive();
        }

        [TestCase]
        public void ApplyTransformationAddedEvent()
        {
            Assert.Inconclusive();
        }


    }
}
