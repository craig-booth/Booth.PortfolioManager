using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using FluentAssertions;
using Moq;

using Booth.Common;
using Booth.EventStore;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Web.Utilities;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.RestApi.CorporateActions;
using Booth.PortfolioManager.Domain.CorporateActions.Events;
using Booth.PortfolioManager.Domain.CorporateActions;

namespace Booth.PortfolioManager.Web.Test.Services
{
    public class CorporateActionServiceTests
    {
        private readonly Guid _StockWithoutCorporateActions;
        private readonly Guid _StockWithCorporateActions;

        private readonly Guid _Action1;
        private readonly Guid _Action2;

        private readonly CorporateActionService _Service;

        private readonly List<Event> _Events = new List<Event>();

        public CorporateActionServiceTests()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockCache = new EntityCache<Stock>();
            var stockQuery = new StockQuery(stockCache);

            _StockWithoutCorporateActions = Guid.NewGuid();
            var stock = new Stock(_StockWithoutCorporateActions);
            stock.List("ABC", "ABC Pty Ltd", new Date(2000, 01, 01), false, AssetCategory.AustralianStocks);
            stockCache.Add(stock);

            _StockWithCorporateActions = Guid.NewGuid();
            var stock2 = new Stock(_StockWithCorporateActions);
            stock2.List("XYZ", "XYZ Pty Ltd", new Date(2000, 01, 01), false, AssetCategory.AustralianStocks);

            _Action1 = Guid.NewGuid();
            stock2.CorporateActions.AddCapitalReturn(_Action1, new Date(2001, 01, 01), "Action 1", new Date(2001, 01, 02), 10.00m);

            _Action2 = Guid.NewGuid();
            stock2.CorporateActions.AddDividend(_Action2, new Date(2001, 01, 01), "Action 2", new Date(2001, 01, 02), 10.00m, 1.00m, 2.45m);

            stockCache.Add(stock2);

            // Remove any existing events
            stock.FetchEvents();
            stock2.FetchEvents();

            var repository = mockRepository.Create<IRepository<Stock>>();
            repository.Setup(x => x.Update(It.IsAny<Stock>())).Callback<Stock>(x => _Events.AddRange(x.FetchEvents()));

            _Service = new CorporateActionService(stockQuery, repository.Object);
        }


