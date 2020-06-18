using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using Moq;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Web.Utilities;


namespace Booth.PortfolioManager.Web.Test.Utilities
{
    public class StockQueryTests
    {

        private IEntityCache<Stock> _StockCache;
        private Stock _Stock1;
        private Stock _Stock2;
        private Stock _Stock3;

        public StockQueryTests()
        {
            _StockCache = new EntityCache<Stock>();

            _Stock1 = new Stock(Guid.NewGuid());
            _Stock1.List("ABC", "ABC Pty Ltd", new Date(2000, 01, 01), false, AssetCategory.AustralianStocks);
            _Stock1.DeList(new Date(2010, 01, 01));
            _StockCache.Add(_Stock1);

            _Stock2 = new Stock(Guid.NewGuid());
            _Stock2.List("XYZ", "XYZ Pty Ltd", new Date(2000, 01, 01), false, AssetCategory.AustralianStocks);
            _Stock2.ChangeProperties(new Date(2005, 01, 01), "XYZ2", "XYZ 2 Pty Ltd", AssetCategory.AustralianStocks);
            _StockCache.Add(_Stock2);

            _Stock3 = new Stock(Guid.NewGuid());
            _Stock3.List("DEF", "DEF Pty Ltd", new Date(2002, 01, 01), false, AssetCategory.AustralianProperty);
            _Stock3.ChangeProperties(new Date(2005, 01, 01), "XYZ", "XYZ Pty Ltd", AssetCategory.AustralianProperty);
            _StockCache.Add(_Stock3);
        }

        [Fact]
        public void GetByIdStockInCache()
        { 
            var query = new StockQuery(_StockCache);

            var result = query.Get(_Stock1.Id);

            result.Should().Be(_Stock1);
        }

        [Fact]
        public void GetByIdStockNotInCache()
        {
            var query = new StockQuery(_StockCache);

            var result = query.Get(Guid.NewGuid());

            result.Should().BeNull();
        }

        [Fact]
        public void GetByCodeNotInCache()
        {
            var query = new StockQuery(_StockCache);

            var result = query.Get("AAA", new Date(2000, 01, 01));

            result.Should().BeNull();
        }

        [Fact]
        public void GetByCodeNotInCacheAtDate()
        {
            var query = new StockQuery(_StockCache);

            var result = query.Get("ABC", new Date(2012, 01, 01));

            result.Should().BeNull();
        }

        [Fact]
        public void GetByCodeInCache()
        {
            var query = new StockQuery(_StockCache);

            var result = query.Get("ABC", new Date(2002, 01, 01));

            result.Should().Be(_Stock1);
        }

        [Fact]
        public void GetByCodeTwoStocksWithSameCode()
        {
            var query = new StockQuery(_StockCache);

            var result = query.Get("XYZ", new Date(2006, 01, 01));

            result.Should().Be(_Stock3);
        }

        [Fact]
        public void All()
        {
            var query = new StockQuery(_StockCache);

            var result = query.All();

            result.Should().BeEquivalentTo(new[] { _Stock1, _Stock2, _Stock3 });
        }


        [Fact]
        public void AllAtDate()
        {
            var query = new StockQuery(_StockCache);

            var result = query.All(new Date(2012, 01, 01));

            result.Should().BeEquivalentTo(new[] { _Stock2, _Stock3 });
        }

        [Fact]
        public void AllInDateRange()
        {
            var query = new StockQuery(_StockCache);

            var result = query.All(new DateRange(new Date(2011, 02, 01), new Date(2015, 01, 01)));

            result.Should().BeEquivalentTo(new[] { _Stock2, _Stock3 });
        }

        [Fact]
        public void Find()
        {
            var query = new StockQuery(_StockCache);

            var result = query.Find(x => x.AsxCode == "ABC");

            result.Should().BeEquivalentTo(new[] { _Stock1 });
        }

        [Fact]
        public void FindAtDate()
        {
            var query = new StockQuery(_StockCache);

            var result = query.Find(new Date(2005, 01, 01), x => x.AsxCode == "XYZ");

            result.Should().BeEquivalentTo(new[] { _Stock3 });
        }


        [Fact]
        public void FindInDateRange()
        {
            var query = new StockQuery(_StockCache);

            var result = query.Find(new DateRange(new Date(2001, 02, 01), new Date(2009, 01, 01)), x => x.Category == AssetCategory.AustralianProperty);

            result.Should().BeEquivalentTo(new[] { _Stock3 });
        }
    
    }
}
