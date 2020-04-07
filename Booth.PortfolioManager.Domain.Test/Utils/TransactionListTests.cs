using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Booth.Common;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.Test.Utils
{
    class TransactionListTests
    {

        [TestCase]
        public void AccessByIndex()
        {
            var list = new TransactionListTestClass();

            var id = Guid.NewGuid();
            list.Add(id, new Date(2000, 01, 01));
            var id2 = Guid.NewGuid();
            list.Add(id2, new Date(2001, 01, 01));

            var entry = list[1];

            Assert.That(entry.Date, Is.EqualTo(new Date(2001, 01, 01)));
        }

        [TestCase]
        public void AccessByIndexNoEntry()
        {
            var list = new TransactionListTestClass();

            var id = Guid.NewGuid();
            list.Add(id, new Date(2000, 01, 01));
            var id2 = Guid.NewGuid();
            list.Add(id2, new Date(2001, 01, 01));

            Assert.That(() => list[2], Throws.Exception.InstanceOf(typeof(ArgumentOutOfRangeException)));
        }

        [TestCase]
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
            Assert.That(entry.Date, Is.EqualTo(new Date(2002, 01, 01)));
        }

        [TestCase]
        public void SetByIndexNoEntry()
        {
            var list = new TransactionListTestClass();

            var id = Guid.NewGuid();
            list.Add(id, new Date(2000, 01, 01));
            var id2 = Guid.NewGuid();
            list.Add(id2, new Date(2001, 01, 01));

            Assert.That(() => list[2] = new TransactionTestClass(id, new Date(2002, 01, 01)), Throws.Exception.InstanceOf(typeof(ArgumentOutOfRangeException)));
        }

        [TestCase]
        public void AccessById()
        {
            var list = new TransactionListTestClass();

            var id = Guid.NewGuid();
            list.Add(id, new Date(2000, 01, 01));
            var id2 = Guid.NewGuid();
            list.Add(id2, new Date(2001, 01, 01));

            var entry = list[id];

            Assert.That(entry.Date, Is.EqualTo(new Date(2000, 01, 01)));
        }

        [TestCase]
        public void AccessByIdNoEntry()
        {
            var list = new TransactionListTestClass();

            var id = Guid.NewGuid();
            list.Add(id, new Date(2000, 01, 01));
            var id2 = Guid.NewGuid();
            list.Add(id2, new Date(2001, 01, 01));

            Assert.That(() => list[Guid.NewGuid()], Throws.Exception.InstanceOf(typeof(KeyNotFoundException)));
        }

        [TestCase]
        public void SetById()
        {
            var list = new TransactionListTestClass();

            var id = Guid.NewGuid();
            list.Add(id, new Date(2000, 01, 01));
            var id2 = Guid.NewGuid();
            list.Add(id2, new Date(2001, 01, 01));

            list[id] = new TransactionTestClass(id, new Date(2002, 01, 01));

            var entry = list[id];
            Assert.That(entry.Date, Is.EqualTo(new Date(2002, 01, 01)));
        }

        [TestCase]
        public void SetByIdNoEntry()
        {
            var list = new TransactionListTestClass();

            var id = Guid.NewGuid();
            list.Add(id, new Date(2000, 01, 01));
            var id2 = Guid.NewGuid();
            list.Add(id2, new Date(2001, 01, 01));

            Assert.That(() => list[Guid.NewGuid()], Throws.Exception.InstanceOf(typeof(KeyNotFoundException)));
        }

        [TestCase]
        public void Count()
        {
            var list = new TransactionListTestClass();

            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

            Assert.That(list.Count, Is.EqualTo(3));
        }


        [TestCase]
        public void CountNoEnties()
        {
            var list = new TransactionListTestClass();

            Assert.That(list.Count, Is.EqualTo(0));
        }

        [TestCase]
        public void Earliest()
        {
            var list = new TransactionListTestClass();

            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

            Assert.That(list.Earliest, Is.EqualTo(new Date(2000, 01, 01)));
        }

        [TestCase]
        public void EarliestNoEnties()
        {
            var list = new TransactionListTestClass();

            Assert.That(list.Earliest, Is.EqualTo(Date.MinValue));
        }

        [TestCase]
        public void Latest()
        {
            var list = new TransactionListTestClass();

            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

            Assert.That(list.Latest, Is.EqualTo(new Date(2002, 01, 01)));
        }

        [TestCase]
        public void LatestNoEnties()
        {
            var list = new TransactionListTestClass();

            Assert.That(list.Earliest, Is.EqualTo(Date.MinValue));
        }

        [TestCase]
        public void AddToEmptyList()
        {
            var list = new TransactionListTestClass();

            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));

            Assert.That(list.Count, Is.EqualTo(1));
        }

        [TestCase]
        public void AddWithSameIdAsExistingEntry()
        {
            var list = new TransactionListTestClass();

            var id = Guid.NewGuid();
            list.Add(id, new Date(2000, 01, 01));

            Assert.That(() => list.Add(id, new Date(2001, 01, 01)), Throws.Exception.InstanceOf(typeof(ArgumentException)));
        }

        [TestCase]
        public void AddBeforeFirstEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

            var id = Guid.NewGuid();
            list.Add(id, new Date(1999, 01, 01));
            Assert.That(list[0].Id, Is.EqualTo(id));
        }

        [TestCase]
        public void AddSameDateAsFirstEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

            var id = Guid.NewGuid();
            list.Add(id, new Date(2000, 01, 01));
            Assert.That(list[1].Id, Is.EqualTo(id));
        }

        [TestCase]
        public void AddInMiddle()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

            var id = Guid.NewGuid();
            list.Add(id, new Date(2001, 06, 01));
            Assert.That(list[2].Id, Is.EqualTo(id));
        }

        [TestCase]
        public void AddInMiddleOnTheSameDayAsAnExitingEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

            var id = Guid.NewGuid();
            list.Add(id, new Date(2001, 01, 01));
            Assert.That(list[2].Id, Is.EqualTo(id));
        }


        [TestCase]
        public void AddSameDateAsLastEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

            var id = Guid.NewGuid();
            list.Add(id, new Date(2002, 01, 01));
            Assert.That(list[3].Id, Is.EqualTo(id));
        }

        [TestCase]
        public void AddAfterLastEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

            var id = Guid.NewGuid();
            list.Add(id, new Date(2003, 01, 01));
            Assert.That(list[3].Id, Is.EqualTo(id));
        }

        [TestCase]
        public void Clear()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

            list.Clear();
            Assert.That(list.Count, Is.EqualTo(0));
        }

        [TestCase]
        public void ClearNoEntries()
        {
            var list = new TransactionListTestClass();

            list.Clear();
            Assert.That(list.Count, Is.EqualTo(0));
        }

        [TestCase]
        public void IndexOfEmptyList()
        {
            var list = new TransactionListTestClass();

            var result = list.IndexOf(new Date(2000, 01, 01), TransationListPosition.First);

            Assert.That(result, Is.EqualTo(~0));
        }

        [TestCase]
        public void IndexOfBeforeFirstEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2003, 01, 01));

            var result = list.IndexOf(new Date(1999, 01, 01), TransationListPosition.First);

            Assert.That(result, Is.EqualTo(~0));
        }

        [TestCase]
        public void IndexOfFirstEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2003, 01, 01));

            var result = list.IndexOf(new Date(2000, 01, 01), TransationListPosition.First);

            Assert.That(result, Is.EqualTo(0));
        }

        [TestCase]
        public void IndexOfBetweenTwoDates()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2003, 01, 01));

            var result = list.IndexOf(new Date(2001, 06, 01), TransationListPosition.First);

            Assert.That(result, Is.EqualTo(~3));
        }

        [TestCase]
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

            Assert.That(result, Is.EqualTo(1));
        }

        [TestCase]
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

            Assert.That(result, Is.EqualTo(3));
        }

        [TestCase]
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

            Assert.That(result, Is.EqualTo(5));
        }

        [TestCase]
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

            Assert.That(result, Is.EqualTo(~6));
        }

        [TestCase]
        public void EnumerateEmptyList()
        {
            var list = new TransactionListTestClass();

            var result = list.ToList();
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [TestCase]
        public void EnumerateList()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

            var result = list.Select(x => x.Date).ToArray();
            Assert.That(result, Is.EqualTo(new Date[] { new Date(2000, 01, 01), new Date(2001, 01, 01), new Date(2002, 01, 01) } ));
        }

        [TestCase]
        public void EnumerateFromDateOfEmptyList()
        {
            var list = new TransactionListTestClass();

            var result = list.FromDate(new Date(2000, 01, 01)).Select(x => x.Date).ToArray();
            Assert.That(result, Is.Empty);
        }

        [TestCase]
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
            Assert.That(result, Is.EqualTo(new Date[] { new Date(2001, 01, 01), new Date(2001, 01, 01), new Date(2001, 01, 01), new Date(2002, 01, 01), new Date(2003, 01, 01) }));
        }

        [TestCase]
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
            Assert.That(result, Is.EqualTo(new Date[] { new Date(2002, 01, 01), new Date(2003, 01, 01) }));
        }

        [TestCase]
        public void EnumerateToDateOfEmptyList()
        {
            var list = new TransactionListTestClass();

            var result = list.ToDate(new Date(2000, 01, 01)).Select(x => x.Date).ToArray();
            Assert.That(result, Is.Empty);
        }

        [TestCase]
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
            Assert.That(result, Is.EqualTo(new Date[] { new Date(2000, 01, 01), new Date(2001, 01, 01), new Date(2001, 01, 01), new Date(2001, 01, 01) }));

        }

        [TestCase]
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
            Assert.That(result, Is.EqualTo(new Date[] { new Date(2000, 01, 01), new Date(2001, 01, 01), new Date(2001, 01, 01), new Date(2001, 01, 01) }));
        }

        [TestCase]
        public void EnumerateRangeOfEmptyList()
        {
            var list = new TransactionListTestClass();

            var result = list.InDateRange(new DateRange(new Date(2000, 01, 01), new Date(2003, 01, 01))).Select(x => x.Date).ToList();
            Assert.That(result, Is.Empty);
        }

        [TestCase]
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
            Assert.That(result, Is.EqualTo(new Date[] { new Date(2001, 01, 01), new Date(2001, 01, 01), new Date(2001, 01, 01), new Date(2002, 01, 01), new Date(2002, 01, 01) }));
        }

        [TestCase]
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
            Assert.That(result, Is.EqualTo(new Date[] { new Date(2002, 01, 01), new Date(2002, 01, 01) }));

        }

        [TestCase]
        public void RemoveAtEmptyList()
        {
            var list = new TransactionListTestClass();

            Assert.That(() => list.RemoveAt(1), Throws.Exception.InstanceOf(typeof(ArgumentOutOfRangeException)));
        }

        [TestCase]
        public void RemoveAtFirstEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

            list.RemoveAt(0);
            var result = list.Select(x => x.Date).ToArray();
            Assert.That(result, Is.EqualTo(new Date[] { new Date(2001, 01, 01), new Date(2002, 01, 01) }));
        }

        [TestCase]
        public void RemoveAtLastEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

            list.RemoveAt(2);
            var result = list.Select(x => x.Date).ToArray();
            Assert.That(result, Is.EqualTo(new Date[] { new Date(2000, 01, 01), new Date(2001, 01, 01) }));
        }

        [TestCase]
        public void RemoveAtMiddleEntry()
        {
            var list = new TransactionListTestClass();
            list.Add(Guid.NewGuid(), new Date(2000, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2001, 01, 01));
            list.Add(Guid.NewGuid(), new Date(2002, 01, 01));

            list.RemoveAt(1);
            var result = list.Select(x => x.Date).ToArray();
            Assert.That(result, Is.EqualTo(new Date[] { new Date(2000, 01, 01), new Date(2002, 01, 01) }));
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
