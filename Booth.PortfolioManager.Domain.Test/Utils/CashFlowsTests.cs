using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;

using Booth.Common;
using Booth.PortfolioManager.Domain.Utils;


namespace Booth.PortfolioManager.Domain.Test.Utils
{
    public class CashFlowsTests
    {

        [Fact]
        public void AddToEmptyList()
        {
            var cashFlows = new CashFlows();

            cashFlows.Add(new Date(2000, 01, 01), 100.00m);
     
            using (new AssertionScope())
            {
                cashFlows.Count.Should().Be(1);
                cashFlows[new Date(2000, 01, 01)].Should().Be(100.00m);
            }
        }

        [Fact]
        public void AddNewDate()
        {
            var cashFlows = new CashFlows();

            cashFlows.Add(new Date(2000, 01, 01), 100.00m);
            cashFlows.Add(new Date(2001, 06, 30), 200.00m);

            using (new AssertionScope())
            {
                cashFlows.Count.Should().Be(2);
                cashFlows[new Date(2000, 01, 01)].Should().Be(100.00m);
                cashFlows[new Date(2001, 06, 30)].Should().Be(200.00m);
            }

        }

        [Fact]
        public void AddMatchingExistingDate()
        {
            var cashFlows = new CashFlows();

            cashFlows.Add(new Date(2000, 01, 01), 100.00m);
            cashFlows.Add(new Date(2000, 01, 01), 200.00m);

            using (new AssertionScope())
            {
                cashFlows.Count.Should().Be(1);
                cashFlows[new Date(2000, 01, 01)].Should().Be(300.00m);
            }
        }

