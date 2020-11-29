using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

using Xunit;
using Moq;
using FluentAssertions;

using Booth.PortfolioManager.Web.Utilities;
using Booth.EventStore;
using Booth.PortfolioManager.Domain.Portfolios;
using System.Reflection.Metadata;

namespace Booth.PortfolioManager.Web.Test.Utilities
{
    public class PortfolioCacheTests
    {

        [Fact]
        public void TryGetReturnsEntryInCache()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockResolver = mockRepository.Create<IStockResolver>(MockBehavior.Loose);
            var portfolioFactory = new PortfolioFactory(stockResolver.Object);

            var id = Guid.NewGuid();
            var portfolio = portfolioFactory.CreatePortfolio(id);

            var repositry = mockRepository.Create<IRepository<Portfolio>>();

            var memoryCache = mockRepository.Create<IMemoryCache>();
            object portfolioObject = portfolio;
            memoryCache.Setup(x => x.TryGetValue(id, out portfolioObject)).Returns(true);

            var cache = new PortfolioCache(repositry.Object, memoryCache.Object);

            var result = cache.TryGet(id, out var cachedPortfolio);

            result.Should().BeTrue();
            cachedPortfolio.Should().Be(portfolio);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void TryGetFetchesEntryNotInCache()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockResolver = mockRepository.Create<IStockResolver>(MockBehavior.Loose);
            var portfolioFactory = new PortfolioFactory(stockResolver.Object);

            var id = Guid.NewGuid();
            var portfolio = portfolioFactory.CreatePortfolio(id);

            var repositry = mockRepository.Create<IRepository<Portfolio>>();
            repositry.Setup(x => x.Get(id)).Returns(portfolio).Verifiable();

            var cacheEntry = mockRepository.Create<ICacheEntry>(MockBehavior.Loose);
            cacheEntry.SetupSet(x => x.Value = portfolio).Verifiable();

            var memoryCache = mockRepository.Create<IMemoryCache>();
            memoryCache.Setup(x => x.TryGetValue(id, out It.Ref<object>.IsAny)).Returns(false);
            memoryCache.Setup(x => x.CreateEntry(id)).Returns(cacheEntry.Object).Verifiable();

            var cache = new PortfolioCache(repositry.Object, memoryCache.Object);

            var result = cache.TryGet(id, out var cachedPortfolio);

            result.Should().BeTrue();
            cachedPortfolio.Should().Be(portfolio);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void TryGetEntryDoesNotExist()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            Portfolio portfolio = null;

            var repositry = mockRepository.Create<IRepository<Portfolio>>();
            repositry.Setup(x => x.Get(id)).Returns(portfolio);

            var memoryCache = mockRepository.Create<IMemoryCache>();
            memoryCache.Setup(x => x.TryGetValue(id, out It.Ref<object>.IsAny)).Returns(false);

            var cache = new PortfolioCache(repositry.Object, memoryCache.Object);

            var result = cache.TryGet(id, out var cachedPortfolio);

            result.Should().BeFalse();

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetReturnsEntryInCache()
        { 
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockResolver = mockRepository.Create<IStockResolver>(MockBehavior.Loose);
            var portfolioFactory = new PortfolioFactory(stockResolver.Object);

            var id = Guid.NewGuid();
            var portfolio = portfolioFactory.CreatePortfolio(id);

            var repositry = mockRepository.Create<IRepository<Portfolio>>();

            var memoryCache = mockRepository.Create<IMemoryCache>();
            object portfolioObject = portfolio;
            memoryCache.Setup(x => x.TryGetValue(id, out portfolioObject)).Returns(true);

            var cache = new PortfolioCache(repositry.Object, memoryCache.Object);

            var cachedPortfolio = cache.Get(id);

            cachedPortfolio.Should().Be(portfolio);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetFetchesEntryNotInCache()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockResolver = mockRepository.Create<IStockResolver>(MockBehavior.Loose);
            var portfolioFactory = new PortfolioFactory(stockResolver.Object);

            var id = Guid.NewGuid();
            var portfolio = portfolioFactory.CreatePortfolio(id);

            var repositry = mockRepository.Create<IRepository<Portfolio>>();
            repositry.Setup(x => x.Get(id)).Returns(portfolio).Verifiable();

            var cacheEntry = mockRepository.Create<ICacheEntry>(MockBehavior.Loose);
            cacheEntry.SetupSet(x => x.Value = portfolio).Verifiable();

            var memoryCache = mockRepository.Create<IMemoryCache>();
            memoryCache.Setup(x => x.TryGetValue(id, out It.Ref<object>.IsAny)).Returns(false);
            memoryCache.Setup(x => x.CreateEntry(id)).Returns(cacheEntry.Object).Verifiable();

            var cache = new PortfolioCache(repositry.Object, memoryCache.Object);

            var cachedPortfolio = cache.Get(id);

            cachedPortfolio.Should().Be(portfolio);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetEntryDoesNotExist()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            Portfolio portfolio = null;

            var repositry = mockRepository.Create<IRepository<Portfolio>>();
            repositry.Setup(x => x.Get(id)).Returns(portfolio);

            var memoryCache = mockRepository.Create<IMemoryCache>();
            memoryCache.Setup(x => x.TryGetValue(id, out It.Ref<object>.IsAny)).Returns(false);

            var cache = new PortfolioCache(repositry.Object, memoryCache.Object);

            IReadOnlyPortfolio cachedPortfolio;
            Action a = () => cachedPortfolio = cache.Get(id);

            a.Should().ThrowExactly<KeyNotFoundException>();

            mockRepository.VerifyAll();
        }
    }
}
