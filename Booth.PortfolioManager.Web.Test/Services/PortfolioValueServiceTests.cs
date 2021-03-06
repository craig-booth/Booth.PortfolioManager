﻿using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.RestApi.Portfolios;
using Booth.PortfolioManager.RestApi.Stocks;

namespace Booth.PortfolioManager.Web.Test.Services
{
    public class PortfolioValueServiceTests
    {
        [Fact]
        public void PortfolioNotFound()
        {
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31));

            var service = new PortfolioValueService(null, PortfolioTestCreator.TradingCalendar);

            var result = service.GetValue(dateRange, ValueFrequency.Day);

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetValueDaily()
        {
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 01, 10));

            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var service = new PortfolioValueService(portfolio, PortfolioTestCreator.TradingCalendar);

            var result = service.GetValue(dateRange, ValueFrequency.Day);

            result.Result.Should().BeEquivalentTo(new
            {
                Values = new[]
                {
                    new ClosingPrice() {Date = new Date(2000, 01, 03), Price = 9963.10m},
                    new ClosingPrice() {Date = new Date(2000, 01, 04), Price = 9960.10m},
                    new ClosingPrice() {Date = new Date(2000, 01, 05), Price = 9969.10m},
                    new ClosingPrice() {Date = new Date(2000, 01, 06), Price = 9966.10m},
                    new ClosingPrice() {Date = new Date(2000, 01, 07), Price = 9963.10m},
                    new ClosingPrice() {Date = new Date(2000, 01, 10), Price = 9975.10m}
                }
            }); 

        }

        [Fact]
        public void GetValueWeekly()
        {
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 01, 17));

            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var service = new PortfolioValueService(portfolio, PortfolioTestCreator.TradingCalendar);

            var result = service.GetValue(dateRange, ValueFrequency.Week);

            result.Result.Should().BeEquivalentTo(new
            {
                Values = new[]
                {
                    new ClosingPrice() {Date = new Date(2000, 01, 02), Price = 9960.10m},
                    new ClosingPrice() {Date = new Date(2000, 01, 09), Price = 9963.10m},
                    new ClosingPrice() {Date = new Date(2000, 01, 16), Price = 9975.10m},
                    new ClosingPrice() {Date = new Date(2000, 01, 17), Price = 9982.10m}
                }
            });
        }

        [Fact]
        public void GetValueMonthly()
        {
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 05, 25));

            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var service = new PortfolioValueService(portfolio, PortfolioTestCreator.TradingCalendar);

            var result = service.GetValue(dateRange, ValueFrequency.Month);

            result.Result.Should().BeEquivalentTo(new
            {
                Values = new[]
                {
                    new ClosingPrice() {Date = new Date(2000, 01, 03), Price = 9963.10m},
                    new ClosingPrice() {Date = new Date(2000, 01, 31), Price = 9985.10m},
                    new ClosingPrice() {Date = new Date(2000, 02, 29), Price = 9988.10m},
                    new ClosingPrice() {Date = new Date(2000, 03, 31), Price = 9981.10m},
                    new ClosingPrice() {Date = new Date(2000, 04, 30), Price = 9981.10m},
                    new ClosingPrice() {Date = new Date(2000, 05, 25), Price = 9969.10m}
                }
            });
        }

        [Fact]
        public void GetValueForStockNotFound()
        {
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31));

            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var service = new PortfolioValueService(portfolio, PortfolioTestCreator.TradingCalendar);

            var result = service.GetValue(Guid.Empty, dateRange, ValueFrequency.Day);

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetValueForStockNotOwned()
        {
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31));

            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var service = new PortfolioValueService(portfolio, PortfolioTestCreator.TradingCalendar);

            var result = service.GetValue(Guid.NewGuid(), dateRange, ValueFrequency.Day);

            result.Should().HaveNotFoundStatus();
        }


        [Fact]
        public void GetValueForStockDaily()
        {
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 01, 10));

            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var service = new PortfolioValueService(portfolio, PortfolioTestCreator.TradingCalendar);

            var result = service.GetValue(PortfolioTestCreator.Stock_ARG.Id, dateRange, ValueFrequency.Day);

            result.Result.Should().BeEquivalentTo(new
            {
                Values = new[]
                {
                    new ClosingPrice() {Date = new Date(2000, 01, 03), Price = 101.00m},
                    new ClosingPrice() {Date = new Date(2000, 01, 04), Price = 100.00m},
                    new ClosingPrice() {Date = new Date(2000, 01, 05), Price = 103.00m},
                    new ClosingPrice() {Date = new Date(2000, 01, 06), Price = 102.00m},
                    new ClosingPrice() {Date = new Date(2000, 01, 07), Price = 101.00m},
                    new ClosingPrice() {Date = new Date(2000, 01, 10), Price = 105.00m}
                }
            });
        }

        [Fact]
        public void GetValueForStockWeekly()
        {
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 01, 17));

            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var service = new PortfolioValueService(portfolio, PortfolioTestCreator.TradingCalendar);

            var result = service.GetValue(PortfolioTestCreator.Stock_ARG.Id, dateRange, ValueFrequency.Week);

            result.Result.Should().BeEquivalentTo(new
            {
                Values = new[]
                {
                    new ClosingPrice() {Date = new Date(2000, 01, 02), Price = 100.00m},
                    new ClosingPrice() {Date = new Date(2000, 01, 09), Price = 101.00m},
                    new ClosingPrice() {Date = new Date(2000, 01, 16), Price = 107.00m},
                    new ClosingPrice() {Date = new Date(2000, 01, 17), Price = 108.00m}
                }
            });
        }

        [Fact]
        public void GetValueForStockMonthly()
        {
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 05, 25));

            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var service = new PortfolioValueService(portfolio, PortfolioTestCreator.TradingCalendar);

            var result = service.GetValue(PortfolioTestCreator.Stock_ARG.Id, dateRange, ValueFrequency.Month);

            result.Result.Should().BeEquivalentTo(new
            {
                Values = new[]
                {
                    new ClosingPrice() {Date = new Date(2000, 01, 03), Price = 101.00m},
                    new ClosingPrice() {Date = new Date(2000, 01, 31), Price = 109.00m},
                    new ClosingPrice() {Date = new Date(2000, 02, 29), Price = 110.00m},
                    new ClosingPrice() {Date = new Date(2000, 03, 31), Price = 107.00m},
                    new ClosingPrice() {Date = new Date(2000, 04, 30), Price = 107.00m},
                    new ClosingPrice() {Date = new Date(2000, 05, 25), Price = 103.00m}
                }
            });
        }

    } 
}
