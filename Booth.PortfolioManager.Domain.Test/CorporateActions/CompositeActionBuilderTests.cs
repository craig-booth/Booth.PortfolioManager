using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.CorporateActions;
using Booth.PortfolioManager.Domain.CorporateActions.Events;

namespace Booth.PortfolioManager.Domain.Test.CorporateActions
{
    public class CompositeActionBuilderTests
    {

        [Fact]
        public void NoChildActions()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            CompositeActionAddedEvent @event = null;
            var id = Guid.NewGuid();
            var builder = new CompositeActionBuilder(stock, id, new Date(2000, 01, 01), "Test Composite Action", (x) => { @event = x; });
            builder.Finish();

            using (new AssertionScope())
            {
                @event.Should().BeEquivalentTo(
                    new
                    {
                        EntityId = stock.Id,
                        Version = 1,
                        ActionId = id,
                        ActionDate = new Date(2000, 01, 01),
                        Description = "Test Composite Action"
                    });
                @event.ChildActions.Should().BeEmpty();
            };
        }

        [Fact]
        public void AddCapitalReturn()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            CompositeActionAddedEvent @event = null;
            var id = Guid.NewGuid();
            var builder = new CompositeActionBuilder(stock, id, new Date(2000, 01, 01), "Test Composite Action", (x) => { @event = x; });
            builder.AddCapitalReturn("Test Capital Return", new Date(2000, 02, 01), 10.00m)
                   .Finish();

