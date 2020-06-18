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
            list.Add(id, new Date(2000, 01, 01));
            var id2 = Guid.NewGuid();
            list.Add(id2, new Date(2001, 01, 01));

            var entry = list[1];

            entry.Date.Should().Be(new Date(2001, 01, 01));
        }

        [Fact]
        public void AccessByIndexNoEntry()
        {
            var list = new TransactionListTestClass();

            var id = Guid.NewGuid();
            list.Add(id, new Date(2000, 01, 01));
            var id2 = Guid.NewGuid();
            list.Add(id2, new Date(2001, 01, 01));

            Action a = () => { var x = list[2]; };
            
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void SetByIndex()
        {
            var list = new TransactionListTestClass();

            var id = Guid.NewGuid();
            list.Add(id, new Date(2000, 01, 01));
            var id2 = Guid.NewGuid();
            list.Add(id2, new Date(2001, 01, 01));

            var id3 = Guid.NewGuid();
            list[0] = new TransactionTestClass(id3,  new Date(2002, 01, 01));

            var entry = list[0];
            entry.Date.Should().Be(new Date(2002, 01, 01));
        }

        [Fact]
        public void SetByIndexNoEntry()
        {
            var list = new TransactionListTestClass();

            var id = Guid.NewGuid();
            list.Add(id, new Date(2000, 01, 01));
            var id2 = Guid.NewGuid();
            list.Add(id2, new Date(2001, 01, 01));

            Action a = () => list[2] = new TransactionTestClass(id, new Date(2002, 01, 01));
            
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void AccessById()
        {
            var list = new TransactionListTestClass();

            var id = Guid.NewGuid();
            list.Add(id, new Date(2000, 01, 01));
            var id2 = Guid.NewGuid();
            list.Add(id2, new Date(2001, 01, 01));

            var entry = list[id];

            entry.Date.Should().Be(new Date(2000, 01, 01));
        }

        [Fact]
        public void AccessByIdNoEntry()
        {
            var list = new TransactionListTestClass();

            var id = Guid.NewGuid();
            list.Add(id, new Date(2000, 01, 01));
            var id2 = Guid.NewGuid();
            list.Add(id2, new Date(2001, 01, 01));

            Action a = () => { var x = list[Guid.NewGuid()]; };
            
            a.Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void SetById()
        {
            var list = new TransactionListTestClass();

            var id = Guid.NewGuid();
            list.Add(id, new Date(2000, 01, 01));
            var id2 = Guid.NewGuid();
            list.Add(id2, new Date(2001, 01, 01));

            list[id] = new TransactionTestClass(id, new Date(2002, 01, 01));

            var entry = list[id];
            entry.Date.Should().Be(new Date(2002, 01, 01));
        }

        [Fact]
        public void SetByIdNoEntry()
        {
            var list = new TransactionListTestClass();

            var id = Guid.NewGuid();
            list.Add(id, new Date(2000, 01, 01));
            var id2 = Guid.NewGuid();
            list.Add(id2, new Date(2001, 01, 01));

            Action a = () => { list[Guid.NewGuid()] = new TransactionTestClass(Guid.NewGuid(), new Date(2002, 01, 01)); };
            
            a.Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void Count()
        {
            var list = new TransactionListTestClass();

            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

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

            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

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

            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

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

            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));

            list.Count.Should().Be(1);
        }

        [Fact]
        public void AddWithSameIdAsExistingEntry()
        {
            var list = new TransactionListTestClass();

            var id = Guid.NewGuid();
            list.Add(id, new Date(2000, 01, 01));

            Action a = () => list.Add(id, new Date(2001, 01, 01));
            
            a.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void AddBeforeFirstEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

            var id = Guid.NewGuid();
            list.Add(id, new Date(1999, 01, 01));

            list[0].Id.Should().Be(id);
        }

        [Fact]
        public void AddSameDateAsFirstEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

            var id = Guid.NewGuid();
            list.Add(id, new Date(2000, 01, 01));

            list[1].Id.Should().Be(id);
        }

        [Fact]
        public void AddInMiddle()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

            var id = Guid.NewGuid();
            list.Add(id, new Date(2001, 06, 01));

            list[2].Id.Should().Be(id);
        }

        [Fact]
        public void AddInMiddleOnTheSameDayAsAnExitingEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

            var id = Guid.NewGuid();
            list.Add(id, new Date(2001, 01, 01));

            list[2].Id.Should().Be(id);
        }


        [Fact]
        public void AddSameDateAsLastEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

            var id = Guid.NewGuid();
            list.Add(id, new Date(2002, 01, 01));

            list[3].Id.Should().Be(id);
        }

        [Fact]
        public void AddAfterLastEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

            var id = Guid.NewGuid();
            list.Add(id, new Date(2003, 01, 01));

            list[3].Id.Should().Be(id);
        }

        [Fact]
        public void Clear()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

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
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2003, 01, 01));

            var result = list.IndexOf(new Date(1999, 01, 01), TransationListPosition.First);

            result.Should().Be(~0);
        }

        [Fact]
        public void IndexOfFirstEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2003, 01, 01));

            var result = list.IndexOf(new Date(2000, 01, 01), TransationListPosition.First);

            result.Should().Be(0);
        }

        [Fact]
        public void IndexOfBetweenTwoDates()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2003, 01, 01));

            var result = list.IndexOf(new Date(2001, 06, 01), TransationListPosition.First);

            result.Should().Be(~3);
        }

        [Fact]
        public void IndexOfMatchingMultipeDatesToGetFirstEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2003, 01, 01));

            var result = list.IndexOf(new Date(2001, 01, 01), TransationListPosition.First);

            result.Should().Be(1);
        }

        [Fact]
        public void IndexOfMatchingMultipeDatesToGetLastEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2003, 01, 01));

            var result = list.IndexOf(new Date(2001, 01, 01), TransationListPosition.Last);

            result.Should().Be(3);
        }

        [Fact]
        public void IndexOfSameDateAsLastEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2003, 01, 01));

            var result = list.IndexOf(new Date(2003, 01, 01), TransationListPosition.Last);

            result.Should().Be(5);
        }

        [Fact]
        public void IndexOfAfterLastEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2003, 01, 01));

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
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

            var result = list.Select(x => x.Date).ToArray();

            result.Should().Equal(new Date[] { new Date(2000, 01, 01), new Date(2001, 01, 01), new Date(2002, 01, 01) } );
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
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2003, 01, 01));

            var result = list.FromDate(new Date(2001, 01, 01)).Select(x => x.Date).ToArray();

            result.Should().Equal(new Date[] { new Date(2001, 01, 01), new Date(2001, 01, 01), new Date(2001, 01, 01), new Date(2002, 01, 01), new Date(2003, 01, 01) });
        }

        [Fact]
        public void EnumerateFromDateBetweenEntries()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2003, 01, 01));

            var result = list.FromDate(new Date(2001, 06, 01)).Select(x => x.Date).ToArray();

            result.Should().Equal(new Date[] { new Date(2002, 01, 01), new Date(2003, 01, 01) });
        }

        [Fact]
        public void EnumerateToDateOfEmptyList()
        {
            var list = new TransactionListTestClass();

            var result = list.ToDate(new Date(2000, 01, 01)).Select(x => x.Date).ToArray();

            result.Should().BeEmpty();
        }

        [Fact]
        public void EnumerateToDateMatchingMultipleEntries()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2003, 01, 01));

            var result = list.ToDate(new Date(2001, 01, 01)).Select(x => x.Date).ToArray();

            result.Should().Equal(new Date[] { new Date(2000, 01, 01), new Date(2001, 01, 01), new Date(2001, 01, 01), new Date(2001, 01, 01) });

        }

        [Fact]
        public void EnumerateToDateBetweenEntries()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2003, 01, 01));

            var result = list.ToDate(new Date(2001, 06, 01)).Select(x => x.Date).ToArray();

            result.Should().Equal(new Date[] { new Date(2000, 01, 01), new Date(2001, 01, 01), new Date(2001, 01, 01), new Date(2001, 01, 01) });
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
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2003, 01, 01));

            var result = list.InDateRange(new DateRange(new Date(2001, 01, 01), new Date(2002, 01, 01))).Select(x => x.Date).ToArray();

            result.Should().Equal(new Date[] { new Date(2001, 01, 01), new Date(2001, 01, 01), new Date(2001, 01, 01), new Date(2002, 01, 01), new Date(2002, 01, 01) });
        }

        [Fact]
        public void EnumerateRangeBetweenEntries()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2003, 01, 01));

            var result = list.InDateRange(new DateRange(new Date(2001, 06, 01), new Date(2002, 06, 01))).Select(x => x.Date).ToArray();


            result.Should().Equal(new Date[] { new Date(2002, 01, 01), new Date(2002, 01, 01) });

        }

        [Fact]
        public void RemoveAtEmptyList()
        {
            var list = new TransactionListTestClass();

            Action a = () => list.RemoveAt(1);
            
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void RemoveAtFirstEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

            list.RemoveAt(0);
            var result = list.Select(x => x.Date).ToArray();

            result.Should().Equal(new Date[] { new Date(2001, 01, 01), new Date(2002, 01, 01) });
        }

        [Fact]
        public void RemoveAtLastEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

            list.RemoveAt(2);
            var result = list.Select(x => x.Date).ToArray();

            result.Should().Equal(new Date[] { new Date(2000, 01, 01), new Date(2001, 01, 01) });
        }

        [Fact]
        public void RemoveAtMiddleEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

            list.RemoveAt(1);
            var result = list.Select(x => x.Date).ToArray();

            result.Should().Equal(new Date[] { new Date(2000, 01, 01), new Date(2002, 01, 01) });
        }

    }

    class TransactionTestClass : ITransaction
    {
        public Guid Id { get; }

        public Date Date { get; }

        public TransactionTestClass(Guid id, Date date)
        {
            Id = id;
            Date = date;
        }
    }
    class TransactionListTestClass : TransactionList<TransactionTestClass>
    {
        public void Add(Guid id, Date date)
        {
            Add(new TransactionTestClass(id, date));
        }

        public new void Clear()
        {
            base.Clear();
        }

        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);
        }
    }
}
