using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.CorporateActions;
using Booth.PortfolioManager.Domain.CorporateActions.Events;

namespace Booth.PortfolioManager.Domain.Test.CorporateActions
{
    class CompositeActionBuilderTests
    {

        [TestCase]
        public void NoChildActions()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            CompositeActionAddedEvent @event = null;
            var id = Guid.NewGuid();
            var builder = new CompositeActionBuilder(stock, id, new Date(2000, 01, 01), "Test Composite Action", (x) => { @event = x; });
            builder.Finish();

            Assert.That(@event, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(@event.EntityId, Is.EqualTo(stock.Id), "Parent");
                Assert.That(@event.Version, Is.EqualTo(1), "Parent");
                Assert.That(@event.ActionId, Is.EqualTo(id), "Parent");
                Assert.That(@event.ActionDate, Is.EqualTo(new Date(2000, 01, 01)), "Parent");
                Assert.That(@event.Description, Is.EqualTo("Test Composite Action"), "Parent");

                var childActions = @event.ChildActions.ToList();
                Assert.That(childActions, Is.Empty, "Parent");
            });

        }

        [TestCase]
        public void AddCapitalReturn()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            CompositeActionAddedEvent @event = null;
            var id = Guid.NewGuid();
            var builder = new CompositeActionBuilder(stock, id, new Date(2000, 01, 01), "Test Composite Action", (x) => { @event = x; });
            builder.AddCapitalReturn("Test Capital Return", new Date(2000, 02, 01), 10.00m)
                   .Finish();

            Assert.That(@event, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(@event.EntityId, Is.EqualTo(stock.Id), "Parent");
                Assert.That(@event.Version, Is.EqualTo(1), "Parent");
                Assert.That(@event.ActionId, Is.EqualTo(id), "Parent");
                Assert.That(@event.ActionDate, Is.EqualTo(new Date(2000, 01, 01)), "Parent");
                Assert.That(@event.Description, Is.EqualTo("Test Composite Action"), "Parent");

                var childActions = @event.ChildActions.ToList();
                Assert.That(childActions, Has.Count.EqualTo(1), "Parent");

                if (childActions.Count >= 1)
                {
                    Assert.That(childActions[0], Is.TypeOf(typeof(CapitalReturnAddedEvent)), "Child 1");
                    if (childActions[0] is CapitalReturnAddedEvent capitalReturn)
                    {
                        Assert.That(capitalReturn.EntityId, Is.EqualTo(stock.Id), "Child 1");
                        Assert.That(capitalReturn.Version, Is.EqualTo(1), "Child 1");
                        Assert.That(capitalReturn.ActionDate, Is.EqualTo(new Date(2000, 01, 01)), "Child 1");
                        Assert.That(capitalReturn.Description, Is.EqualTo("Test Capital Return"), "Child 1");
                        Assert.That(capitalReturn.PaymentDate, Is.EqualTo(new Date(2000, 02, 01)), "Child 1");
                        Assert.That(capitalReturn.Amount, Is.EqualTo(10.00m), "Child 1");
                    }
                }
            });
        }

        [TestCase]
        public void AddDividend()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            CompositeActionAddedEvent @event = null;
            var id = Guid.NewGuid();
            var builder = new CompositeActionBuilder(stock, id, new Date(2000, 01, 01), "Test Composite Action", (x) => { @event = x; });
            builder.AddDividend("Test Dividend", new Date(2000, 02, 01), 1.20m, 1.00m, 2.50m)
                   .Finish();

            Assert.That(@event, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(@event.EntityId, Is.EqualTo(stock.Id), "Parent");
                Assert.That(@event.Version, Is.EqualTo(1), "Parent");
                Assert.That(@event.ActionId, Is.EqualTo(id), "Parent");
                Assert.That(@event.ActionDate, Is.EqualTo(new Date(2000, 01, 01)), "Parent");
                Assert.That(@event.Description, Is.EqualTo("Test Composite Action"), "Parent");

                var childActions = @event.ChildActions.ToList();
                Assert.That(childActions, Has.Count.EqualTo(1), "Parent");

                if (childActions.Count >= 1)
                {
                    Assert.That(childActions[0], Is.TypeOf(typeof(DividendAddedEvent)), "Child 1");
                    if (childActions[0] is DividendAddedEvent dividend)
                    {
                        Assert.That(dividend.EntityId, Is.EqualTo(stock.Id), "Child 1");
                        Assert.That(dividend.Version, Is.EqualTo(1), "Child 1");
                        Assert.That(dividend.ActionDate, Is.EqualTo(new Date(2000, 01, 01)), "Child 1");
                        Assert.That(dividend.Description, Is.EqualTo("Test Dividend"), "Child 1");
                        Assert.That(dividend.PaymentDate, Is.EqualTo(new Date(2000, 02, 01)), "Child 1");
                        Assert.That(dividend.DividendAmount, Is.EqualTo(1.20m), "Child 1");
                        Assert.That(dividend.PercentFranked, Is.EqualTo(1.00m), "Child 1");
                        Assert.That(dividend.DRPPrice, Is.EqualTo(2.50m), "Child 1");
                    }
                }
            });
        }

       
        [TestCase]
        public void AddTransformation()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "XYZ Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            CompositeActionAddedEvent @event = null;
            var id = Guid.NewGuid();
            var builder = new CompositeActionBuilder(stock, id, new Date(2000, 01, 01), "Test Composite Action", (x) => { @event = x; });

            var resultStocks = new Transformation.ResultingStock[] {
                new Transformation.ResultingStock(stock2.Id, 1, 2, 0.40m, new Date(2020, 02, 01))
            };
            builder.AddTransformation("Test Transformation", new Date(2000, 02, 01), 1.20m, false, resultStocks)
                   .Finish();

            Assert.That(@event, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(@event.EntityId, Is.EqualTo(stock.Id), "Parent");
                Assert.That(@event.Version, Is.EqualTo(1), "Parent");
                Assert.That(@event.ActionId, Is.EqualTo(id), "Parent");
                Assert.That(@event.ActionDate, Is.EqualTo(new Date(2000, 01, 01)), "Parent");
                Assert.That(@event.Description, Is.EqualTo("Test Composite Action"), "Parent");

                var childActions = @event.ChildActions.ToList();
                Assert.That(childActions, Has.Count.EqualTo(1), "Parent");

                if (childActions.Count >= 1)
                {
                    Assert.That(childActions[0], Is.TypeOf(typeof(TransformationAddedEvent)), "Child 1");
                    if (childActions[0] is TransformationAddedEvent transformation)
                    {
                        Assert.That(transformation.EntityId, Is.EqualTo(stock.Id), "Child 1");
                        Assert.That(transformation.Version, Is.EqualTo(1), "Child 1");
                        Assert.That(transformation.ActionDate, Is.EqualTo(new Date(2000, 01, 01)), "Child 1");
                        Assert.That(transformation.Description, Is.EqualTo("Test Transformation"), "Child 1");
                        Assert.That(transformation.ImplementationDate, Is.EqualTo(new Date(2000, 02, 01)), "Child 1");
                        Assert.That(transformation.CashComponent, Is.EqualTo(1.20m), "Child 1");
                        Assert.That(transformation.RolloverRefliefApplies, Is.EqualTo(false), "Child 1");
                        Assert.That(transformation.ResultingStocks.Count(), Is.EqualTo(1), "Child 1");
                    }
                }
            });
        }

        [TestCase]
        public void AddSplitConsolidation()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            CompositeActionAddedEvent @event = null;
            var id = Guid.NewGuid();
            var builder = new CompositeActionBuilder(stock, id, new Date(2000, 01, 01), "Test Composite Action", (x) => { @event = x; });
            builder.AddSplitConsolidation("Test Split", 1, 2)
                   .Finish();

            Assert.That(@event, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(@event.EntityId, Is.EqualTo(stock.Id), "Parent");
                Assert.That(@event.Version, Is.EqualTo(1), "Parent");
                Assert.That(@event.ActionId, Is.EqualTo(id), "Parent");
                Assert.That(@event.ActionDate, Is.EqualTo(new Date(2000, 01, 01)), "Parent");
                Assert.That(@event.Description, Is.EqualTo("Test Composite Action"), "Parent");

                var childActions = @event.ChildActions.ToList();
                Assert.That(childActions, Has.Count.EqualTo(1), "Parent");

                if (childActions.Count >= 1)
                {
                    Assert.That(childActions[0], Is.TypeOf(typeof(SplitConsolidationAddedEvent)), "Child 1");
                    if (childActions[0] is SplitConsolidationAddedEvent splitConsolidation)
                    {
                        Assert.That(splitConsolidation.EntityId, Is.EqualTo(stock.Id), "Child 1");
                        Assert.That(splitConsolidation.Version, Is.EqualTo(1), "Child 1");
                        Assert.That(splitConsolidation.ActionDate, Is.EqualTo(new Date(2000, 01, 01)), "Child 1");
                        Assert.That(splitConsolidation.Description, Is.EqualTo("Test Split"), "Child 1");
                        Assert.That(splitConsolidation.OriginalUnits, Is.EqualTo(1), "Child 1");
                        Assert.That(splitConsolidation.NewUnits, Is.EqualTo(2), "Child 1");
                    }
                }
            });
        }

        [TestCase]
        public void AddOneOfEachTypeOfChildAction()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            CompositeActionAddedEvent @event = null;
            var id = Guid.NewGuid();
            var builder = new CompositeActionBuilder(stock, id, new Date(2000, 01, 01), "Test Composite Action", (x) => { @event = x; });
            builder.AddSplitConsolidation("Test Split", 1, 2)
                   .AddCapitalReturn("Test Capital Return", new Date(2000, 02, 01), 10.00m)
                   .AddDividend("Test Dividend", new Date(2000, 02, 01), 1.20m, 1.00m, 2.50m)
                   .Finish();

            Assert.That(@event, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(@event.EntityId, Is.EqualTo(stock.Id), "Parent");
                Assert.That(@event.Version, Is.EqualTo(1), "Parent");
                Assert.That(@event.ActionId, Is.EqualTo(id), "Parent");
                Assert.That(@event.ActionDate, Is.EqualTo(new Date(2000, 01, 01)), "Parent");
                Assert.That(@event.Description, Is.EqualTo("Test Composite Action"), "Parent");

                var childActions = @event.ChildActions.ToList();
                Assert.That(childActions, Has.Count.EqualTo(3), "Parent");

                if (childActions.Count >= 1)
                    Assert.That(childActions[0], Is.TypeOf(typeof(SplitConsolidationAddedEvent)), "Child 1");       

                if (childActions.Count >= 2)
                    Assert.That(childActions[1], Is.TypeOf(typeof(CapitalReturnAddedEvent)), "Child 1");

                if (childActions.Count >= 3)
                    Assert.That(childActions[2], Is.TypeOf(typeof(DividendAddedEvent)), "Child 1");
            });
        }

        [TestCase]
        public void AddMultipleOfTheSameTypeOfChildAction()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            CompositeActionAddedEvent @event = null;
            var id = Guid.NewGuid();
            var builder = new CompositeActionBuilder(stock, id, new Date(2000, 01, 01), "Test Composite Action", (x) => { @event = x; });
            builder.AddCapitalReturn("Test Capital Return 1", new Date(2000, 02, 01), 10.00m)
                   .AddCapitalReturn("Test Capital Return 2", new Date(2000, 02, 01), 12.00m)
                   .Finish();

            Assert.That(@event, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(@event.EntityId, Is.EqualTo(stock.Id), "Parent");
                Assert.That(@event.Version, Is.EqualTo(1), "Parent");
                Assert.That(@event.ActionId, Is.EqualTo(id), "Parent");
                Assert.That(@event.ActionDate, Is.EqualTo(new Date(2000, 01, 01)), "Parent");
                Assert.That(@event.Description, Is.EqualTo("Test Composite Action"), "Parent");

                var childActions = @event.ChildActions.ToList();
                Assert.That(childActions, Has.Count.EqualTo(2), "Parent");

                if (childActions.Count >= 1)
                {
                    Assert.That(childActions[0], Is.TypeOf(typeof(CapitalReturnAddedEvent)), "Child 1");
                    if (childActions[0] is CapitalReturnAddedEvent capitalReturn)
                    {
                        Assert.That(capitalReturn.EntityId, Is.EqualTo(stock.Id), "Child 1");
                        Assert.That(capitalReturn.Version, Is.EqualTo(1), "Child 1");
                        Assert.That(capitalReturn.ActionDate, Is.EqualTo(new Date(2000, 01, 01)), "Child 1");
                        Assert.That(capitalReturn.Description, Is.EqualTo("Test Capital Return 1"), "Child 1");
                        Assert.That(capitalReturn.PaymentDate, Is.EqualTo(new Date(2000, 02, 01)), "Child 1");
                        Assert.That(capitalReturn.Amount, Is.EqualTo(10.00m), "Child 1");
                    }
                }

                if (childActions.Count >= 2)
                {
                    Assert.That(childActions[1], Is.TypeOf(typeof(CapitalReturnAddedEvent)), "Child 2");
                    if (childActions[1] is CapitalReturnAddedEvent capitalReturn)
                    {
                        Assert.That(capitalReturn.EntityId, Is.EqualTo(stock.Id), "Child 2");
                        Assert.That(capitalReturn.Version, Is.EqualTo(1), "Child 2");
                        Assert.That(capitalReturn.ActionDate, Is.EqualTo(new Date(2000, 01, 01)), "Child 2");
                        Assert.That(capitalReturn.Description, Is.EqualTo("Test Capital Return 2"), "Child 2");
                        Assert.That(capitalReturn.PaymentDate, Is.EqualTo(new Date(2000, 02, 01)), "Child 2");
                        Assert.That(capitalReturn.Amount, Is.EqualTo(12.00m), "Child 2");
                    }
                }
            });
        }

    }

}
