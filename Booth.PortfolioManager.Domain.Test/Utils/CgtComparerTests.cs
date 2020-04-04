using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.Test.Utils
{
    class CgtComparerTests
    {

        [TestCaseSource(nameof(CalculateCapitalGainData), new object[] { CGTCalculationMethod.FirstInFirstOut })]
        public int FirstInFirstOut(Date disposalDate,  Parcel parcel1, Parcel parcel2)
        {
            var comparer = new CgtComparerOld(disposalDate, CGTCalculationMethod.FirstInFirstOut);

            return comparer.Compare(parcel1, parcel2);    
        }

        [TestCaseSource(nameof(CalculateCapitalGainData), new object[] { CGTCalculationMethod.LastInFirstOut })]
        public int LastInFirstOut(Date disposalDate, Parcel parcel1, Parcel parcel2)
        {
            var comparer = new CgtComparerOld(disposalDate, CGTCalculationMethod.LastInFirstOut);

            return comparer.Compare(parcel1, parcel2);
        }

        [TestCaseSource(nameof(CalculateCapitalGainData), new object[] { CGTCalculationMethod.MaximizeGain })]
        public int MaximizeGain(Date disposalDate, Parcel parcel1, Parcel parcel2)
        {
            var comparer = new CgtComparerOld(disposalDate, CGTCalculationMethod.MaximizeGain);

            return comparer.Compare(parcel1, parcel2);
        }

        [TestCaseSource(nameof(CalculateCapitalGainData), new object[] { CGTCalculationMethod.MinimizeGain })]
        public int MinimizeGain(Date disposalDate, Parcel parcel1, Parcel parcel2)
        {
            var comparer = new CgtComparerOld(disposalDate, CGTCalculationMethod.MinimizeGain);

            return comparer.Compare(parcel1, parcel2);
        }

        static IEnumerable<TestCaseData> CalculateCapitalGainData(CGTCalculationMethod method)
        {
            var p1 = new Parcel(Guid.NewGuid(), new Date(2010, 01, 01), new Date(2019, 01, 01), new ParcelProperties(1000, 1000.00m, 1000.00m), null);
            var p2 = new Parcel(Guid.NewGuid(), new Date(2010, 01, 01), new Date(2019, 01, 01), new ParcelProperties(1000, 2000.00m, 2000.00m), null);
            var p3 = new Parcel(Guid.NewGuid(), new Date(2010, 01, 01), new Date(2019, 06, 30), new ParcelProperties(1000, 1000.00m, 1000.00m), null);
            var p4 = new Parcel(Guid.NewGuid(), new Date(2010, 01, 01), new Date(2019, 06, 30), new ParcelProperties(1000, 2000.00m, 2000.00m), null);

            var scenarios = new CapitalGainScenario[]
            {
                new CapitalGainScenario(new Date(2019, 08, 01), p1, p3, "Parcel 1, no discount before Parcel 2, no discount", -1, 1, -1, -1),
                new CapitalGainScenario(new Date(2020, 02, 01), p1, p3, "Parcel 1, discount before Parcel 2, no discount", -1, 1, -1, -1),
                new CapitalGainScenario(new Date(2020, 08, 01), p1, p3, "Parcel 1, discount before Parcel 2, discount", -1, 1, -1, -1),
                new CapitalGainScenario(new Date(2019, 08, 01), p3, p1, "Parcel 1, no discount after Parcel 2, no discount", 1, -1, 1, 1),
                new CapitalGainScenario(new Date(2020, 02, 01), p3, p1, "Parcel 1, no discount after Parcel 2, discount", 1, -1, 1, 1),
                new CapitalGainScenario(new Date(2019, 08, 01), p3, p1, "Parcel 1, discount after Parcel 2, discount", 1, -1, 1, 1),
                new CapitalGainScenario(new Date(2019, 08, 01), p1, p2, "Parcel 1 same date as Parcel 2", 0, 0, -1, 1),
                new CapitalGainScenario(new Date(2019, 08, 01), p4, p1, "Parcel 1, no discount > Parcel 2, no discount", 1, -1, 1, -1),
                new CapitalGainScenario(new Date(2020, 02, 01), p4, p1, "Parcel 1, no discount > Parcel 2, discount", 1, -1, 1, 1),
                new CapitalGainScenario(new Date(2020, 02, 01), p2, p3, "Parcel 1, discount > Parcel 2, no discount", -1, 1, -1, -1),
                new CapitalGainScenario(new Date(2020, 08, 01), p2, p3, "Parcel 1, discount > Parcel 2, discount", -1, 1, 1, -1),
                new CapitalGainScenario(new Date(2019, 08, 01), p2, p3, "Parcel 1, no discount < Parcel 2, no discount", -1, 1, 1, -1),
                new CapitalGainScenario(new Date(2020, 02, 01), p3, p2, "Parcel 1, no discount < Parcel 2, discount", 1, -1, 1, 1),
                new CapitalGainScenario(new Date(2020, 02, 01), p1, p4, "Parcel 1, discount < Parcel 2, no discount", -1, 1, -1, -1),
                new CapitalGainScenario(new Date(2020, 08, 01), p1, p4, "Parcel 1, discount < Parcel 2, discount", -1, 1, -1, 1)
             };

            return scenarios.Select(x => x.TestData(method));
        }

        public class CapitalGainScenario
        {
            Date DisposalDate;
            Parcel Parcel1;
            Parcel Parcel2;
            string Message;

            int FirstInFirstOutResult;
            int LastInFirstOutResult;
            int MaximizeGainResult;
            int MinimizeGainResult;

            public CapitalGainScenario(Date disposalDate, Parcel parcel1, Parcel parcel2 ,string message ,int firstInFirstOutResult, int lastInFirstOutResult ,int maximizeGainResult, int minimizeGainResult)
            {
                DisposalDate = disposalDate;
                Parcel1 = parcel1;
                Parcel2 = parcel2;
                Message = message;

                FirstInFirstOutResult = firstInFirstOutResult;
                LastInFirstOutResult = lastInFirstOutResult;
                MaximizeGainResult = maximizeGainResult;
                MinimizeGainResult = minimizeGainResult;
            }

            public TestCaseData TestData(CGTCalculationMethod method)
            {
                switch (method)
                {
                    case CGTCalculationMethod.FirstInFirstOut:
                        return new TestCaseData(DisposalDate, Parcel1, Parcel2).Returns(FirstInFirstOutResult).SetName("FirstInFirstOut(" + Message + ")");
                    case CGTCalculationMethod.LastInFirstOut:
                        return new TestCaseData(DisposalDate, Parcel1, Parcel2).Returns(LastInFirstOutResult).SetName("LastInFirstOut(" + Message + ")");
                    case CGTCalculationMethod.MaximizeGain:
                        return new TestCaseData(DisposalDate, Parcel1, Parcel2).Returns(MaximizeGainResult).SetName("MaximizeGain(" + Message + ")");
                    case CGTCalculationMethod.MinimizeGain:
                        return new TestCaseData(DisposalDate, Parcel1, Parcel2).Returns(MinimizeGainResult).SetName("MinimizeGain(" + Message + ")");
                    default:
                        return null;
                }
            }
        }
    }
}
