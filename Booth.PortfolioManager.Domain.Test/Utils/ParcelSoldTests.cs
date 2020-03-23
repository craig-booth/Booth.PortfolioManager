using System;
using System.Collections.Generic;

using NUnit.Framework;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.Test.Utils
{
    class ParcelSoldTests
    {

        [TestCaseSource(nameof(CalculateCapitalGainData))]
        public void CalculateCapitalGain(Date disposalDate, int unitsSold, decimal saleAmount, decimal expectedCostBase, decimal expectedCapitalGain, decimal expectedDiscountedGain, CGTMethod expectedCgtMethod)
        {
            var aquisitionDate = new Date(2017, 01, 01);
            var properties = new ParcelProperties(1000, 1000.00m, 2000.00m);
            var parcel = new Parcel(Guid.NewGuid(), aquisitionDate, aquisitionDate, properties, null);

            parcel.Change(new Date(2017, 08, 01), 1000, 2000.00m, 2000.00m, null);

            var parcelSold = new ParcelSold(parcel, unitsSold, disposalDate);

            parcelSold.CalculateCapitalGain(saleAmount);


            Assert.Multiple(() =>
            {
                Assert.That(parcelSold.AmountReceived, Is.EqualTo(saleAmount), "Amount Received");
                Assert.That(parcelSold.CostBase, Is.EqualTo(expectedCostBase), "Costbase");
                Assert.That(parcelSold.CapitalGain, Is.EqualTo(expectedCapitalGain), "Capital Gain");
                Assert.That(parcelSold.DiscountedGain, Is.EqualTo(expectedDiscountedGain), "Discounted Gain");
                Assert.That(parcelSold.CgtMethod, Is.EqualTo(expectedCgtMethod), "CGT Method");
            });        
        }

        static IEnumerable<TestCaseData> CalculateCapitalGainData()
        {
            yield return new TestCaseData(new Date(2017, 06, 30), 1000, 4000.00m, 2000.00m, 2000.00m, 2000.00m, CGTMethod.Other).SetName("CalculateCapitalGain(All units sold from first period)");
            yield return new TestCaseData(new Date(2017, 06, 30), 500, 4000.00m, 1000.00m, 3000.00m, 3000.00m, CGTMethod.Other).SetName("CalculateCapitalGain(Half units sold from first period)");
            yield return new TestCaseData(new Date(2017, 09, 30), 2000, 4000.00m, 4000.00m, 0.00m, 0.00m, CGTMethod.Other).SetName("CalculateCapitalGain(All units sold from current period)");
            yield return new TestCaseData(new Date(2017, 09, 30), 1000, 4000.00m, 2000.00m, 2000.00m, 2000.00m, CGTMethod.Other).SetName("CalculateCapitalGain(Half units sold from current period)");
            yield return new TestCaseData(new Date(2019, 09, 30), 500, 5000.00m, 1000.00m, 4000.00m, 2000.00m, CGTMethod.Discount).SetName("CalculateCapitalGain(More than 12 months)");
        }
    }
}
