using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Booth.Common;
using Booth.PortfolioManager.Domain.Utils;
using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Test.Utils
{
    class CgtCalculationTests
    {
        [TestCase]
        public void AmountRecievedSet()
        {
            var parcelsSold = new List<ParcelSold>();

            var calculation = new CgtCalculation(new Date(2000, 01, 01), 1000.00m, parcelsSold, CGTCalculationMethod.FirstInFirstOut);

            Assert.That(calculation.AmountReceived, Is.EqualTo(1000.00m));
        }

        [TestCase]
        public void AmountDisposalDateSet()
        {
            var parcelsSold = new List<ParcelSold>();

            var calculation = new CgtCalculation(new Date(2000, 01, 01), 1000.00m, parcelsSold, CGTCalculationMethod.FirstInFirstOut);

            Assert.That(calculation.DisposalDate, Is.EqualTo(new Date(2000, 01, 01)));
        }

        [TestCase(CGTCalculationMethod.FirstInFirstOut)]
        [TestCase(CGTCalculationMethod.LastInFirstOut)]
        [TestCase(CGTCalculationMethod.MaximizeGain)]
        [TestCase(CGTCalculationMethod.MinimizeGain)]
        public void AmountCGTMethodSet(CGTCalculationMethod method)
        {
            var parcelsSold = new List<ParcelSold>();

            var calculation = new CgtCalculation(new Date(2000, 01, 01), 1000.00m, parcelsSold, method);

            Assert.That(calculation.MethodUsed, Is.EqualTo(method));
        }

        [TestCase]
        public void UnitsSoldCalculatededCorrectly()
        {
            var parcelsSold = new List<ParcelSold>();

            var parcel1 = new ParcelSold(null, 100, new Date(2000, 01, 01));
            parcelsSold.Add(parcel1);

            var parcel2 = new ParcelSold(null, 200, new Date(2000, 01, 01));
            parcelsSold.Add(parcel2);

            var parcel3 = new ParcelSold(null, 300, new Date(2000, 01, 01));
            parcelsSold.Add(parcel3);

            var calculation = new CgtCalculation(new Date(2000, 01, 01), 1000.00m, parcelsSold, CGTCalculationMethod.FirstInFirstOut);

            Assert.That(calculation.UnitsSold, Is.EqualTo(600));
        }

        [TestCase]
        public void CapitalGainCalculatededCorrectly()
        {
            var parcelsSold = new List<ParcelSold>();

            var parcel1 = new ParcelSold(null, 100, new Date(2000, 01, 01));
            parcelsSold.Add(parcel1);

            var parcel2 = new ParcelSold(null, 200, new Date(2000, 01, 01));
            parcelsSold.Add(parcel2);

            var parcel3 = new ParcelSold(null, 300, new Date(2000, 01, 01));
            parcelsSold.Add(parcel3);

            var calculation = new CgtCalculation(new Date(2000, 01, 01), 1000.00m, parcelsSold, CGTCalculationMethod.FirstInFirstOut);

            Assert.That(calculation.CapitalGain, Is.EqualTo(1000.00m));
        }


        [TestCase]
        public void AmountRecievedApportionedCorrectly()
        {
            var parcelsSold = new List<ParcelSold>();

            var parcel1 = new ParcelSold(null, 100, new Date(2000, 01, 01));
            parcelsSold.Add(parcel1);

            var parcel2 = new ParcelSold(null, 200, new Date(2000, 01, 01));
            parcelsSold.Add(parcel2);

            var parcel3 = new ParcelSold(null, 300, new Date(2000, 01, 01));
            parcelsSold.Add(parcel3);

            var calculation = new CgtCalculation(new Date(2000, 01, 01), 1000.00m, parcelsSold, CGTCalculationMethod.FirstInFirstOut);

            Assert.Multiple(() =>
            {
                Assert.That(parcel1.AmountReceived, Is.EqualTo(166.66m));
                Assert.That(parcel2.AmountReceived, Is.EqualTo(333.33m));
                Assert.That(parcel3.AmountReceived, Is.EqualTo(500.01m));
            });
        }
    }
}
