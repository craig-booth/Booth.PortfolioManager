using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

using Booth.Common;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.Test.Utils
{
    class CashFlowsTests
    {

        [TestCase]
        public void AddToEmptyList()
        {
            var cashFlows = new CashFlows();

            cashFlows.Add(new Date(2000, 01, 01), 100.00m);

            Assert.Multiple(() =>
            {
                Assert.That(cashFlows, Has.Count.EqualTo(1));
                Assert.That(cashFlows[new Date(2000, 01, 01)], Is.EqualTo(100.00m));
            });
        }

        [TestCase]
        public void AddNewDate()
        {
            var cashFlows = new CashFlows();

            cashFlows.Add(new Date(2000, 01, 01), 100.00m);
            cashFlows.Add(new Date(2001, 06, 30), 200.00m);

            Assert.Multiple(() =>
            {
                Assert.That(cashFlows, Has.Count.EqualTo(2));
                Assert.That(cashFlows[new Date(2000, 01, 01)], Is.EqualTo(100.00m));
                Assert.That(cashFlows[new Date(2001, 06, 30)], Is.EqualTo(200.00m));
            });
        }

        [TestCase]
        public void AddMatchingExistingDate()
        {
            var cashFlows = new CashFlows();

            cashFlows.Add(new Date(2000, 01, 01), 100.00m);
            cashFlows.Add(new Date(2000, 01, 01), 200.00m);

            Assert.Multiple(() =>
            {
                Assert.That(cashFlows, Has.Count.EqualTo(1));
                Assert.That(cashFlows[new Date(2000, 01, 01)], Is.EqualTo(300.00m));
            });
        }

        [TestCase]
        public void GetCashFlowsFinalDateBeforeStartDate()
        {
            var cashFlows = new CashFlows();

            Assert.That(() => cashFlows.GetCashFlows(new Date(2001, 01, 01), 10.00m, new Date(2000, 01, 01), 20.00m, out var values, out var periods), Throws.ArgumentException);
        }

        [TestCase]
        public void GetCashFlowsStartAndFinalDateSame()
        {
            var cashFlows = new CashFlows();

            Assert.That(() => cashFlows.GetCashFlows(new Date(2000, 01, 01), 10.00m, new Date(2000, 01, 01), 20.00m, out var values, out var periods), Throws.ArgumentException);
        }

        [TestCase]
        public void GetCashFlowsNegativeInitialInvestment()
        {
            var cashFlows = new CashFlows();

            Assert.That(() => cashFlows.GetCashFlows(new Date(2000, 01, 01), -10.00m, new Date(2001, 01, 01), 20.00m, out var values, out var periods), Throws.ArgumentException);
        }

        [TestCase]
        public void GetCashFlowsEmptyList()
        {
            var cashFlows = new CashFlows();

            cashFlows.GetCashFlows(new Date(2000, 01, 01), 10.00m, new Date(2001, 01, 01), 20.00m, out var values, out var periods);

            Assert.Multiple(() =>
            {
                Assert.That(values, Is.EqualTo(new double[] { -10d, 20d }));
                Assert.That(periods, Is.EqualTo(new double[] { 0, 366 / 365d }));
            });
        }

        [TestCase]
        public void GetCashFlowsOnlyZeroValues()
        {
            var cashFlows = new CashFlows();

            cashFlows.Add(new Date(2000, 02, 01), 0.00m);
            cashFlows.Add(new Date(2000, 04, 01), 0.00m);
            cashFlows.Add(new Date(2000, 03, 01), 0.00m);
            cashFlows.Add(new Date(2000, 04, 01), 0.00m);
            cashFlows.Add(new Date(2000, 05, 01), 0.00m);

            cashFlows.GetCashFlows(new Date(2000, 01, 01), 10.00m, new Date(2001, 01, 01), 20.00m, out var values, out var periods);

            Assert.Multiple(() =>
            {
                Assert.That(values, Is.EqualTo(new double[] { -10d, 20d }));
                Assert.That(periods, Is.EqualTo(new double[] { 0, 366 / 365d }));
            });
        }

        [TestCase]
        public void GetCashFlowsAllEntries()
        {
            var cashFlows = new CashFlows();

            cashFlows.Add(new Date(2000, 02, 01), 100.00m);
            cashFlows.Add(new Date(2000, 04, 01), 200.00m);
            cashFlows.Add(new Date(2000, 03, 01), 300.00m);
            cashFlows.Add(new Date(2000, 04, 01), 400.00m);
            cashFlows.Add(new Date(2000, 05, 01), 500.00m);

            cashFlows.GetCashFlows(new Date(2000, 01, 01), 10.00m, new Date(2001, 01, 01), 600.00m, out var values, out var periods);

            Assert.Multiple(() =>
            {
                Assert.That(values, Is.EqualTo(new double[] { -10d, 100.00d, 300.00d, 600.00d, 500.00d, 600d}));
                Assert.That(periods, Is.EqualTo(new double[] { 0, 31 / 365d, (31 + 29) / 365d, (31 + 29 + 31) / 365d, (31 + 29 + 31 + 30) / 365d, 366 /365d }));
            });
        }

        [TestCase]
        public void GetCashFlowsEntryMatchingStartDate()
        {
            var cashFlows = new CashFlows();

            cashFlows.Add(new Date(2000, 01, 01), 100.00m);
            cashFlows.Add(new Date(2000, 03, 01), 200.00m);
            cashFlows.Add(new Date(2000, 02, 01), 300.00m);
            cashFlows.Add(new Date(2000, 03, 01), 400.00m);
            cashFlows.Add(new Date(2000, 04, 01), 500.00m);

            cashFlows.GetCashFlows(new Date(2000, 01, 01), 10.00m, new Date(2001, 01, 01), 600.00m, out var values, out var periods);

            Assert.Multiple(() =>
            {
                Assert.That(values, Is.EqualTo(new double[] { -10d, 300.00d, 600.00d, 500.00d, 600d }));
                Assert.That(periods, Is.EqualTo(new double[] { 0, 31 / 365d, (31 + 29) / 365d, (31 + 29 + 31) / 365d, 366 / 365d }));
            });
        }

        [TestCase]
        public void GetCashFlowsEntryMatchingEndDate()
        {
            var cashFlows = new CashFlows();

            cashFlows.Add(new Date(2000, 02, 01), 100.00m);
            cashFlows.Add(new Date(2000, 04, 01), 200.00m);
            cashFlows.Add(new Date(2000, 03, 01), 300.00m);
            cashFlows.Add(new Date(2000, 04, 01), 400.00m);
            cashFlows.Add(new Date(2000, 05, 01), 500.00m);

            cashFlows.GetCashFlows(new Date(2000, 01, 01), 10.00m, new Date(2000, 05, 01), 600.00m, out var values, out var periods);

            Assert.Multiple(() =>
            {
                Assert.That(values, Is.EqualTo(new double[] { -10d, 100.00d, 300.00d, 600.00d, 1100d }));
                Assert.That(periods, Is.EqualTo(new double[] { 0, 31 / 365d, (31 + 29) / 365d, (31 + 29 + 31) / 365d, (31 + 29 + 31 + 30) / 365d }));
            });
        }

        [TestCase]
        public void GetCashFlowsPartialList()
        {
            var cashFlows = new CashFlows();

            cashFlows.Add(new Date(2000, 01, 01), 100.00m);
            cashFlows.Add(new Date(2000, 03, 01), 200.00m);
            cashFlows.Add(new Date(2000, 02, 01), 300.00m);
            cashFlows.Add(new Date(2000, 03, 01), 400.00m);
            cashFlows.Add(new Date(2000, 04, 01), 500.00m);

            cashFlows.GetCashFlows(new Date(2000, 01, 15), 10.00m, new Date(2000, 03, 15), 600.00m, out var values, out var periods);

            Assert.Multiple(() =>
            {
                Assert.That(values, Is.EqualTo(new double[] { -10d, 300.00d, 600.00d, 600.00d }));
                Assert.That(periods, Is.EqualTo(new double[] { 0, 17 / 365d, (17 + 29) / 365d, (17 + 29 + 14) / 365d }));
            });
        }

        
        [TestCase]
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

            Assert.Multiple(() =>
            {
                Assert.That(values, Is.EqualTo(new double[] { -10d, 600.00d, 600.00d }));
                Assert.That(periods, Is.EqualTo(new double[] { 0, (17 + 29) / 365d, (17 + 29 + 14) / 365d }));
            });
        }
    }
}
