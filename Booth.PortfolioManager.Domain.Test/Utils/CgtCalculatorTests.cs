using System;
using System.Collections.Generic;

using NUnit.Framework;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.Test.Utils
{
    class CgtCalculatorTests
    {

        [TestCaseSource(nameof(CgtMethodCalculationData))]
        public CGTMethod CgtMethodCalculation(Date aquisitionDate)
        {
            var saleDate = new Date(2020, 01, 01);
             
            var cgtMethod = CgtCalculator.CgtMethodForParcel(aquisitionDate, saleDate);
            return cgtMethod;
        }

        static IEnumerable<TestCaseData> CgtMethodCalculationData()
        {
            yield return new TestCaseData(new Date(1997, 01, 01)).Returns(CGTMethod.Indexation).SetName("CgtMethodCalculation(Before Indexation Date)");
            yield return new TestCaseData(new Date(2018, 01, 01)).Returns(CGTMethod.Discount).SetName("CgtMethodCalculation(Held More Than 12 Months)");
            yield return new TestCaseData(new Date(2019, 01, 01)).Returns(CGTMethod.Other).SetName("CgtMethodCalculation(Held 12 Months)");
            yield return new TestCaseData(new Date(2020, 06, 01)).Returns(CGTMethod.Other).SetName("CgtMethodCalculation(Held Less Than 12 Months)");
            yield return new TestCaseData(new Date(2020, 01, 01)).Returns(CGTMethod.Other).SetName("CgtMethodCalculation(Sold On Same Day)");
        }


        [TestCaseSource(nameof(DiscountedCgtCalculationData))]
        public decimal DiscountedCgtCalculation(Date aquisitionDate, decimal amount)
        {
            var saleDate = new Date(2020, 01, 01);

            var discountedGain = CgtCalculator.DiscountedCgt(amount, aquisitionDate, saleDate);

            return discountedGain;
        }

        static IEnumerable<TestCaseData> DiscountedCgtCalculationData()
        {
            yield return new TestCaseData(new Date(2018, 01, 01), 1000.00m).Returns(500.00m).SetName("DiscountedCgtCalculation(Gain Held More Than 12 Months)");
            yield return new TestCaseData(new Date(2018, 01, 01), -1000.00m).Returns(-500.00m).SetName("DiscountedCgtCalculation(Loss Held More Than 12 Months)");
            yield return new TestCaseData(new Date(2019, 01, 01), 1000.00m).Returns(1000.00m).SetName("DiscountedCgtCalculation(Gain Held 12 Months)");
            yield return new TestCaseData(new Date(2019, 01, 01), -1000.00m).Returns(-1000.00m).SetName("DiscountedCgtCalculation(Loss Held 12 Months)");
            yield return new TestCaseData(new Date(2020, 06, 01), 1000.00m).Returns(1000.00m).SetName("DiscountedCgtCalculation(Gain Held Less Than 12 Months)");
            yield return new TestCaseData(new Date(2020, 06, 01), -1000.00m).Returns(-1000.00m).SetName("DiscountedCgtCalculation(Loss Held Less Than 12 Months)");
            yield return new TestCaseData(new Date(2020, 01, 01), 1000.00m).Returns(1000.00m).SetName("DiscountedCgtCalculation(Sold On Same Day)");
            yield return new TestCaseData(new Date(2020, 01, 01), 0.00m).Returns(0.00m).SetName("DiscountedCgtCalculation(No Gain or Loss)");
        }


        [TestCase]
        public void CalculateCapitalGainNotEnoughParcels()
        {
            Assert.Inconclusive();
        }

        [TestCase]
        public void CalculateCapitalGainExactParcelCount()
        {
            Assert.Inconclusive();
        }

        [TestCase]
        public void CalculateCapitalGainPartialSale()
        {
            Assert.Inconclusive();
        }


    }
}
