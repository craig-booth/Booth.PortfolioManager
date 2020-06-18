using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.Test.Utils
{
    public class IrrCalculatorTests
    {

        [Fact]
        public void NoCashflows()
        {
            var cashFlows = new CashFlows();

            var result = IrrCalculator.CalculateIrr(new Date(2000, 01, 01), 1000.00m, new Date(2005, 12, 31), 1500.00m, cashFlows);

            result.Should().BeApproximately(0.0698802d, 0.000001d);
        }

        [Fact]
        public void SingleCashFlow()
        {
            var cashFlows = new CashFlows();
            cashFlows.Add(new Date(2001, 01, 01), -1000.00m);

            var result = IrrCalculator.CalculateIrr(new Date(2000, 01, 01), 1000.00m, new Date(2005, 12, 31), 2500.00m, cashFlows);

            result.Should().BeApproximately(0.0413562d, 0.000001d);
        }

        [Fact]
        public void TwoCashFlow()
        {
            var cashFlows = new CashFlows();
            cashFlows.Add(new Date(2001, 01, 01), -1000.00m);
            cashFlows.Add(new Date(2002, 01, 01), 500.00m);

            var result = IrrCalculator.CalculateIrr(new Date(2000, 01, 01), 1000.00m, new Date(2005, 12, 31), 5000.00m, cashFlows);

            result.Should().BeApproximately(0.2244317d, 0.000001d);
        }

        [Fact]
        public void LessThanOneYear()
        {
            var cashFlows = new CashFlows();
            cashFlows.Add(new Date(2000, 02, 01), -1000.00m);
            cashFlows.Add(new Date(2000, 03, 01), 500.00m);
            cashFlows.Add(new Date(2000, 04, 01), -1500.00m);
            cashFlows.Add(new Date(2000, 05, 01), -1500.00m);
            cashFlows.Add(new Date(2000, 06, 01), 500.00m);
            cashFlows.Add(new Date(2000, 07, 01), -2000.00m);

            var result = IrrCalculator.CalculateIrr(new Date(2000, 01, 01), 1000.00m, new Date(2000, 08, 30), 10000.00m, cashFlows);

            result.Should().BeApproximately(2.5071595d, 0.000001d);
        }

        [Fact]
        public void MoreThanOneYear()
        {
            var cashFlows = new CashFlows();
            cashFlows.Add(new Date(2001, 01, 01), -1500.00m);
            cashFlows.Add(new Date(2002, 01, 01), 500.00m);
            cashFlows.Add(new Date(2003, 01, 01), 2000.00m);
            cashFlows.Add(new Date(2004, 01, 01), -5000.00m);
            cashFlows.Add(new Date(2005, 01, 01), 100.00m);

            var result = IrrCalculator.CalculateIrr(new Date(2000, 01, 01), 1000.00m, new Date(2005, 12, 31), 10000.00m, cashFlows);

            result.Should().BeApproximately(0.2220352d, 0.000001d);
        }
    }
}
