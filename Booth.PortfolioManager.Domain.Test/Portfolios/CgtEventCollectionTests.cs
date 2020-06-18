using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Test.Portfolios
{
    public class CgtEventCollectionTests
    {
        [Fact]
        public void Add()
        {
            var events = new CgtEventCollection();

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            events.Add(new Date(2000, 01, 01), stock, 100, 1000.00m, 1200.00m, 200.00m, CgtMethod.Indexation);

            events.Should().ContainSingle().Which.Should().BeEquivalentTo(new
            { 
                Date = new Date(2000, 01, 01),
                Stock = stock,
                Units = 100,
                AmountReceived = 1200.00m,
                CapitalGain = 200.00m,
                CgtMethod = CgtMethod.Indexation
            });
        }
    }
}
