using System;
using System.Linq;

using NUnit.Framework;

using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Test.Stocks
{
    class StockEntityFactoryTests
    {

        [TestCase]
        public void CreateStock()
        {
            var factory = new StockEntityFactory();

            var id = Guid.NewGuid();
            var result = factory.Create(id, "Stock");

            Assert.That(result, Is.TypeOf<Stock>());
            Assert.That(result.Id, Is.EqualTo(id));
        }

        [TestCase]
        public void CreateStapledSecurity()
        {
            var factory = new StockEntityFactory();

            var id = Guid.NewGuid();
            var result = factory.Create(id, "StapledSecurity");

            Assert.That(result, Is.TypeOf<StapledSecurity>());
            Assert.That(result.Id, Is.EqualTo(id));
        }

        [TestCase]
        public void CreateUnknownType()
        {
            var factory = new StockEntityFactory();

            var id = Guid.NewGuid();

            Assert.That(() => factory.Create(id, "xxx"), Throws.ArgumentException);
        }
    }
}
