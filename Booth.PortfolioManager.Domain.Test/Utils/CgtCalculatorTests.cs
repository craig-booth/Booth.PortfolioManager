using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.Test.Utils
{
    public class CgtCalculatorTests
    {

        [Theory]
        [MemberData(nameof(CgtMethodCalculationData))]
        public void CgtMethodCalculation(Date aquisitionDate, CgtMethod method, string reason)
        {
            var saleDate = new Date(2020, 01, 01);
             
            var cgtMethod = CgtUtils.CgtMethodForParcel(aquisitionDate, saleDate);

            cgtMethod.Should().Be(method, reason);
        }

        public static IEnumerable<object[]> CgtMethodCalculationData()
        {
            yield return new object[] { new Date(1997, 01, 01), CgtMethod.Indexation, "CgtMethodCalculation(Before Indexation Date)" };
            yield return new object[] { new Date(2018, 01, 01), CgtMethod.Discount, "CgtMethodCalculation(Held More Than 12 Months)" };
            yield return new object[] { new Date(2019, 01, 01), CgtMethod.Other, "CgtMethodCalculation(Held 12 Months)" };
            yield return new object[] { new Date(2020, 06, 01), CgtMethod.Other, "CgtMethodCalculation(Held Less Than 12 Months)" };
            yield return new object[] { new Date(2020, 01, 01), CgtMethod.Other, "CgtMethodCalculation(Sold On Same Day)" };
        }

        [Fact]
        public void DiscountedCgtCalculationPositiveAmount()
        {
            var discountedGain = CgtUtils.DiscountedCgt(1000.00m, CgtMethod.Discount);

            discountedGain.Should().Be(500.00m);
        }

        [Fact]
        public void DiscountedCgtCalculationNegativeAmount()
        {
            var discountedGain = CgtUtils.DiscountedCgt(-1000.00m, CgtMethod.Discount);

            discountedGain.Should().Be(-1000.00m);
        }

        [Fact]
        public void DiscountedCgtCalculationZeroAmount()
        {
            var discountedGain = CgtUtils.DiscountedCgt(0.00m, CgtMethod.Discount);

            discountedGain.Should().Be(0.00m);
        }

        [Theory]
        [MemberData(nameof(CgtMethods))]
        public void CheckAllCgtMethodsHaveAComparere(CgtCalculationMethod method)
        {
            var comparer = CgtCalculator.GetCgtComparer(Date.MinValue, method);

            comparer.Should().BeAssignableTo<IComparer<Parcel>>();
        }

        public static IEnumerable<object[]> CgtMethods()
        {
            foreach (var calculationMethod in Enum.GetValues(typeof(CgtCalculationMethod)))
            {
                yield return new object[] { calculationMethod };
            }
        }

        [Fact]
        public void CalculateCgtNoParcels()
        {
            var parcels = new List<Parcel>();
    
            var calculator = new CgtCalculator();

            var parcelsSold = calculator.Calculate(parcels, new Date(2010, 01, 01), 250, 1000.00m, new FirstInFirstOutCgtComparer());

            Action a = () => parcelsSold.ToList();
            
            a.Should().Throw<ArgumentException>();
        }

        [Fact]
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

            Action a = () => parcelsSold.ToList();
a.Should().Throw<ArgumentException>();
        }

        [Fact]
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

            parcelsSold.Should().Equal(expectedResult);
        }

        [Fact]
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

            parcelsSold.Should().Equal(expectedResult);
        }

        [Theory]
        [MemberData(nameof(CalculateParcelCgtData))]
        public void CalculateParcelCgt(Date disposalDate, int unitsSold, decimal saleAmount, decimal expectedCostBase, decimal expectedCapitalGain, decimal expectedDiscountedGain, CgtMethod expectedCgtMethod, string because)
        {
            var aquisitionDate = new Date(2017, 01, 01);
            var properties = new ParcelProperties(1000, 1000.00m, 2000.00m);
            var parcel = new Parcel(Guid.NewGuid(), aquisitionDate, aquisitionDate, properties, null);

            parcel.Change(new Date(2017, 08, 01), 1000, 2000.00m, 2000.00m, null);

            var calculator = new CgtCalculator();

            var parcelSold = calculator.CalculateParcelCgt(parcel, disposalDate, unitsSold, saleAmount);
 
            parcelSold.Should().BeEquivalentTo(new
            {
                AmountReceived = saleAmount,
                CostBase = expectedCostBase,
                CapitalGain = expectedCapitalGain,
                DiscountedGain = expectedDiscountedGain,
                CgtMethod = expectedCgtMethod
            }, because);

        }

        public static IEnumerable<object[]> CalculateParcelCgtData()
        {
            yield return new object[] { new Date(2017, 06, 30), 1000, 4000.00m, 2000.00m, 2000.00m, 2000.00m, CgtMethod.Other, "CalculateParcelCgt(All units sold from first period)" };
            yield return new object[] { new Date(2017, 06, 30), 500, 4000.00m, 1000.00m, 3000.00m, 3000.00m, CgtMethod.Other, "CalculateParcelCgt(Half units sold from first period)" };
            yield return new object[] { new Date(2017, 09, 30), 2000, 4000.00m, 4000.00m, 0.00m, 0.00m, CgtMethod.Other, "CalculateParcelCgt(All units sold from current period)" };
            yield return new object[] { new Date(2017, 09, 30), 1000, 4000.00m, 2000.00m, 2000.00m, 2000.00m, CgtMethod.Other, "CalculateParcelCgt(Half units sold from current period)" };
            yield return new object[] { new Date(2019, 09, 30), 500, 5000.00m, 1000.00m, 4000.00m, 2000.00m, CgtMethod.Discount, "CalculateParcelCgt(More than 12 months)" };
        }
    }
}
