using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.Test.Utils
{
    public class TransactionListTests
    {

        [Fact]
        public void AccessByIndex()
        {
            var list = new TransactionListTestClass();

            var id = Guid.NewGuid();
            list.Add(new TransactionTestClass(id, new Date(2000, 01, 01), "Entry 1"));
            var id2 = Guid.NewGuid();
            list.Add(new TransactionTestClass(id2, new Date(2001, 01, 01), "Entry 2"));

            var entry = list[1];

            entry.Description.Should().Be("Entry 2");
        }

        [Fact]
        public void AccessByIndexNoEntry()
        {
            var list = new TransactionListTestClass();

            var id = Guid.NewGuid();
            list.Add(new TransactionTestClass(id, new Date(2000, 01, 01), "Entry 1"));
            var id2 = Guid.NewGuid();
            list.Add(new TransactionTestClass(id2, new Date(2001, 01, 01), "Entry 2"));

            Action a = () => { var x = list[2]; };
            
            a.Should().Throw<ArgumentOutOfRangeException>();
        }


        [Fact]
        public void AccessById()
        {
            var list = new TransactionListTestClass();

            var id = Guid.NewGuid();
            list.Add(new TransactionTestClass(id, new Date(2000, 01, 01), "Entry 1"));
            var id2 = Guid.NewGuid();
            list.Add(new TransactionTestClass(id2, new Date(2001, 01, 01), "Entry 2"));

            var entry = list[id];

            entry.Description.Should().Be("Entry 1");
        }

        [Fact]
        public void AccessByIdNoEntry()
        {
            var list = new TransactionListTestClass();

            var id = Guid.NewGuid();
            list.Add(new TransactionTestClass(id, new Date(2000, 01, 01), "Entry 1"));
            var id2 = Guid.NewGuid();
            list.Add(new TransactionTestClass(id2, new Date(2001, 01, 01), "Entry 2"));

            Action a = () => { var x = list[Guid.NewGuid()]; };
            
            a.Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void ContainsIdNotExists()
        {
            var list = new TransactionListTestClass();

            var id = Guid.NewGuid();
            list.Add(new TransactionTestClass(id, new Date(2000, 01, 01), "Entry 1"));
            var id2 = Guid.NewGuid();
            list.Add(new TransactionTestClass(id2, new Date(2001, 01, 01), "Entry 2"));

            list.Contains(Guid.NewGuid()).Should().BeFalse();
        }

        [Fact]
        public void ContainsIdExists()
        {
            var list = new TransactionListTestClass();

            var id = Guid.NewGuid();
            list.Add(new TransactionTestClass(id, new Date(2000, 01, 01), "Entry 1"));
            var id2 = Guid.NewGuid();
            list.Add(new TransactionTestClass(id2, new Date(2001, 01, 01), "Entry 2"));

            list.Contains(id2).Should().BeTrue();
        }

        [Fact]
        public void TryGetValueIdNotExists()
        {
            var list = new TransactionListTestClass();

            var id = Guid.NewGuid();
            list.Add(new TransactionTestClass(id, new Date(2000, 01, 01), "Entry 1"));
            var id2 = Guid.NewGuid();
            list.Add(new TransactionTestClass(id2, new Date(2001, 01, 01), "Entry 2"));

            list.TryGetValue(Guid.NewGuid(), out var transaction).Should().BeFalse();
        }

        [Fact]
        public void TryGetValueIdExists()
        {
            var list = new TransactionListTestClass();

            var id = Guid.NewGuid();
            list.Add(new TransactionTestClass(id, new Date(2000, 01, 01), "Entry 1"));
            var id2 = Guid.NewGuid();
            list.Add(new TransactionTestClass(id2, new Date(2001, 01, 01), "Entry 2"));

            list.TryGetValue(id, out var transaction).Should().BeTrue();
            transaction.Description.Should().Be("Entry 1");
        }

        [Fact]
        public void Count()
        {
            var list = new TransactionListTestClass();

            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 3"));

            list.Count.Should().Be(3);
        }


        [Fact]
        public void CountNoEnties()
        {
            var list = new TransactionListTestClass();

            list.Count.Should().Be(0);
        }

        [Fact]
        public void Earliest()
        {
            var list = new TransactionListTestClass();

            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 3"));

            list.Earliest.Should().Be(new Date(2000, 01, 01));
        }

        [Fact]
        public void EarliestNoEnties()
        {
            var list = new TransactionListTestClass();

            list.Earliest.Should().Be(Date.MinValue);
        }

        [Fact]
        public void Latest()
        {
            var list = new TransactionListTestClass();

            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 3"));

            list.Latest.Should().Be(new Date(2002, 01, 01));
        }

        [Fact]
        public void LatestNoEnties()
        {
            var list = new TransactionListTestClass();

            list.Earliest.Should().Be(Date.MinValue);
        }

        [Fact]
        public void AddToEmptyList()
        {
            var list = new TransactionListTestClass();

            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));

            list.Count.Should().Be(1);
        }

        [Fact]
        public void AddWithSameIdAsExistingEntry()
        {
            var list = new TransactionListTestClass();

            var id = Guid.NewGuid();
            list.Add(new TransactionTestClass(id, new Date(2000, 01, 01), "Entry 1"));

            Action a = () => list.Add(new TransactionTestClass(id, new Date(2001, 01, 01), "Entry 2"));

            a.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void AddBeforeFirstEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 3"));

            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(1999, 01, 01), "Entry 4"));

            list[0].Description.Should().Be("Entry 4");
        }

        [Fact]
        public void AddSameDateAsFirstEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 3"));

            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 4"));

            list[1].Description.Should().Be("Entry 4");
        }

        [Fact]
        public void AddInMiddle()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 3"));

            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 06, 01), "Entry 4"));

            list[2].Description.Should().Be("Entry 4");
        }

        [Fact]
        public void AddInMiddleOnTheSameDayAsAnExitingEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 3"));

            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 4"));

            list[2].Description.Should().Be("Entry 4");
        }


        [Fact]
        public void AddSameDateAsLastEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 3"));

            var id = Guid.NewGuid();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 4"));

            list[3].Description.Should().Be("Entry 4");
        }

        [Fact]
        public void AddAfterLastEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 3"));

            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2003, 01, 01), "Entry 4"));

            list[3].Description.Should().Be("Entry 4");
        }

        [Fact]
        public void Clear()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 3"));

            list.Clear();

            list.Should().BeEmpty();
        }

        [Fact]
        public void ClearNoEntries()
        {
            var list = new TransactionListTestClass();

            list.Clear();

            list.Should().BeEmpty();
        }

        [Fact]
        public void IndexOfEmptyList()
        {
            var list = new TransactionListTestClass();

            var result = list.IndexOf(new Date(2000, 01, 01), TransationListPosition.First);

            result.Should().Be(~0);
        }

        [Fact]
        public void IndexOfBeforeFirstEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 3"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 4"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2003, 01, 01), "Entry 5"));

            var result = list.IndexOf(new Date(1999, 01, 01), TransationListPosition.First);

            result.Should().Be(~0);
        }

        [Fact]
        public void IndexOfFirstEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 3"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 4"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2003, 01, 01), "Entry 5"));

            var result = list.IndexOf(new Date(2000, 01, 01), TransationListPosition.First);

            result.Should().Be(0);
        }

        [Fact]
        public void IndexOfBetweenTwoDates()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 3"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 4"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2003, 01, 01), "Entry 5"));

            var result = list.IndexOf(new Date(2001, 06, 01), TransationListPosition.First);

            result.Should().Be(~3);
        }

        [Fact]
        public void IndexOfMatchingMultipeDatesToGetFirstEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 3"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 4"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 5"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2003, 01, 01), "Entry 6"));

            var result = list.IndexOf(new Date(2001, 01, 01), TransationListPosition.First);

            result.Should().Be(1);
        }

        [Fact]
        public void IndexOfMatchingMultipeDatesToGetLastEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 3"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 4"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 5"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2003, 01, 01), "Entry 6"));

            var result = list.IndexOf(new Date(2001, 01, 01), TransationListPosition.Last);

            result.Should().Be(3);
        }

        [Fact]
        public void IndexOfSameDateAsLastEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 3"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 4"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 5"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2003, 01, 01), "Entry 6"));

            var result = list.IndexOf(new Date(2003, 01, 01), TransationListPosition.Last);

            result.Should().Be(5);
        }

        [Fact]
        public void IndexOfAfterLastEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 3"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 4"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 5"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2003, 01, 01), "Entry 6"));

            var result = list.IndexOf(new Date(2004, 01, 01), TransationListPosition.Last);

            result.Should().Be(~6);
        }

        [Fact]
        public void EnumerateEmptyList()
        {
            var list = new TransactionListTestClass();

            var result = list.ToList();

            result.Should().BeEmpty();
        }

        [Fact]
        public void EnumerateList()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 3"));

            var result = list.Select(x => x.Description).ToArray();

            result.Should().Equal(new string[] { "Entry 1", "Entry 2", "Entry 3" } );
        }

        [Fact]
        public void EnumerateFromDateOfEmptyList()
        {
            var list = new TransactionListTestClass();

            var result = list.FromDate(new Date(2000, 01, 01)).Select(x => x.Date).ToArray();

            result.Should().BeEmpty();
        }

        [Fact]
        public void EnumerateFromDateMatchingMultipleEntries()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 3"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 4"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 5"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2003, 01, 01), "Entry 6"));

            var result = list.FromDate(new Date(2001, 01, 01)).Select(x => x.Description).ToArray();

            result.Should().Equal(new string[] { "Entry 2", "Entry 3", "Entry 4", "Entry 5", "Entry 6" });
        }

        [Fact]
        public void EnumerateFromDateBetweenEntries()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 3"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 4"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 5"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2003, 01, 01), "Entry 6"));

            var result = list.FromDate(new Date(2001, 06, 01)).Select(x => x.Description).ToArray();

            result.Should().Equal(new string[] { "Entry 5", "Entry 6" });
        }

        [Fact]
        public void EnumerateToDateOfEmptyList()
        {
            var list = new TransactionListTestClass();

            var result = list.ToDate(new Date(2000, 01, 01)).Select(x => x.Description).ToArray();

            result.Should().BeEmpty();
        }

        [Fact]
        public void EnumerateToDateMatchingMultipleEntries()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 3"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 4"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 5"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2003, 01, 01), "Entry 6"));

            var result = list.ToDate(new Date(2001, 01, 01)).Select(x => x.Description).ToArray();

            result.Should().Equal(new string[] { "Entry 1", "Entry 2", "Entry 3", "Entry 4" });

        }

        [Fact]
        public void EnumerateToDateBetweenEntries()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 3"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 4"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 5"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2003, 01, 01), "Entry 6"));

            var result = list.ToDate(new Date(2001, 06, 01)).Select(x => x.Description).ToArray();

            result.Should().Equal(new string[] { "Entry 1", "Entry 2", "Entry 3", "Entry 4" });
        }

        [Fact]
        public void EnumerateRangeOfEmptyList()
        {
            var list = new TransactionListTestClass();

            var result = list.InDateRange(new DateRange(new Date(2000, 01, 01), new Date(2003, 01, 01))).Select(x => x.Date).ToList();

            result.Should().BeEmpty();
        }

        [Fact]
        public void EnumerateRangeMatchingMultipleEntries()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 3"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 4"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 5"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 6"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2003, 01, 01), "Entry 7"));

            var result = list.InDateRange(new DateRange(new Date(2001, 01, 01), new Date(2002, 01, 01))).Select(x => x.Description).ToArray();

            result.Should().Equal(new string[] { "Entry 2", "Entry 3", "Entry 4", "Entry 5", "Entry 6" });
        }

        [Fact]
        public void EnumerateRangeBetweenEntries()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 3"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 4"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 5"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 6"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2003, 01, 01), "Entry 7"));

            var result = list.InDateRange(new DateRange(new Date(2001, 06, 01), new Date(2002, 06, 01))).Select(x => x.Description).ToArray();


            result.Should().Equal(new string[] { "Entry 5", "Entry 6" });

        }

        [Fact]
        public void UpdateEmptyList()
        {
            var list = new TransactionListTestClass();

            Action a = () => list.Update(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Updated Entry"));

            a.Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void UpdateEntryNotExists()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 3"));

            Action a = () => list.Update(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Updated Entry"));

            a.Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void UpdateSameIdAndDate()
        {
            var id = Guid.NewGuid();
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(id, new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 3"));

            list.Update(new TransactionTestClass(id, new Date(2001, 01, 01), "Updated Entry"));

            var result = list.Select(x => x.Description).ToArray();
            result.Should().Equal(new string[] { "Entry 1", "Updated Entry", "Entry 3" });
        }

        [Fact]
        public void UpdateChangeDate()
        {
            var id = Guid.NewGuid();
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(id, new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 3"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 4"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2003, 01, 01), "Entry 5"));

            list.Update(new TransactionTestClass(id, new Date(2002, 01, 01), "Updated Entry"));

            var result = list.Select(x => x.Description).ToArray();

            result.Should().Equal(new string[] { "Entry 1", "Entry 3" , "Entry 4", "Updated Entry", "Entry 5" });
        }

        [Fact]
        public void RemoveAtEmptyList()
        {
            var list = new TransactionListTestClass();

            Action a = () => list.RemoveAt(1);
            
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void RemoveAtItemLessThanZero()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 3"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 4"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 5"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 6"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2003, 01, 01), "Entry 7"));

            Action a = () => list.RemoveAt(-1);

            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void RemoveAtafterLastEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 3"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 4"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 5"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 6"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2003, 01, 01), "Entry 7"));

            Action a = () => list.RemoveAt(15);

            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void RemoveAtFirstEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 3"));

            list.RemoveAt(0);
            var result = list.Select(x => x.Date).ToArray();

            result.Should().Equal(new Date[] { new Date(2001, 01, 01), new Date(2002, 01, 01) });
        }

        [Fact]
        public void RemoveAtLastEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 3"));

            list.RemoveAt(2);
            var result = list.Select(x => x.Description).ToArray();

            result.Should().Equal(new string[] { "Entry 1", "Entry 2" });
        }

        [Fact]
        public void RemoveAtMiddleEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 3"));

            list.RemoveAt(1);
            var result = list.Select(x => x.Description).ToArray();

            result.Should().Equal(new string[] { "Entry 1", "Entry 3" });
        }

        [Fact]
        public void RemoveEmptyList()
        {
            var list = new TransactionListTestClass();

            Action a = () => list.Remove(Guid.NewGuid());

            a.Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void RemoveIdNotExists()
        {
            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 3"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 4"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 5"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 6"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2003, 01, 01), "Entry 7"));

            Action a = () => list.Remove(Guid.NewGuid());

            a.Should().Throw<KeyNotFoundException>();
        }


        [Fact]
        public void RemoveFirstEntry()
        {
            var id = Guid.NewGuid();

            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(id, new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 3"));

            list.Remove(id);
            var result = list.Select(x => x.Description).ToArray();

            result.Should().Equal(new string[] { "Entry 2", "Entry 3" });
        }

        [Fact]
        public void RemoveLastEntry()
        {
            var id = Guid.NewGuid();

            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(id, new Date(2002, 01, 01), "Entry 3"));

            list.Remove(id);
            var result = list.Select(x => x.Description).ToArray();

            result.Should().Equal(new string[] { "Entry 1", "Entry 2" });
        }

        [Fact]
        public void RemoveMiddleEntry()
        {
            var id = Guid.NewGuid();

            var list = new TransactionListTestClass();
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2000, 01, 01), "Entry 1"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 2"));
            list.Add(new TransactionTestClass(id, new Date(2001, 01, 01), "Entry 3"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2001, 01, 01), "Entry 4"));
            list.Add(new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01), "Entry 5"));

            list.Remove(id);
            var result = list.Select(x => x.Description).ToArray();

            result.Should().Equal(new string[] { "Entry 1", "Entry 2", "Entry 4", "Entry 5" });
        }


    }

    class TransactionTestClass : ITransaction
    {
        public Guid Id { get; }

        public Date Date { get; }

        public string Description { get; }

        public TransactionTestClass(Guid id, Date date, string description)
        {
            Id = id;
            Date = date;
            Description = description;
        }
    }
    class TransactionListTestClass : TransactionList<TransactionTestClass>
    {
        public new void Add(TransactionTestClass transaction)
        {
            base.Add(transaction);
        }

        public new void Clear()
        {
            base.Clear();
        }

        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);
        }

        public new void Update(TransactionTestClass transaction)
        {
            base.Update(transaction);
        }

        public new void Remove(Guid id)
        {
            base.Remove(id);
        }

        public new bool Contains(Guid id)
        {
            return base.Contains(id);
        }

        public new bool TryGetValue(Guid id, out TransactionTestClass transaction)
        {
            return base.TryGetValue(id, out transaction);
        }
    }
}
