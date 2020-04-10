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

        [TestCaseSource(nameof(CgtMethodCalculationData))]
        public CgtMethod CgtMethodCalculation(Date aquisitionDate)
        {
            var saleDate = new Date(2020, 01, 01);
             
            var cgtMethod = CgtCalculator.CgtMethodForParcel(aquisitionDate, saleDate);
            return cgtMethod;
        }

        static IEnumerable<TestCaseData> CgtMethodCalculationData()
        {
            yield return new TestCaseData(new Date(1997, 01, 01)).Returns(CgtMethod.Indexation).SetName("CgtMethodCalculation(Before Indexation Date)");
            yield return new TestCaseData(new Date(2018, 01, 01)).Returns(CgtMethod.Discount).SetName("CgtMethodCalculation(Held More Than 12 Months)");
            yield return new TestCaseData(new Date(2019, 01, 01)).Returns(CgtMethod.Other).SetName("CgtMethodCalculation(Held 12 Months)");
            yield return new TestCaseData(new Date(2020, 06, 01)).Returns(CgtMethod.Other).SetName("CgtMethodCalculation(Held Less Than 12 Months)");
            yield return new TestCaseData(new Date(2020, 01, 01)).Returns(CgtMethod.Other).SetName("CgtMethodCalculation(Sold On Same Day)");
        }

        [TestCase]
        public void DiscountedCgtCalculationPositiveAmount()
        {
            var discountedGain = CgtCalculator.DiscountedCgt(1000.00m, CgtMethod.Discount);

            Assert.That(discountedGain, Is.EqualTo(500.00m));
        }

        [TestCase]
        public void DiscountedCgtCalculationNegativeAmount()
        {
            var discountedGain = CgtCalculator.DiscountedCgt(-1000.00m, CgtMethod.Discount);

            Assert.That(discountedGain, Is.EqualTo(-1000.00m));
        }

        [TestCase]
        public void DiscountedCgtCalculationZeroAmount()
        {
            var discountedGain = CgtCalculator.DiscountedCgt(0.00m, CgtMethod.Discount);

            Assert.That(discountedGain, Is.EqualTo(0.00m));
        }

        public static CgtCalculationMethod[] CgtMethods = (CgtCalculationMethod[])Enum.GetValues(typeof(CgtCalculationMethod));
        [TestCaseSource(nameof(CgtMethods))]
        public void CheckAllCgtMethodsHaveAComparere(CgtCalculationMethod method)
        {
            var comparer = CgtCalculator.GetCgtComparer(Date.MinValue, method);

            Assert.That(comparer, Is.InstanceOf<IComparer<Parcel>>());
        }

        [TestCase]
        public void CalculateCgtNoParcels()
        {
            var parcels = new List<Parcel>();
    
            var calculator = new CgtCalculator();

            var parcelsSold = calculator.Calculate(parcels, new Date(2010, 01, 01), 250, 1000.00m, new FirstInFirstOutCgtComparer());

            Assert.That(() => parcelsSold.ToList(), Throws.ArgumentException);
        }

        [TestCase]
        public void CalculateCgtNotEnoughParcels()
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

            var calculator = new CgtCalculator();

            var parcelsSold = calculator.Calculate(parcels, new Date(2010, 01, 01), 500, 1000.00m, new FirstInFirstOutCgtComparer());

            Assert.That(() => parcelsSold.ToList(), Throws.ArgumentException);
        }

        [TestCase]
        public void CalculateCgtExactParcelCount()
        {
            var parcels = new List<Parcel>();

            var parcel1 = new Parcel(Guid.NewGuid(), new Date(2000, 01, 01), new Date(2000, 01, 01), new ParcelProperties(100, 100.00m, 110.00m), null);
            parcels.Add(parcel1);

            var parcel2 = new Parcel(Guid.NewGuid(), new Date(2009, 06, 30), new Date(2009, 01, 01), new ParcelProperties(100, 120.00m, 130.00m), null);
            parcels.Add(parcel2);

            var parcel3 = new Parcel(Guid.NewGuid(), new Date(2001, 01, 01), new Date(2001, 01, 01), new ParcelProperties(100, 130.00m, 140.00m), null);
            parcel3.Change(new Date(2005, 01, 01), -100, -100.00m, -100.00m, null);
            parcels.Add(parcel3);

            var parcel4 = new Parcel(Guid.NewGuid(), new Date(2003, 01, 01), new Date(2003, 01, 01), new ParcelProperties(100, 140.00m, 150.00m), null);
            parcels.Add(parcel4);

            var calculator = new CgtCalculator();

            var parcelsSold = calculator.Calculate(parcels, new Date(2010, 01, 01), 300, 1000.00m, new FirstInFirstOutCgtComparer()).ToArray();

            var expectedResult = new ParcelSold[]
            {
                new ParcelSold(parcel1, 100, 110.00m, 333.33m, 223.33m, CgtMethod.Discount, 111.67m),
                new ParcelSold(parcel4, 100, 150.00m, 333.34m, 183.34m, CgtMethod.Discount,  91.67m),
                new ParcelSold(parcel2, 100, 130.00m, 333.33m, 203.33m, CgtMethod.Other   , 203.33m),
            };

            Assert.That(parcelsSold, Is.EqualTo(expectedResult));
        }

        [TestCase]
        public void CalculateCgtPartialSale()
        {
            var parcels = new List<Parcel>();

            var parcel1 = new Parcel(Guid.NewGuid(), new Date(2000, 01, 01), new Date(2000, 01, 01), new ParcelProperties(100, 100.00m, 110.00m), null);
            parcels.Add(parcel1);

            var parcel2 = new Parcel(Guid.NewGuid(), new Date(2009, 06, 30), new Date(2009, 01, 01), new ParcelProperties(100, 120.00m, 130.00m), null);
            parcels.Add(parcel2);

            var parcel3 = new Parcel(Guid.NewGuid(), new Date(2001, 01, 01), new Date(2001, 01, 01), new ParcelProperties(100, 130.00m, 140.00m), null);
            parcel3.Change(new Date(2005, 01, 01), -100, -100.00m, -100.00m, null);
            parcels.Add(parcel3);

            var parcel4 = new Parcel(Guid.NewGuid(), new Date(2003, 01, 01), new Date(2003, 01, 01), new ParcelProperties(100, 140.00m, 150.00m), null);
            parcels.Add(parcel4);

            var calculator = new CgtCalculator();

            var parcelsSold = calculator.Calculate(parcels, new Date(2010, 01, 01), 250, 1000.00m, new FirstInFirstOutCgtComparer()).ToArray();

            var expectedResult = new ParcelSold[]
            {
                new ParcelSold(parcel1, 100, 110.00m, 400.00m, 290.00m, CgtMethod.Discount, 145.00m),
                new ParcelSold(parcel4, 100, 150.00m, 400.00m, 250.00m, CgtMethod.Discount, 125.00m),
                new ParcelSold(parcel2,  50,  65.00m, 200.00m, 135.00m, CgtMethod.Other,    135.00m),
            };

            Assert.That(parcelsSold, Is.EqualTo(expectedResult));
        }

        [TestCaseSource(nameof(CalculateParcelCgtData))]
        public void CalculateParcelCgt(Date disposalDate, int unitsSold, decimal saleAmount, decimal expectedCostBase, decimal expectedCapitalGain, decimal expectedDiscountedGain, CgtMethod expectedCgtMethod)
        {
            var aquisitionDate = new Date(2017, 01, 01);
            var properties = new ParcelProperties(1000, 1000.00m, 2000.00m);
            var parcel = new Parcel(Guid.NewGuid(), aquisitionDate, aquisitionDate, properties, null);

            parcel.Change(new Date(2017, 08, 01), 1000, 2000.00m, 2000.00m, null);

            var calculator = new CgtCalculator();

            var parcelSold = calculator.CalculateParcelCgt(parcel, disposalDate, unitsSold, saleAmount);

            Assert.Multiple(() =>
            {
                Assert.That(parcelSold.AmountReceived, Is.EqualTo(saleAmount), "Amount Received");
                Assert.That(parcelSold.CostBase, Is.EqualTo(expectedCostBase), "Costbase");
                Assert.That(parcelSold.CapitalGain, Is.EqualTo(expectedCapitalGain), "Capital Gain");
                Assert.That(parcelSold.DiscountedGain, Is.EqualTo(expectedDiscountedGain), "Discounted Gain");
            });
        }

        static IEnumerable<TestCaseData> CalculateParcelCgtData()
        {
            yield return new TestCaseData(new Date(2017, 06, 30), 1000, 4000.00m, 2000.00m, 2000.00m, 2000.00m, CgtMethod.Other).SetName("CalculateParcelCgt(All units sold from first period)");
            yield return new TestCaseData(new Date(2017, 06, 30), 500, 4000.00m, 1000.00m, 3000.00m, 3000.00m, CgtMethod.Other).SetName("CalculateParcelCgt(Half units sold from first period)");
            yield return new TestCaseData(new Date(2017, 09, 30), 2000, 4000.00m, 4000.00m, 0.00m, 0.00m, CgtMethod.Other).SetName("CalculateParcelCgt(All units sold from current period)");
            yield return new TestCaseData(new Date(2017, 09, 30), 1000, 4000.00m, 2000.00m, 2000.00m, 2000.00m, CgtMethod.Other).SetName("CalculateParcelCgt(Half units sold from current period)");
            yield return new TestCaseData(new Date(2019, 09, 30), 500, 5000.00m, 1000.00m, 4000.00m, 2000.00m, CgtMethod.Discount).SetName("CalculateParcelCgt(More than 12 months)");
        }
    }
}
