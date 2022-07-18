using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using Moq;
using FluentAssertions;

using Booth.PortfolioManager.Domain;
using Booth.PortfolioManager.Web.Utilities;

namespace Booth.PortfolioManager.Web.Test.Utilities
{
    public class EntityCacheTests
    {

        [Fact]
        public void GetEmptyCache()
        {

            var cache = new EntityCache<TestEntity>();

            var result = cache.Get(Guid.NewGuid());

            result.Should().BeNull();
        }

        [Fact]
        public void GetEntityNotInCache()
        {
            var cache = new EntityCache<TestEntity>();

            cache.Add(new TestEntity(Guid.NewGuid()));
            cache.Add(new TestEntity(Guid.NewGuid()));

            var result = cache.Get(Guid.NewGuid());

            result.Should().BeNull();
        }

        [Fact]
        public void GetEntityInCache()
        {
            var cache = new EntityCache<TestEntity>();

            var entity = new TestEntity(Guid.NewGuid());
            cache.Add(new TestEntity(Guid.NewGuid()));
            cache.Add(entity);

            var result = cache.Get(entity.Id);

            result.Should().Be(entity);
        }

        [Fact]
        public void AddEmptyCache()
        {
            var cache = new EntityCache<TestEntity>();

            var entity = new TestEntity(Guid.NewGuid());
            cache.Add(entity);

            cache.All().Should().HaveCount(1);
        }

        [Fact]
        public void AddEntityWithSameIdInCache()
        {
            var cache = new EntityCache<TestEntity>();

            var id = Guid.NewGuid();
            var entity = new TestEntity(id);
            cache.Add(entity);

            var entity2 = new TestEntity(id);
            cache.Add(entity2);

            cache.All().Should().HaveCount(1);
        }

        [Fact]
        public void Add()
        {
            var cache = new EntityCache<TestEntity>();

            var entity = new TestEntity(Guid.NewGuid());
            cache.Add(entity);

            var entity2 = new TestEntity(Guid.NewGuid());
            cache.Add(entity2);

            cache.All().Should().BeEquivalentTo(new[] { entity, entity2 });
        }

        [Fact]
        public void RemoveEntityNotInCache()
        {
            var cache = new EntityCache<TestEntity>();

            var entity = new TestEntity(Guid.NewGuid());
            cache.Add(entity);

            var entity2 = new TestEntity(Guid.NewGuid());
            cache.Add(entity2);

            cache.Remove(Guid.NewGuid());

            cache.All().Should().BeEquivalentTo(new[] { entity, entity2 });
        }

        [Fact]
        public void RemoveEntityInCache()
        {
            var cache = new EntityCache<TestEntity>();

            var entity = new TestEntity(Guid.NewGuid());
            cache.Add(entity);

            var entity2 = new TestEntity(Guid.NewGuid());
            cache.Add(entity2);

            cache.Remove(entity.Id);

            cache.All().Should().BeEquivalentTo(new[] { entity2 });

        }

        [Fact]
        public void Clear()
        {
            var cache = new EntityCache<TestEntity>();

            var entity = new TestEntity(Guid.NewGuid());
            cache.Add(entity);

            var entity2 = new TestEntity(Guid.NewGuid());
            cache.Add(entity2);

            cache.Clear();

            cache.All().Should().BeEmpty();
        }

        [Fact]
        public void ClearEmptyCache()
        {
            var cache = new EntityCache<TestEntity>();

            var entity = new TestEntity(Guid.NewGuid());
            cache.Add(entity);

            var entity2 = new TestEntity(Guid.NewGuid());
            cache.Add(entity2);

            cache.Clear();

            cache.All().Should().BeEmpty();
        }

        [Fact]
        public void AllEmptyCache()
        {
            var cache = new EntityCache<TestEntity>();

            cache.All().Should().BeEmpty();
        }

        [Fact]
        public void AllSingleItemInCache()
        {
            var cache = new EntityCache<TestEntity>();

            var entity = new TestEntity(Guid.NewGuid());
            cache.Add(entity);

            cache.All().Should().BeEquivalentTo(new[] { entity });
        }

        [Fact]
        public void AllMultipleItemsInCache()
        {
            var cache = new EntityCache<TestEntity>();

            var entity = new TestEntity(Guid.NewGuid());
            cache.Add(entity);

            var entity2 = new TestEntity(Guid.NewGuid());
            cache.Add(entity2);

            cache.All().Should().BeEquivalentTo(new[] { entity, entity2 });
        }
    }


    class TestEntity : IEntity
    {
        public Guid Id { get; set; }

        public TestEntity(Guid id)
        {
            Id = id;
        }
    }
}
