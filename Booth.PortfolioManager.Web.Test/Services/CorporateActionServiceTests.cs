using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Xunit;
using FluentAssertions;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Web.Utilities;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.CorporateActions;
using Booth.PortfolioManager.Web.Mappers;

namespace Booth.PortfolioManager.Web.Test.Services
{
    public class CorporateActionServiceTests
    {
        private Stock _StockWithoutCorporateActions;
        private Stock _StockWithCorporateActions;

        private readonly Guid _Action1 = Guid.NewGuid();
        private readonly Guid _Action2 = Guid.NewGuid();
        private readonly Guid _NewAction = Guid.NewGuid();

        private readonly CorporateActionService _Service;

        private readonly Mock<IStockRepository> _RepositoryMock;

        public CorporateActionServiceTests()
        {        
            var stockCache = new EntityCache<Stock>();
            var stockQuery = new StockQuery(stockCache);
            var stockResolver = new StockResolver(stockCache);

            _StockWithoutCorporateActions = new Stock(Guid.NewGuid());
            _StockWithoutCorporateActions.List("ABC", "ABC Pty Ltd", new Date(2000, 01, 01), false, AssetCategory.AustralianStocks);
            stockCache.Add(_StockWithoutCorporateActions);

            _StockWithCorporateActions = new Stock(Guid.NewGuid());
            _StockWithCorporateActions.List("XYZ", "XYZ Pty Ltd", new Date(2000, 01, 01), false, AssetCategory.AustralianStocks);

            _StockWithCorporateActions.CorporateActions.Add(new CapitalReturn(_Action1, _StockWithCorporateActions, new Date(2001, 01, 01), "Action 1", new Date(2001, 01, 02), 10.00m));
            _StockWithCorporateActions.CorporateActions.Add(new Dividend(_Action2, _StockWithCorporateActions, new Date(2001, 01, 01), "Action 2", new Date(2001, 01, 02), 10.00m, 1.00m, 2.45m));

            stockCache.Add(_StockWithCorporateActions);

            _RepositoryMock = new Mock<IStockRepository>(MockBehavior.Loose);
            _Service = new CorporateActionService(stockQuery, _RepositoryMock.Object, new CorporateActionMapper(stockResolver));
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
            var result = _Service.GetCorporateAction(_StockWithoutCorporateActions.Id, Guid.NewGuid());

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetCorporateAction()
        {
            var result = _Service.GetCorporateAction(_StockWithCorporateActions.Id, _Action2);

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
            var result = _Service.GetCorporateActions(_StockWithCorporateActions.Id, new DateRange(Date.MinValue, Date.MaxValue));

            result.Result.Should().HaveCount(2);
        }

        [Fact]
        public async Task AddCorporateActionStockNotFound()
        {
            var dividend = new Models.CorporateAction.Dividend();

            var result = await _Service.AddCorporateActionAsync(Guid.NewGuid(), dividend);

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public async Task AddCapitalReturn()
        {
            var action = new Models.CorporateAction.CapitalReturn()
            {
                Id = _NewAction,
                Stock = _StockWithoutCorporateActions.Id,
                ActionDate = new Date(2001, 01, 01),
                Description = "Capital Return",
                PaymentDate = new Date(2001, 01, 15),
                Amount = 10.00m
            };

            var result = await _Service.AddCorporateActionAsync(_StockWithoutCorporateActions.Id, action);

            result.Should().HaveOkStatus();
            _StockWithoutCorporateActions.CorporateActions.Should().ContainEquivalentOf<Domain.CorporateActions.CapitalReturn>(
                new Domain.CorporateActions.CapitalReturn(action.Id, _StockWithoutCorporateActions, action.ActionDate, action.Description, action.PaymentDate, action.Amount)
                );

            _RepositoryMock.Verify(x => x.AddCorporateActionAsync(_StockWithoutCorporateActions, _NewAction));
        }

        [Fact]
        public async Task AddCompositeAction()
        {
            var action = new Models.CorporateAction.CompositeAction()
            {
                Id = _NewAction,
                Stock = _StockWithoutCorporateActions.Id,
                ActionDate = new Date(2001, 01, 01),
                Description = "Restructure"
            };
            var child1 = Guid.NewGuid();
            action.ChildActions.Add(new Models.CorporateAction.CapitalReturn()
            {
                Id = child1,
                Stock = _StockWithoutCorporateActions.Id,
                ActionDate = new Date(2001, 01, 01),
                Description = "Capital Return",
                PaymentDate = new Date(2001, 01, 15),
                Amount = 10.00m
            });
            var child2 = Guid.NewGuid();
            action.ChildActions.Add(new Models.CorporateAction.SplitConsolidation()
            {
                Id = child2,
                Stock = _StockWithoutCorporateActions.Id,
                ActionDate = new Date(2001, 01, 01),
                Description = "Split",
                OriginalUnits = 1,
                NewUnits = 2
            });

            var result = await _Service.AddCorporateActionAsync(_StockWithoutCorporateActions.Id, action);

            result.Should().HaveOkStatus();

            _StockWithoutCorporateActions.CorporateActions.Should().ContainEquivalentOf<Domain.CorporateActions.CompositeAction>(
                new Domain.CorporateActions.CompositeAction(action.Id, _StockWithoutCorporateActions, action.ActionDate, action.Description, new ICorporateAction[]
                {
                    new Domain.CorporateActions.CapitalReturn(Guid.Empty, _StockWithoutCorporateActions, action.ActionDate, "Capital Return", new Date(2001, 01, 15), 10.00m),
                    new Domain.CorporateActions.SplitConsolidation(Guid.Empty, _StockWithoutCorporateActions, action.ActionDate, "Split", 1, 2)
                })
            , opts => opts.Excluding(x => x.Path.EndsWith(".Id")));

            _RepositoryMock.Verify(x => x.AddCorporateActionAsync(_StockWithoutCorporateActions, _NewAction));
        }

        [Fact]
        public async Task AddDividend()
        {
            var action = new Models.CorporateAction.Dividend()
            {
                Id = _NewAction,
                Stock = _StockWithoutCorporateActions.Id,
                ActionDate = new Date(2001, 01, 01),
                Description = "Dividend",
                PaymentDate = new Date(2001, 01, 15),
                Amount = 10.00m,
                PercentFranked = 0.30m,
                DrpPrice = 1.00m
            };

            var result = await _Service.AddCorporateActionAsync(_StockWithoutCorporateActions.Id, action);

            result.Should().HaveOkStatus();
            _StockWithoutCorporateActions.CorporateActions.Should().ContainEquivalentOf<Domain.CorporateActions.Dividend>(
                new Domain.CorporateActions.Dividend(action.Id, _StockWithoutCorporateActions, action.ActionDate, action.Description, action.PaymentDate, action.Amount, action.PercentFranked, action.DrpPrice)
            );

            _RepositoryMock.Verify(x => x.AddCorporateActionAsync(_StockWithoutCorporateActions, _NewAction));
        }

        [Fact]
        public async Task AddSplitConsolidation()
        {
            var action = new Models.CorporateAction.SplitConsolidation()
            {
                Id = _NewAction,
                Stock = _StockWithoutCorporateActions.Id,
                ActionDate = new Date(2001, 01, 01),
                Description = "Split",
                OriginalUnits = 1,
                NewUnits = 2
            };

            var result = await _Service.AddCorporateActionAsync(_StockWithoutCorporateActions.Id, action);

            result.Should().HaveOkStatus();

            _StockWithoutCorporateActions.CorporateActions.Should().ContainEquivalentOf<Domain.CorporateActions.SplitConsolidation>(
                new Domain.CorporateActions.SplitConsolidation(action.Id, _StockWithoutCorporateActions, action.ActionDate, action.Description, action.OriginalUnits, action.NewUnits)
            );

            _RepositoryMock.Verify(x => x.AddCorporateActionAsync(_StockWithoutCorporateActions, _NewAction));
        }


        [Fact]
        public async Task AddTransformation()
        {
            var action = new Models.CorporateAction.Transformation()
            {
                Id = _NewAction,
                Stock = _StockWithoutCorporateActions.Id,
                ActionDate = new Date(2001, 01, 01),
                Description = "Transformation",
                ImplementationDate = new Date(2001, 01, 15),
                CashComponent = 10.15m,
                RolloverRefliefApplies = true
            };
            action.ResultingStocks.Add(new Models.CorporateAction.Transformation.ResultingStock()
            {
                Stock = _StockWithCorporateActions.Id,
                AquisitionDate = new Date(2001, 04, 01),
                CostBase = 0.50m,
                OriginalUnits = 1,
                NewUnits = 2
            });

            var result = await _Service.AddCorporateActionAsync(_StockWithoutCorporateActions.Id, action);

            result.Should().HaveOkStatus();
            _StockWithoutCorporateActions.CorporateActions.Should().ContainEquivalentOf<Domain.CorporateActions.Transformation>(
                new Domain.CorporateActions.Transformation(action.Id, _StockWithoutCorporateActions, action.ActionDate, action.Description, action.ImplementationDate, action.CashComponent, action.RolloverRefliefApplies, new Domain.CorporateActions.Transformation.ResultingStock[]
                {
                    new Domain.CorporateActions.Transformation.ResultingStock(_StockWithCorporateActions.Id, 1, 2, 0.5m, new Date(2001, 04, 01))
                }));

            _RepositoryMock.Verify(x => x.AddCorporateActionAsync(_StockWithoutCorporateActions, _NewAction));
        }

        [Fact]
        public async Task UpdateCorporateActionStockNotFound()
        {
            var dividend = new Models.CorporateAction.Dividend() { Id = _Action1 };

            var result = await _Service.UpdateCorporateActionAsync(Guid.NewGuid(), dividend);

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public async Task UpdateCorporateActionNotFound()
        {
            var dividend = new Models.CorporateAction.Dividend() { Id = Guid.NewGuid() };

            var result = await _Service.UpdateCorporateActionAsync(_StockWithCorporateActions.Id, dividend);

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public async Task UpdateCorporateAction()
        {
            var capitalReturn = new Models.CorporateAction.CapitalReturn()
            {
                Id = _Action1,
                Stock = _StockWithCorporateActions.Id,
                ActionDate = new Date(2001, 01, 01),
                Description = "Updated",
                PaymentDate = new Date(2001, 01, 02),
                Amount = 13.00m
            };

            var result = await _Service.UpdateCorporateActionAsync(_StockWithCorporateActions.Id, capitalReturn);

            result.Should().HaveOkStatus();
            _StockWithCorporateActions.CorporateActions.Should().ContainEquivalentOf<Domain.CorporateActions.CapitalReturn>(
                new Domain.CorporateActions.CapitalReturn(_Action1, _StockWithCorporateActions, new Date(2001, 01, 01), "Updated", new Date(2001, 01, 02), 13.00m)
                );

            _RepositoryMock.Verify(x => x.UpdateCorporateActionAsync(_StockWithCorporateActions, _Action1));
        }

        [Fact]
        public async Task DeleteCorporateActionStockNotFound()
        {
            var result = await _Service.DeleteCorporateActionAsync(Guid.NewGuid(), _Action1);

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public async Task DeleteCorporateActionNotFound()
        {
            var result = await _Service.DeleteCorporateActionAsync(_StockWithCorporateActions.Id, Guid.NewGuid());

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public async Task DeleteCorporateAction()
        {
            var result = await _Service.DeleteCorporateActionAsync(_StockWithCorporateActions.Id, _Action1);

            result.Should().HaveOkStatus();
            _StockWithCorporateActions.CorporateActions.Should().NotContain(x => x.Id == _Action1);

            _RepositoryMock.Verify(x => x.DeleteCorporateActionAsync(_StockWithCorporateActions, _Action1));
        }
    }
}
