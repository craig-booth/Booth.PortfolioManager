using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Test.Portfolios
{
    class CgtEventCollectionTests
    {
        [TestCase]
        public void Add()
        {
            var events = new CgtEventCollection();

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            events.Add(new Date(2000, 01, 01), stock, 100, 1000.00m, 1200.00m, 200.00m, CgtMethod.Indexation);

            Assert.Multiple(() =>
            {
                Assert.That(events[0].Id, Is.Not.EqualTo(Guid.Empty));
                Assert.That(events[0].Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(events[0].Stock, Is.EqualTo(stock));
                Assert.That(events[0].Units, Is.EqualTo(100));
                Assert.That(events[0].AmountReceived, Is.EqualTo(1200.00m));
                Assert.That(events[0].CapitalGain, Is.EqualTo(200.00m));
                Assert.That(events[0].CgtMethod, Is.EqualTo(CgtMethod.Indexation));
            });
        }
    }
}
