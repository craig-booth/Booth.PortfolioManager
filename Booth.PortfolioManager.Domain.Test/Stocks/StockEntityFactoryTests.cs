using System;
using System.Linq;

using Xunit;
using FluentAssertions;

using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Test.Stocks
{
    public class StockEntityFactoryTests
    {

        [Fact]
        public void CreateStock()
        {
            var factory = new StockEntityFactory();

            var id = Guid.NewGuid();
            var result = factory.Create(id, "Stock");

            result.Should().BeOfType<Stock>().Which.Id.Should().Be(id);
        }

        [Fact]
        public void CreateStapledSecurity()
        {
            var factory = new StockEntityFactory();

            var id = Guid.NewGuid();
            var result = factory.Create(id, "StapledSecurity");

            result.Should().BeOfType<StapledSecurity>().Which.Id.Should().Be(id);
        }

        [Fact]
        public void CreateUnknownType()
        {
            var factory = new StockEntityFactory();

            var id = Guid.NewGuid();

            Action a = () => factory.Create(id, "xxx");
            
            a.Should().Throw<ArgumentException>();
        }
    }
}
