using Booth.Common;
using Booth.PortfolioManager.Web.Services;
using FluentAssertions;
using System;
using Xunit;

namespace Booth.PortfolioManager.Web.Test.Services
{
    [Collection(Services.Collection)]
    public class PortfolioCapitalGainsServiceTests
    {
        private readonly ServicesTestFixture _Fixture;
        public PortfolioCapitalGainsServiceTests(ServicesTestFixture fixture)
        {
            _Fixture = fixture;
        }

        [Fact]
        public void PortfolioNotFound()
        {
            var service = new PortfolioCapitalGainsService(null);

            var result = service.GetCapitalGains(new Date(2000, 01, 01));

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetCapitalGains()
        {
            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioCapitalGainsService(portfolio);

            var result = service.GetCapitalGains(new Date(2010, 01, 01));

            result.Result.Should().BeEquivalentTo(new
            {
                UnrealisedGains = new[]
                {
                    new RestApi.Portfolios.SimpleUnrealisedGainsItem()
                    {
                        Stock = _Fixture.Stock_ARG,
                        AquisitionDate = new Date(2000, 01, 01),
                        Units = 50,
                        CostBase = 59.97m,
                        MarketValue = 100m,
                        CapitalGain = 40.03m,
                        DiscoutedGain = 20.02m,
                        DiscountMethod = RestApi.Portfolios.CgtMethod.Discount
                    },
                    new RestApi.Portfolios.SimpleUnrealisedGainsItem()
                    {
                        Stock = _Fixture.Stock_ARG,
                        AquisitionDate = new Date(2003, 01, 01),
                        Units = 100,
                        CostBase = 119.95m,
                        MarketValue = 200m,
                        CapitalGain = 80.05m,
                        DiscoutedGain = 40.03m,
                        DiscountMethod = RestApi.Portfolios.CgtMethod.Discount
                    },
                    new RestApi.Portfolios.SimpleUnrealisedGainsItem()
                    {
                        Stock = _Fixture.Stock_ARG,
                        AquisitionDate = new Date(2005, 01, 01),
                        Units = 100,
                        CostBase = 119.95m,
                        MarketValue = 200m,
                        CapitalGain = 80.05m,
                        DiscoutedGain = 40.03m,
                        DiscountMethod = RestApi.Portfolios.CgtMethod.Discount
                    },
                    new RestApi.Portfolios.SimpleUnrealisedGainsItem()
                    {
                        Stock = _Fixture.Stock_WAM,
                        AquisitionDate = new Date(2000, 01, 01),
                        Units = 200,
                        CostBase = 259.95m,
                        MarketValue = 300m,
                        CapitalGain = 40.05m,
                        DiscoutedGain = 20.03m,
                        DiscountMethod = RestApi.Portfolios.CgtMethod.Discount
                    },
                    new RestApi.Portfolios.SimpleUnrealisedGainsItem()
                    {
                        Stock = _Fixture.Stock_WAM,
                        AquisitionDate = new Date(2005, 01, 03),
                        Units = 5,
                        CostBase = 32.50m,
                        MarketValue = 7.50m,
                        CapitalGain = -25.00m,
                        DiscoutedGain = -25.00m,
                        DiscountMethod = RestApi.Portfolios.CgtMethod.Discount
                    }
                }

            });
        }

        [Fact]
        public void GetCapitalGainsStockNotFound()
        {
            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioCapitalGainsService(portfolio);

            var result = service.GetCapitalGains(Guid.NewGuid(), new Date(2010, 01, 01));

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetCapitalGainsStockNotOwnedAtDate()
        {
            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioCapitalGainsService(portfolio);

            var result = service.GetCapitalGains(_Fixture.Stock_ARG.Id, new Date(1999, 01, 01));

            result.Result.UnrealisedGains.Should().BeEmpty();
        }

        [Fact]
        public void GetCapitalGainsStockOwnedAtDate()
        {
            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioCapitalGainsService(portfolio);

            var result = service.GetCapitalGains(_Fixture.Stock_ARG.Id, new Date(2010, 01, 01));

            result.Result.Should().BeEquivalentTo(new
            {
                UnrealisedGains = new[]
                {
                    new RestApi.Portfolios.SimpleUnrealisedGainsItem()
                    {
                        Stock = _Fixture.Stock_ARG,
                        AquisitionDate = new Date(2000, 01, 01),
                        Units = 50,
                        CostBase = 59.97m,
                        MarketValue = 100m,
                        CapitalGain = 40.03m,
                        DiscoutedGain = 20.02m,
                        DiscountMethod = RestApi.Portfolios.CgtMethod.Discount
                    },
                    new RestApi.Portfolios.SimpleUnrealisedGainsItem()
                    {
                        Stock = _Fixture.Stock_ARG,
                        AquisitionDate = new Date(2003, 01, 01),
                        Units = 100,
                        CostBase = 119.95m,
                        MarketValue = 200m,
                        CapitalGain = 80.05m,
                        DiscoutedGain = 40.03m,
                        DiscountMethod = RestApi.Portfolios.CgtMethod.Discount
                    },
                    new RestApi.Portfolios.SimpleUnrealisedGainsItem()
                    {
                        Stock = _Fixture.Stock_ARG,
                        AquisitionDate = new Date(2005, 01, 01),
                        Units = 100,
                        CostBase = 119.95m,
                        MarketValue = 200m,
                        CapitalGain = 80.05m,
                        DiscoutedGain = 40.03m,
                        DiscountMethod = RestApi.Portfolios.CgtMethod.Discount
                    }
                }

            });
        }

        [Fact]
        public void GetGetDetailedCapitalGainsCapitalGains()
        {
            var portfolio = _Fixture    .CreateDefaultPortfolio();

            var service = new PortfolioCapitalGainsService(portfolio);

            var result = service.GetDetailedCapitalGains(new Date(2010, 01, 01));

            result.Result.Should().BeEquivalentTo(new
            {
                UnrealisedGains = new[]
                {
                    new RestApi.Portfolios.DetailedUnrealisedGainsItem()
                    {
                        Stock = _Fixture.Stock_ARG,
                        AquisitionDate = new Date(2000, 01, 01),
                        Units = 50,
                        CostBase = 59.97m,
                        MarketValue = 100m,
                        CapitalGain = 40.03m,
                        DiscoutedGain = 20.02m,
                        DiscountMethod = RestApi.Portfolios.CgtMethod.Discount,
                        CGTEvents = {
                            new RestApi.Portfolios.DetailedUnrealisedGainsItem.CGTEventItem()
                            {
                                Date = new Date(2000, 01, 01),
                                Description = "Aquired 100 shares @ $1.00",
                                UnitChange = 100,
                                Units = 100,
                                CostBaseChange = 119.95m,
                                CostBase = 119.95m
                            },
                            new RestApi.Portfolios.DetailedUnrealisedGainsItem.CGTEventItem()
                            {
                                Date = new Date(2004, 01, 01),
                                Description = "Disposed of 50 shares @ $1.02",
                                UnitChange = -50,
                                Units = 50,
                                CostBaseChange = -59.98m,
                                CostBase = 59.97m
                            }
                        }
                    },
                    new RestApi.Portfolios.DetailedUnrealisedGainsItem()
                    {
                        Stock = _Fixture.Stock_ARG,
                        AquisitionDate = new Date(2003, 01, 01),
                        Units = 100,
                        CostBase = 119.95m,
                        MarketValue = 200m,
                        CapitalGain = 80.05m,
                        DiscoutedGain = 40.03m,
                        DiscountMethod = RestApi.Portfolios.CgtMethod.Discount,
                        CGTEvents = {
                            new RestApi.Portfolios.DetailedUnrealisedGainsItem.CGTEventItem()
                            {
                                Date = new Date(2003, 01, 01),
                                Description = "Aquired 100 shares @ $1.00",
                                UnitChange = 100,
                                Units = 100,
                                CostBaseChange = 119.95m,
                                CostBase = 119.95m
                            }
                        }
                    },
                    new RestApi.Portfolios.DetailedUnrealisedGainsItem()
                    {
                        Stock = _Fixture.Stock_ARG,
                        AquisitionDate = new Date(2005, 01, 01),
                        Units = 100,
                        CostBase = 119.95m,
                        MarketValue = 200m,
                        CapitalGain = 80.05m,
                        DiscoutedGain = 40.03m,
                        DiscountMethod = RestApi.Portfolios.CgtMethod.Discount,
                        CGTEvents = {
                            new RestApi.Portfolios.DetailedUnrealisedGainsItem.CGTEventItem()
                            {
                                Date = new Date(2005, 01, 01),
                                Description = "Aquired 100 shares @ $1.00",
                                UnitChange = 100,
                                Units = 100,
                                CostBaseChange = 119.95m,
                                CostBase = 119.95m
                            }
                        }
                    },
                    new RestApi.Portfolios.DetailedUnrealisedGainsItem()
                    {
                        Stock = _Fixture.Stock_WAM,
                        AquisitionDate = new Date(2000, 01, 01),
                        Units = 200,
                        CostBase = 259.95m,
                        MarketValue = 300m,
                        CapitalGain = 40.05m,
                        DiscoutedGain = 20.03m,
                        DiscountMethod = RestApi.Portfolios.CgtMethod.Discount,
                        CGTEvents = {
                            new RestApi.Portfolios.DetailedUnrealisedGainsItem.CGTEventItem()
                            {
                                Date = new Date(2000, 01, 01),
                                Description = "Aquired 200 shares @ $1.20",
                                UnitChange = 200,
                                Units = 200,
                                CostBaseChange = 259.95m,
                                CostBase = 259.95m
                            }
                        }
                    },
                    new RestApi.Portfolios.DetailedUnrealisedGainsItem()
                    {
                        Stock = _Fixture.Stock_WAM,
                        AquisitionDate = new Date(2005, 01, 03),
                        Units = 5,
                        CostBase = 32.50m,
                        MarketValue = 7.50m,
                        CapitalGain = -25.00m,
                        DiscoutedGain = -25.00m,
                        DiscountMethod = RestApi.Portfolios.CgtMethod.Discount,
                        CGTEvents = {
                            new RestApi.Portfolios.DetailedUnrealisedGainsItem.CGTEventItem()
                            {
                                Date = new Date(2005, 01, 03),
                                Description = "Opening balance of 5 shares",
                                UnitChange = 5,
                                Units = 5,
                                CostBaseChange = 32.50m,
                                CostBase = 32.50m
                            }
                        }
                    }
                }

            });
        }

        [Fact]
        public void GetGetDetailedCapitalGainsCapitalGainsStockNotFound()
        {
            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioCapitalGainsService(portfolio);

            var result = service.GetDetailedCapitalGains(Guid.NewGuid(), new Date(2010, 01, 01));

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetGetDetailedCapitalGainsCapitalGainsStockNotOwnedAtDate()
        {
            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioCapitalGainsService(portfolio);

            var result = service.GetDetailedCapitalGains(_Fixture.Stock_ARG.Id, new Date(1999, 01, 01));

            result.Result.UnrealisedGains.Should().BeEmpty();
        }

        [Fact]
        public void GetGetDetailedCapitalGainsCapitalGainsStockOwnedtDate()
        {
            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioCapitalGainsService(portfolio);

            var result = service.GetDetailedCapitalGains(_Fixture.Stock_ARG.Id, new Date(2010, 01, 01));

            result.Result.Should().BeEquivalentTo(new
            {
                UnrealisedGains = new[]
                {
                    new RestApi.Portfolios.DetailedUnrealisedGainsItem()
                    {
                        Stock = _Fixture.Stock_ARG,
                        AquisitionDate = new Date(2000, 01, 01),
                        Units = 50,
                        CostBase = 59.97m,
                        MarketValue = 100m,
                        CapitalGain = 40.03m,
                        DiscoutedGain = 20.02m,
                        DiscountMethod = RestApi.Portfolios.CgtMethod.Discount,
                        CGTEvents = {
                            new RestApi.Portfolios.DetailedUnrealisedGainsItem.CGTEventItem()
                            {
                                Date = new Date(2000, 01, 01),
                                Description = "Aquired 100 shares @ $1.00",
                                UnitChange = 100,
                                Units = 100,
                                CostBaseChange = 119.95m,
                                CostBase = 119.95m
                            },
                            new RestApi.Portfolios.DetailedUnrealisedGainsItem.CGTEventItem()
                            {
                                Date = new Date(2004, 01, 01),
                                Description = "Disposed of 50 shares @ $1.02",
                                UnitChange = -50,
                                Units = 50,
                                CostBaseChange = -59.98m,
                                CostBase = 59.97m
                            }
                        }
                    },
                    new RestApi.Portfolios.DetailedUnrealisedGainsItem()
                    {
                        Stock = _Fixture.Stock_ARG,
                        AquisitionDate = new Date(2003, 01, 01),
                        Units = 100,
                        CostBase = 119.95m,
                        MarketValue = 200m,
                        CapitalGain = 80.05m,
                        DiscoutedGain = 40.03m,
                        DiscountMethod = RestApi.Portfolios.CgtMethod.Discount,
                        CGTEvents = {
                            new RestApi.Portfolios.DetailedUnrealisedGainsItem.CGTEventItem()
                            {
                                Date = new Date(2003, 01, 01),
                                Description = "Aquired 100 shares @ $1.00",
                                UnitChange = 100,
                                Units = 100,
                                CostBaseChange = 119.95m,
                                CostBase = 119.95m
                            }
                        }
                    },
                    new RestApi.Portfolios.DetailedUnrealisedGainsItem()
                    {
                        Stock = _Fixture.Stock_ARG,
                        AquisitionDate = new Date(2005, 01, 01),
                        Units = 100,
                        CostBase = 119.95m,
                        MarketValue = 200m,
                        CapitalGain = 80.05m,
                        DiscoutedGain = 40.03m,
                        DiscountMethod = RestApi.Portfolios.CgtMethod.Discount,
                        CGTEvents = {
                            new RestApi.Portfolios.DetailedUnrealisedGainsItem.CGTEventItem()
                            {
                                Date = new Date(2005, 01, 01),
                                Description = "Aquired 100 shares @ $1.00",
                                UnitChange = 100,
                                Units = 100,
                                CostBaseChange = 119.95m,
                                CostBase = 119.95m
                            }
                        }
                    }
                }

            });
        }
    }
}
