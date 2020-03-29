using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.Test.Utils
{
    class CgtCalculatorTests
    {
        [TestCase]
        public void SellOrderEmptyParcelList()
        {
            var parcels = new List<Parcel>();

            var result = CgtCalculator.ParcelsInSellOrder(parcels, new CgtComparer(new Date(2010, 01, 01), CGTCalculationMethod.FirstInFirstOut)).Select(x => x.Id).ToArray();

            Assert.That(result, Is.Empty);
        }

        [TestCase]
        public void SellOrderOnlyIncludeParcelsPurchasedBeforeSaleDate()
        {
            var parcels = new List<Parcel>();

            var parcel1 = new Parcel(Guid.NewGuid(), new Date(2000, 01, 01), new Date(2000, 01, 01), new ParcelProperties(100, 100.00m, 100.00m), null);
            parcels.Add(parcel1);

            var parcel2 = new Parcel(Guid.NewGuid(), new Date(2002, 01, 01), new Date(2002, 01, 01), new ParcelProperties(100, 100.00m, 100.00m), null);
            parcels.Add(parcel2);

            var parcel3 = new Parcel(Guid.NewGuid(), new Date(2001, 01, 01), new Date(2001, 01, 01), new ParcelProperties(100, 100.00m, 100.00m), null);
            parcels.Add(parcel3);

            var parcel4 = new Parcel(Guid.NewGuid(), new Date(2003, 01, 01), new Date(2003, 01, 01), new ParcelProperties(100, 100.00m, 100.00m), null);
            parcels.Add(parcel4);

            var result = CgtCalculator.ParcelsInSellOrder(parcels, new CgtComparer(new Date(2002, 06, 30), CGTCalculationMethod.FirstInFirstOut)).Select(x => x.Id).ToArray();

            Assert.That(result, Is.EqualTo(new Guid[] { parcel1.Id, parcel3.Id, parcel2.Id }));
        }

        [TestCase]
        public void SellOrderOnlyIncludeParcelsNotSold()
        {
            var parcels = new List<Parcel>();

            var parcel1 = new Parcel(Guid.NewGuid(), new Date(2000, 01, 01), new Date(2000, 01, 01), new ParcelProperties(100, 100.00m, 100.00m), null);
            parcels.Add(parcel1);

            var parcel2 = new Parcel(Guid.NewGuid(), new Date(2002, 01, 01), new Date(2002, 01, 01), new ParcelProperties(100, 100.00m, 100.00m), null);
            parcels.Add(parcel2);

            var parcel3 = new Parcel(Guid.NewGuid(), new Date(2001, 01, 01), new Date(2001, 01, 01), new ParcelProperties(100, 100.00m, 100.00m), null);
            parcel3.Change(new Date(2005, 01, 01), -100, -100.00m, -100.00m, null);
            parcels.Add(parcel3);

            var parcel4 = new Parcel(Guid.NewGuid(), new Date(2003, 01, 01), new Date(2003, 01, 01), new ParcelProperties(100, 100.00m, 100.00m), null);
            parcels.Add(parcel4);

            var result = CgtCalculator.ParcelsInSellOrder(parcels, new CgtComparer(new Date(2010, 01, 01), CGTCalculationMethod.FirstInFirstOut)).Select(x => x.Id).ToArray();

            Assert.That(result, Is.EqualTo(new Guid[] { parcel1.Id, parcel2.Id, parcel4.Id }));
        }

        [TestCase]
        public void SellOrderIncludeAll()
        {
            var parcels = new List<Parcel>();

            var parcel1 = new Parcel(Guid.NewGuid(), new Date(2000, 01, 01), new Date(2000, 01, 01), new ParcelProperties(100, 100.00m, 100.00m), null);
            parcels.Add(parcel1);

            var parcel2 = new Parcel(Guid.NewGuid(), new Date(2002, 01, 01), new Date(2002, 01, 01), new ParcelProperties(100, 100.00m, 100.00m), null);
            parcels.Add(parcel2);

            var parcel3 = new Parcel(Guid.NewGuid(), new Date(2001, 01, 01), new Date(2001, 01, 01), new ParcelProperties(100, 100.00m, 100.00m), null);
            parcel3.Change(new Date(2015, 01, 01), -50, -100.00m, -100.00m, null);
            parcels.Add(parcel3);

            var parcel4 = new Parcel(Guid.NewGuid(), new Date(2003, 01, 01), new Date(2003, 01, 01), new ParcelProperties(100, 100.00m, 100.00m), null);
            parcels.Add(parcel4);

            var result = CgtCalculator.ParcelsInSellOrder(parcels, new CgtComparer(new Date(2010, 01, 01), CGTCalculationMethod.FirstInFirstOut)).Select(x => x.Id).ToArray();

            Assert.That(result, Is.EqualTo(new Guid[] { parcel1.Id, parcel3.Id, parcel2.Id, parcel4.Id }));
        }

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
