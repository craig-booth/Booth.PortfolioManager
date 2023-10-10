using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Repository.Serialization;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Booth.PortfolioManager.Repository.Test.Serialization
{
    public class SerializationTestFixture : IDisposable
    {
        public PortfolioFactory PortfolioFactory;

        private SerializartionTestStockResolver _StockResolver;
        public IStockResolver StockResolver => _StockResolver;

        public SerializationTestFixture() 
        {
            _StockResolver = new SerializartionTestStockResolver();
            PortfolioFactory = new PortfolioFactory(StockResolver);

            SerializationProvider.Register(PortfolioFactory, StockResolver);
        }

        public void SetStockCache(IEnumerable<Stock> stocks)
        {
            _StockResolver.Stocks.Clear();
            _StockResolver.Stocks.AddRange(stocks);
        }
        private class SerializartionTestStockResolver : IStockResolver
        {
            public List<Stock> Stocks = new List<Stock>();

            public IReadOnlyStock GetStock(Guid id)
            {
                return Stocks.Find(x => x.Id == id);
            }
        }

        public void Dispose()
        {
            
        }
    }

    static class Collection
    {
        public const string Serialization = "Serialization collection";
    }

    [CollectionDefinition(Collection.Serialization)]
    public class SerializationCollection : ICollectionFixture<SerializationTestFixture>
    {
    }
}
