using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Utils;
using FluentAssertions;

namespace Booth.PortfolioManager.Domain.Test.Utils
{
    public class CgtComparerTests
    {

#pragma warning disable xUnit1026 // FirstInFirstOut and LastInLastOut do not need disposal date

        [Theory]
        [MemberData(nameof(CalculateCapitalGainData), new object[] { CgtCalculationMethod.FirstInFirstOut })]
        public void FirstInFirstOut(Date disposalDate, ParcelPair parcels, int expected, string because)
        {
            var comparer = new FirstInFirstOutCgtComparer();

            comparer.Compare(parcels.Parcel1, parcels.Parcel2).Should().Be(expected, because);
        }

        [Theory]
        [MemberData(nameof(CalculateCapitalGainData), new object[] { CgtCalculationMethod.LastInFirstOut })]
        public void LastInFirstOut(Date disposalDate, ParcelPair parcels, int expected, string because)
        {
            var comparer = new LastInFirstOutCgtComparer();

            comparer.Compare(parcels.Parcel1, parcels.Parcel2).Should().Be(expected, because);
        }
#pragma warning restore xUnit1026 

        [Theory]
        [MemberData(nameof(CalculateCapitalGainData), new object[] { CgtCalculationMethod.MaximizeGain })]
        public void MaximizeGain(Date disposalDate, ParcelPair parcels, int expected, string because)
        {
            var comparer = new MaximizeGainCgtComparer(disposalDate);

            comparer.Compare(parcels.Parcel1, parcels.Parcel2).Should().Be(expected, because);
        }

        [Theory]
        [MemberData(nameof(CalculateCapitalGainData), new object[] { CgtCalculationMethod.MinimizeGain })]
        public void MinimizeGain(Date disposalDate, ParcelPair parcels, int expected, string because)
        {
            var comparer = new MinimizeGainCgtComparer(disposalDate);

            comparer.Compare(parcels.Parcel1, parcels.Parcel2).Should().Be(expected, because);
        }

        public static IEnumerable<object[]> CalculateCapitalGainData(CgtCalculationMethod method)
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

        public class ParcelPair
        {
            internal Parcel Parcel1;
            internal Parcel Parcel2;
            internal ParcelPair(Parcel parcel1, Parcel parecel2)
            {
                Parcel1 = parcel1;
                Parcel2 = parecel2;
            }
        }

        class CapitalGainScenario
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

            public object[] TestData(CgtCalculationMethod method)
            {
                switch (method)
                {
                    case CgtCalculationMethod.FirstInFirstOut:
                        return new object[] { DisposalDate, new ParcelPair(Parcel1, Parcel2), FirstInFirstOutResult, "FirstInFirstOut(" + Message + ")"};
                    case CgtCalculationMethod.LastInFirstOut:
                        return new object[] { DisposalDate, new ParcelPair(Parcel1, Parcel2), LastInFirstOutResult, "LastInFirstOut(" + Message + ")"};
                    case CgtCalculationMethod.MaximizeGain:
                        return new object[] { DisposalDate, new ParcelPair(Parcel1, Parcel2), MaximizeGainResult, "MaximizeGain(" + Message + ")"};
                    case CgtCalculationMethod.MinimizeGain:
                        return new object[] { DisposalDate, new ParcelPair(Parcel1, Parcel2), MinimizeGainResult, "MinimizeGain(" + Message + ")"};
                    default:
                        return null;
                }
            }
        }
    }
}