        [Fact]
        public void GetCashFlowsFinalDateBeforeStartDate()
        {
            var cashFlows = new CashFlows();

            Action a = () => cashFlows.GetCashFlows(new Date(2001, 01, 01), 10.00m, new Date(2000, 01, 01), 20.00m, out var values, out var periods);

            a.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetCashFlowsStartAndFinalDateSame()
        {
            var cashFlows = new CashFlows();

            Action a = () => cashFlows.GetCashFlows(new Date(2000, 01, 01), 10.00m, new Date(2000, 01, 01), 20.00m, out var values, out var periods);

            a.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetCashFlowsNegativeInitialInvestment()
        {
            var cashFlows = new CashFlows();

            Action a = () => cashFlows.GetCashFlows(new Date(2000, 01, 01), -10.00m, new Date(2001, 01, 01), 20.00m, out var values, out var periods);

            a.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetCashFlowsEmptyList()
        {
            var cashFlows = new CashFlows();

            cashFlows.GetCashFlows(new Date(2000, 01, 01), 10.00m, new Date(2001, 01, 01), 20.00m, out var values, out var periods);

            using (new AssertionScope())
            {
                values.Should().Equal(new double[] { -10d, 20d });
                periods.Should().Equal(new double[] { 0, 366 / 365d });
            }
        }

        [Fact]
        public void GetCashFlowsOnlyZeroValues()
        {
            var cashFlows = new CashFlows();

            cashFlows.Add(new Date(2000, 02, 01), 0.00m);
            cashFlows.Add(new Date(2000, 04, 01), 0.00m);
            cashFlows.Add(new Date(2000, 03, 01), 0.00m);
            cashFlows.Add(new Date(2000, 04, 01), 0.00m);
            cashFlows.Add(new Date(2000, 05, 01), 0.00m);

            cashFlows.GetCashFlows(new Date(2000, 01, 01), 10.00m, new Date(2001, 01, 01), 20.00m, out var values, out var periods);

            using (new AssertionScope())
            {
                values.Should().Equal(new double[] { -10d, 20d });
                periods.Should().Equal(new double[] { 0, 366 / 365d });
            }
        }

        [Fact]
        public void GetCashFlowsAllEntries()
        {
            var cashFlows = new CashFlows();

            cashFlows.Add(new Date(2000, 02, 01), 100.00m);
            cashFlows.Add(new Date(2000, 04, 01), 200.00m);
            cashFlows.Add(new Date(2000, 03, 01), 300.00m);
            cashFlows.Add(new Date(2000, 04, 01), 400.00m);
            cashFlows.Add(new Date(2000, 05, 01), 500.00m);

            cashFlows.GetCashFlows(new Date(2000, 01, 01), 10.00m, new Date(2001, 01, 01), 600.00m, out var values, out var periods);

            using (new AssertionScope())
            {
                values.Should().Equal(new double[] { -10d, 100.00d, 300.00d, 600.00d, 500.00d, 600d});
                periods.Should().Equal(new double[] { 0, 31 / 365d, (31 + 29) / 365d, (31 + 29 + 31) / 365d, (31 + 29 + 31 + 30) / 365d, 366 /365d });
            }
        }

        [Fact]
        public void GetCashFlowsEntryMatchingStartDate()
        {
            var cashFlows = new CashFlows();

            cashFlows.Add(new Date(2000, 01, 01), 100.00m);
            cashFlows.Add(new Date(2000, 03, 01), 200.00m);
            cashFlows.Add(new Date(2000, 02, 01), 300.00m);
            cashFlows.Add(new Date(2000, 03, 01), 400.00m);
            cashFlows.Add(new Date(2000, 04, 01), 500.00m);

            cashFlows.GetCashFlows(new Date(2000, 01, 01), 10.00m, new Date(2001, 01, 01), 600.00m, out var values, out var periods);

            using (new AssertionScope())
            {
                values.Should().Equal(new double[] { -10d, 300.00d, 600.00d, 500.00d, 600d });
                periods.Should().Equal(new double[] { 0, 31 / 365d, (31 + 29) / 365d, (31 + 29 + 31) / 365d, 366 / 365d });
            }
        }

        [Fact]
        public void GetCashFlowsEntryMatchingEndDate()
        {
            var cashFlows = new CashFlows();

            cashFlows.Add(new Date(2000, 02, 01), 100.00m);
            cashFlows.Add(new Date(2000, 04, 01), 200.00m);
            cashFlows.Add(new Date(2000, 03, 01), 300.00m);
            cashFlows.Add(new Date(2000, 04, 01), 400.00m);
            cashFlows.Add(new Date(2000, 05, 01), 500.00m);

            cashFlows.GetCashFlows(new Date(2000, 01, 01), 10.00m, new Date(2000, 05, 01), 600.00m, out var values, out var periods);

            using (new AssertionScope())
            {
                values.Should().Equal(new double[] { -10d, 100.00d, 300.00d, 600.00d, 1100d });
                periods.Should().Equal(new double[] { 0, 31 / 365d, (31 + 29) / 365d, (31 + 29 + 31) / 365d, (31 + 29 + 31 + 30) / 365d });
            }
        }

        [Fact]
        public void GetCashFlowsPartialList()
        {
            var cashFlows = new CashFlows();

            cashFlows.Add(new Date(2000, 01, 01), 100.00m);
            cashFlows.Add(new Date(2000, 03, 01), 200.00m);
            cashFlows.Add(new Date(2000, 02, 01), 300.00m);
            cashFlows.Add(new Date(2000, 03, 01), 400.00m);
            cashFlows.Add(new Date(2000, 04, 01), 500.00m);

            cashFlows.GetCashFlows(new Date(2000, 01, 15), 10.00m, new Date(2000, 03, 15), 600.00m, out var values, out var periods);

            using (new AssertionScope())
            {
                values.Should().Equal(new double[] { -10d, 300.00d, 600.00d, 600.00d });
                periods.Should().Equal(new double[] { 0, 17 / 365d, (17 + 29) / 365d, (17 + 29 + 14) / 365d });
            }
        }

        
        [Fact]
        public void GetCashFlowsIgnoreZeroAmount()
        {
            var cashFlows = new CashFlows();

            cashFlows.Add(new Date(2000, 01, 01), 100.00m);
            cashFlows.Add(new Date(2000, 03, 01), 200.00m);
            cashFlows.Add(new Date(2000, 02, 01), 0.00m);
            cashFlows.Add(new Date(2000, 02, 15), 500.00m);
            cashFlows.Add(new Date(2000, 02, 15), -500.00m);
            cashFlows.Add(new Date(2000, 03, 01), 400.00m);
            cashFlows.Add(new Date(2000, 04, 01), 500.00m);

            cashFlows.GetCashFlows(new Date(2000, 01, 15), 10.00m, new Date(2000, 03, 15), 600.00m, out var values, out var periods);

            using (new AssertionScope())
            {
                values.Should().Equal(new double[] { -10d, 600.00d, 600.00d });
                periods.Should().Equal(new double[] { 0, (17 + 29) / 365d, (17 + 29 + 14) / 365d });
            }
        }
    }
}