            using (new AssertionScope())
            {
                @event.Should().BeEquivalentTo(
                    new
                    {
                        EntityId = stock.Id,
                        Version = 1,
                        ActionId = id,
                        ActionDate = new Date(2000, 01, 01),
                        Description = "Test Composite Action"
                    });
                @event.ChildActions.Should().SatisfyRespectively(

                    first =>
                    {
                        first.Should().BeOfType<CapitalReturnAddedEvent>();
                        if (first is CapitalReturnAddedEvent capitalReturn)
                        {
                            capitalReturn.EntityId.Should().Be(stock.Id);
                            capitalReturn.Version.Should().Be(1);
                            capitalReturn.ActionDate.Should().Be(new Date(2000, 01, 01));
                            capitalReturn.Description.Should().Be("Test Capital Return");
                            capitalReturn.PaymentDate.Should().Be(new Date(2000, 02, 01));
                            capitalReturn.Amount.Should().Be(10.00m);
                        }
                    }
                );
            };
        }

        [Fact]
        public void AddDividend()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            CompositeActionAddedEvent @event = null;
            var id = Guid.NewGuid();
            var builder = new CompositeActionBuilder(stock, id, new Date(2000, 01, 01), "Test Composite Action", (x) => { @event = x; });
            builder.AddDividend("Test Dividend", new Date(2000, 02, 01), 1.20m, 1.00m, 2.50m)
                   .Finish();


            using (new AssertionScope())
            {
                @event.Should().BeEquivalentTo(
                    new
                    {
                        EntityId = stock.Id,
                        Version = 1,
                        ActionId = id,
                        ActionDate = new Date(2000, 01, 01),
                        Description = "Test Composite Action"
                    });
                @event.ChildActions.Should().SatisfyRespectively(

                    first =>
                    {
                        first.Should().BeOfType<DividendAddedEvent>();
                        if (first is DividendAddedEvent dividend)
                        {
                            dividend.EntityId.Should().Be(stock.Id);
                            dividend.Version.Should().Be(1);
                            dividend.ActionDate.Should().Be(new Date(2000, 01, 01));
                            dividend.Description.Should().Be("Test Dividend");
                            dividend.PaymentDate.Should().Be(new Date(2000, 02, 01));
                            dividend.DividendAmount.Should().Be(1.20m);
                            dividend.PercentFranked.Should().Be(1.00m);
                            dividend.DrpPrice.Should().Be(2.50m);
                        }
                    }
                );
            };
        }

       
        [Fact]
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

            using (new AssertionScope())
            {
                @event.Should().BeEquivalentTo(
                    new
                    {
                        EntityId = stock.Id,
                        Version = 1,
                        ActionId = id,
                        ActionDate = new Date(2000, 01, 01),
                        Description = "Test Composite Action"
                    });
                @event.ChildActions.Should().SatisfyRespectively(

                    first =>
                    {
                        first.Should().BeOfType<TransformationAddedEvent>();
                        if (first is TransformationAddedEvent transformation)
                        {
                            transformation.EntityId.Should().Be(stock.Id);
                            transformation.Version.Should().Be(1);
                            transformation.ActionDate.Should().Be(new Date(2000, 01, 01));
                            transformation.Description.Should().Be("Test Transformation");
                            transformation.ImplementationDate.Should().Be(new Date(2000, 02, 01));
                            transformation.CashComponent.Should().Be(1.20m);
                            transformation.RolloverRefliefApplies.Should().BeFalse();
                            transformation.ResultingStocks.Should().HaveCount(1);
                        }
                    }
                );
            };
        }

        [Fact]
        public void AddSplitConsolidation()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            CompositeActionAddedEvent @event = null;
            var id = Guid.NewGuid();
            var builder = new CompositeActionBuilder(stock, id, new Date(2000, 01, 01), "Test Composite Action", (x) => { @event = x; });
            builder.AddSplitConsolidation("Test Split", 1, 2)
                   .Finish();

            using (new AssertionScope())
            {
                @event.Should().BeEquivalentTo(
                    new
                    {
                        EntityId = stock.Id,
                        Version = 1,
                        ActionId = id,
                        ActionDate = new Date(2000, 01, 01),
                        Description = "Test Composite Action"
                    });
                @event.ChildActions.Should().SatisfyRespectively(

                    first =>
                    {
                        first.Should().BeOfType<SplitConsolidationAddedEvent>();
                        if (first is SplitConsolidationAddedEvent splitConsolidation)
                        {
                            splitConsolidation.EntityId.Should().Be(stock.Id);
                            splitConsolidation.Version.Should().Be(1);
                            splitConsolidation.ActionDate.Should().Be(new Date(2000, 01, 01));
                            splitConsolidation.Description.Should().Be("Test Split");
                            splitConsolidation.OriginalUnits.Should().Be(1);
                            splitConsolidation.NewUnits.Should().Be(2);
                        }
                    }
                );
            };

        }

        [Fact]
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

            using (new AssertionScope())
            {
                @event.Should().BeEquivalentTo(
                    new
                    {
                        EntityId = stock.Id,
                        Version = 1,
                        ActionId = id,
                        ActionDate = new Date(2000, 01, 01),
                        Description = "Test Composite Action"
                    });
                @event.ChildActions.Should().SatisfyRespectively(
                    first => first.Should().BeOfType<SplitConsolidationAddedEvent>(),
                    second => second.Should().BeOfType<CapitalReturnAddedEvent>(),
                    third => third.Should().BeOfType<DividendAddedEvent>()
                );
            };

        }

        [Fact]
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

            using (new AssertionScope())
            {
                @event.Should().BeEquivalentTo(
                    new
                    {
                        EntityId = stock.Id,
                        Version = 1,
                        ActionId = id,
                        ActionDate = new Date(2000, 01, 01),
                        Description = "Test Composite Action"
                    });
                @event.ChildActions.Should().SatisfyRespectively(

                    first =>
                    {
                        first.Should().BeOfType<CapitalReturnAddedEvent>();
                        if (first is CapitalReturnAddedEvent capitalReturn)
                        {
                            capitalReturn.EntityId.Should().Be(stock.Id);
                            capitalReturn.Version.Should().Be(1);
                            capitalReturn.ActionDate.Should().Be(new Date(2000, 01, 01));
                            capitalReturn.Description.Should().Be("Test Capital Return 1");
                            capitalReturn.PaymentDate.Should().Be(new Date(2000, 02, 01));
                            capitalReturn.Amount.Should().Be(10.00m);
                        }
                    },

                    second =>
                    {
                        second.Should().BeOfType<CapitalReturnAddedEvent>();
                        if (second is CapitalReturnAddedEvent capitalReturn)
                        {
                            capitalReturn.EntityId.Should().Be(stock.Id);
                            capitalReturn.Version.Should().Be(1);
                            capitalReturn.ActionDate.Should().Be(new Date(2000, 01, 01));
                            capitalReturn.Description.Should().Be("Test Capital Return 2");
                            capitalReturn.PaymentDate.Should().Be(new Date(2000, 02, 01));
                            capitalReturn.Amount.Should().Be(12.00m);
                        }
                    }
                );
            };
        }

    }

}