        [Fact]
        public void GetCorporateActionStockNotFound()
        {
            var result = _Service.GetCorporateAction(Guid.NewGuid(), Guid.NewGuid());

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetCorporateActionIdNotFound()
        {
            var result = _Service.GetCorporateAction(_StockWithoutCorporateActions, Guid.NewGuid());

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetCorporateAction()
        {
            var result = _Service.GetCorporateAction(_StockWithCorporateActions, _Action2);

            result.Result.Should().BeEquivalentTo(new
            {
                Id = _Action2,
                ActionDate = new Date(2001, 01, 01)
            }); ;
        }

        [Fact]
        public void GetCorporateActionsStockNotFound()
        {
            var result = _Service.GetCorporateActions(Guid.NewGuid(), new DateRange(Date.MinValue, Date.MaxValue));

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetCorporateActions()
        {
            var result = _Service.GetCorporateActions(_StockWithCorporateActions, new DateRange(Date.MinValue, Date.MaxValue));

            result.Result.Should().HaveCount(2);
        }

        [Fact]
        public void AddCorporateActionStockNotFound()
        {
            var dividend = new RestApi.CorporateActions.Dividend();

            var result = _Service.AddCorporateAction(Guid.NewGuid(), dividend);

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void AddCapitalReturn()
        {
            var action = new RestApi.CorporateActions.CapitalReturn()
            {
                Id = Guid.NewGuid(),
                Stock = _StockWithoutCorporateActions,
                ActionDate = new Date(2001, 01, 01),
                Description = "Capital Return",
                PaymentDate = new Date(2001, 01, 15),
                Amount = 10.00m
            };

            var result = _Service.AddCorporateAction(_StockWithoutCorporateActions, action);

            result.Should().HaveOkStatus();
            _Events.Should().BeEquivalentTo(new[]
            {
                new CapitalReturnAddedEvent(_StockWithoutCorporateActions, 1, action.Id, new Date(2001, 01, 01), "Capital Return", new Date(2001, 01, 15), 10.00m)
            });          
        }

        [Fact]
        public void AddCompositeAction()
        {
            var action = new RestApi.CorporateActions.CompositeAction()
            {
                Id = Guid.NewGuid(),
                Stock = _StockWithoutCorporateActions,
                ActionDate = new Date(2001, 01, 01),
                Description = "Restructure"
            };
            action.ChildActions.Add(new RestApi.CorporateActions.CapitalReturn()
            {
                Id = Guid.NewGuid(),
                Stock = _StockWithoutCorporateActions,
                ActionDate = new Date(2001, 01, 01),
                Description = "Capital Return",
                PaymentDate = new Date(2001, 01, 15),
                Amount = 10.00m
            });
            action.ChildActions.Add(new RestApi.CorporateActions.SplitConsolidation()
            {
                Id = Guid.NewGuid(),
                Stock = _StockWithoutCorporateActions,
                ActionDate = new Date(2001, 01, 01),
                Description = "Split",
                OriginalUnits = 1,
                NewUnits = 2
            });

            var result = _Service.AddCorporateAction(_StockWithoutCorporateActions, action);

            result.Should().HaveOkStatus();

            var @event = new CompositeActionAddedEvent(_StockWithoutCorporateActions, 1, action.Id, new Date(2001, 01, 01), "Restructure");
            @event.ChildActions.Add(new CapitalReturnAddedEvent(_StockWithoutCorporateActions, 1, action.Id, new Date(2001, 01, 01), "Capital Return", new Date(2001, 01, 15), 10.00m));
            @event.ChildActions.Add(new SplitConsolidationAddedEvent(_StockWithoutCorporateActions, 1, action.Id, new Date(2001, 01, 01), "Split", 1, 2));
            _Events.Should().BeEquivalentTo(new[] { @event }, x => x.Excluding(x => x.ChildActions[0].ActionId).Excluding(x => x.ChildActions[1].ActionId));
        }

        [Fact]
        public void AddDividend()
        {
            var action = new RestApi.CorporateActions.Dividend()
            {
                Id = Guid.NewGuid(),
                Stock = _StockWithoutCorporateActions,
                ActionDate = new Date(2001, 01, 01),
                Description = "Dividend",
                PaymentDate = new Date(2001, 01, 15),
                Amount = 10.00m,
                PercentFranked = 0.30m,
                DrpPrice = 1.00m
            };

            var result = _Service.AddCorporateAction(_StockWithoutCorporateActions, action);

            result.Should().HaveOkStatus();
            _Events.Should().BeEquivalentTo(new[]
            {
                new DividendAddedEvent(_StockWithoutCorporateActions, 1, action.Id, new Date(2001, 01, 01), "Dividend", new Date(2001, 01, 15), 10.00m, 0.30m, 1.00m)
            });
        }

        [Fact]
        public void AddSplitConsolidation()
        {
            var action = new RestApi.CorporateActions.SplitConsolidation()
            {
                Id = Guid.NewGuid(),
                Stock = _StockWithoutCorporateActions,
                ActionDate = new Date(2001, 01, 01),
                Description = "Split",
                OriginalUnits = 1,
                NewUnits = 2
            };

            var result = _Service.AddCorporateAction(_StockWithoutCorporateActions, action);

            result.Should().HaveOkStatus();
            _Events.Should().BeEquivalentTo(new[]
            {
                new SplitConsolidationAddedEvent(_StockWithoutCorporateActions, 1, action.Id, new Date(2001, 01, 01), "Split", 1, 2)
            });
        }


        [Fact]
        public void AddTransformation()
        {
            var action = new RestApi.CorporateActions.Transformation()
            {
                Id = Guid.NewGuid(),
                Stock = _StockWithoutCorporateActions,
                ActionDate = new Date(2001, 01, 01),
                Description = "Transformation",
                ImplementationDate = new Date(2001, 01, 15),
                CashComponent = 10.15m,
                RolloverRefliefApplies = true
            };
            action.ResultingStocks.Add(new RestApi.CorporateActions.Transformation.ResultingStock()
            {
                Stock = _StockWithCorporateActions,
                AquisitionDate = new Date(2001, 04, 01),
                CostBase = 0.50m,          
                OriginalUnits = 1,
                NewUnits = 2
            });

            var result = _Service.AddCorporateAction(_StockWithoutCorporateActions, action);

            result.Should().HaveOkStatus();
            var resultStocks = new TransformationAddedEvent.ResultingStock(_StockWithCorporateActions, 1, 2, 0.50m, new Date(2001, 04, 01));
            _Events.Should().BeEquivalentTo(new[]
            {
                new TransformationAddedEvent(_StockWithoutCorporateActions, 1, action.Id, new Date(2001, 01, 01), "Transformation", new Date(2001, 01, 15), 10.15m, true, new [] {resultStocks })
            });
        }
    }
}
