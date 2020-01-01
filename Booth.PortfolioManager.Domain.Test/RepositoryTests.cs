using System;
using System.Linq;

using NUnit.Framework;
using Moq;

using Booth.Common;
using Booth.EventStore;
using Booth.PortfolioManager.Domain;

namespace Booth.PortfolioManager.Domain.Test
{
    class RepositoryTests
    {

        [TestCase]
        public void GetWithMatchingId()
        {
            var id = Guid.NewGuid();

            var eventStream = Mock.Of<IEventStream<TrackedEntityTestClass>>(MockBehavior.Strict);
            Mock.Get(eventStream).Setup(x => x.Get(id)).Returns(new StoredEntity() { EntityId = id, Type = "" });

            var entityFactory = Mock.Of<IEntityFactory<TrackedEntityTestClass>>(MockBehavior.Strict);
            Mock.Get(entityFactory).Setup(x => x.Create(id, "")).Returns(new TrackedEntityTestClass(id));
            var repository = new Repository<TrackedEntityTestClass>(eventStream, entityFactory);

            
            var entity = repository.Get(id);

            Assert.That(entity.Id, Is.EqualTo(id));
        }

        [TestCase]
        public void GetWithoutMatchingId()
        {
            var id = Guid.NewGuid();

            var eventStream = Mock.Of<IEventStream<TrackedEntityTestClass>>(MockBehavior.Strict);
            Mock.Get(eventStream).Setup(x => x.Get(id)).Returns<StoredEntity>(null);

            var entityFactory = Mock.Of<IEntityFactory<TrackedEntityTestClass>>(MockBehavior.Strict);
            Mock.Get(entityFactory).Setup(x => x.Create(id, "")).Returns(new TrackedEntityTestClass(id));
            var repository = new Repository<TrackedEntityTestClass>(eventStream, entityFactory);

            var entity = repository.Get(id);

            Assert.That(entity, Is.Null);
        }

        [TestCase]
        public void GetAll()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var id3 = Guid.NewGuid();

            var storedEntities = new StoredEntity[]
            {
                new StoredEntity() { EntityId = id1, Type = "" },
                new StoredEntity() { EntityId = id2, Type = "" },
                new StoredEntity() { EntityId = id3, Type = "" }
            };

            var eventStream = Mock.Of<IEventStream<TrackedEntityTestClass>>(MockBehavior.Strict);
            Mock.Get(eventStream).Setup(x => x.GetAll()).Returns(storedEntities);

            var entityFactory = Mock.Of<IEntityFactory<TrackedEntityTestClass>>(MockBehavior.Strict);
            Mock.Get(entityFactory).Setup(x => x.Create(id1, "")).Returns(new TrackedEntityTestClass(id1));
            Mock.Get(entityFactory).Setup(x => x.Create(id2, "")).Returns(new TrackedEntityTestClass(id2));
            Mock.Get(entityFactory).Setup(x => x.Create(id3, "")).Returns(new TrackedEntityTestClass(id3));
            var repository = new Repository<TrackedEntityTestClass>(eventStream, entityFactory);


            var entities = repository.All().Select(x => x.Id).ToList();
            
            Assert.That(entities, Is.EqualTo( new Guid[] { id1, id2, id3 }));
        }

        [TestCase]
        public void GetAllReturningEmptyList()
        {
            var eventStream = Mock.Of<IEventStream<TrackedEntityTestClass>>(MockBehavior.Strict);
            Mock.Get(eventStream).Setup(x => x.GetAll()).Returns(new StoredEntity[] { });

            var entityFactory = Mock.Of<IEntityFactory<TrackedEntityTestClass>>(MockBehavior.Strict);
            var repository = new Repository<TrackedEntityTestClass>(eventStream, entityFactory);

            var entities = repository.All();

            Assert.That(entities, Is.Empty);
        }

        [TestCase]
        public void AddEntity()
        {
            var entityId = Guid.NewGuid();
            var events = new Event[]
            {
                new EventTestClass(Guid.NewGuid(), 0)
            };
            var trackedEntity = new TrackedEntityTestClass(entityId);
            trackedEntity.AddEvent(events[0]);

            var eventStream = Mock.Of<IEventStream<TrackedEntityTestClass>>(MockBehavior.Strict);
            Mock.Get(eventStream).Setup(x => x.Add(entityId, "TrackedEntityTestClass", events));

            var entityFactory = Mock.Of<IEntityFactory<TrackedEntityTestClass>>(MockBehavior.Strict);

            var repository = new Repository<TrackedEntityTestClass>(eventStream, entityFactory);

            repository.Add(trackedEntity);

            Mock.VerifyAll();
        }

        [TestCase]
        public void AddEntityWithProperties()
        {
            var entityId = Guid.NewGuid();
            var events = new Event[]
            {
                new EventTestClass(Guid.NewGuid(), 0)
            };
            var trackedEntity = new TrackedEntityWithPropertiesTestClass(entityId);
            trackedEntity.Properties.Add("Name1", "Value1");
            trackedEntity.AddEvent(events[0]);
  
            var eventStream = Mock.Of<IEventStream<TrackedEntityWithPropertiesTestClass>>(MockBehavior.Strict);
            Mock.Get(eventStream).Setup(x => x.Add(entityId, "TrackedEntityWithPropertiesTestClass", trackedEntity.Properties, events));

            var entityFactory = Mock.Of<IEntityFactory<TrackedEntityWithPropertiesTestClass>>(MockBehavior.Strict);

            var repository = new Repository<TrackedEntityWithPropertiesTestClass>(eventStream, entityFactory);

            repository.Add(trackedEntity);

            Mock.VerifyAll();
        }

        [TestCase]
        public void UpdateExistingEntity()
        {
            var entityId = Guid.NewGuid();
            var events = new Event[]
            {
                new EventTestClass(Guid.NewGuid(), 0)
            };
            var trackedEntity = new TrackedEntityTestClass(entityId);
            trackedEntity.AddEvent(events[0]);

            var eventStream = Mock.Of<IEventStream<TrackedEntityTestClass>>(MockBehavior.Strict);
            Mock.Get(eventStream).Setup(x => x.AppendEvents(entityId, events));

            var entityFactory = Mock.Of<IEntityFactory<TrackedEntityTestClass>>(MockBehavior.Strict);

            var repository = new Repository<TrackedEntityTestClass>(eventStream, entityFactory);

            repository.Update(trackedEntity);

            Mock.VerifyAll();
        }

        [TestCase]
        public void FindFirstOnlyMatchingEntity()
        {
            var id = Guid.NewGuid();

            var eventStream = Mock.Of<IEventStream<TrackedEntityTestClass>>(MockBehavior.Strict);
            Mock.Get(eventStream).Setup(x => x.FindFirst("Property", "Value")).Returns(new StoredEntity() { EntityId = id, Type = "" });

            var entityFactory = Mock.Of<IEntityFactory<TrackedEntityTestClass>>(MockBehavior.Strict);
            Mock.Get(entityFactory).Setup(x => x.Create(id, "")).Returns(new TrackedEntityTestClass(id));

            var repository = new Repository<TrackedEntityTestClass>(eventStream, entityFactory);

            var entity = repository.FindFirst("Property", "Value");

            Assert.That(entity.Id, Is.EqualTo(id));
            Mock.VerifyAll();
        }


        [TestCase]
        public void FindFirstNoMatchingEntities()
        {
            var id = Guid.NewGuid();

            var eventStream = Mock.Of<IEventStream<TrackedEntityTestClass>>(MockBehavior.Strict);
            Mock.Get(eventStream).Setup(x => x.FindFirst("Property", "Value")).Returns<StoredEntity>(null);

            var entityFactory = Mock.Of<IEntityFactory<TrackedEntityTestClass>>(MockBehavior.Strict);

            var repository = new Repository<TrackedEntityTestClass>(eventStream, entityFactory);

            var entity = repository.FindFirst("Property", "Value");

            Assert.That(entity, Is.Null);
            Mock.VerifyAll();
        }

        [TestCase]
        public void FindSingleMatchingEntity()
        {
            var id = Guid.NewGuid();

            var eventStream = Mock.Of<IEventStream<TrackedEntityTestClass>>(MockBehavior.Strict);
            Mock.Get(eventStream).Setup(x => x.Find("Property", "Value")).Returns(new StoredEntity[] { new StoredEntity() { EntityId = id, Type = "" } });

            var entityFactory = Mock.Of<IEntityFactory<TrackedEntityTestClass>>(MockBehavior.Strict);
            Mock.Get(entityFactory).Setup(x => x.Create(id, "")).Returns(new TrackedEntityTestClass(id));

            var repository = new Repository<TrackedEntityTestClass>(eventStream, entityFactory);

            var entities = repository.Find("Property", "Value");

            Assert.That(entities.Count(), Is.EqualTo(1));
            Mock.VerifyAll();
        }

        [TestCase]
        public void FindMultipleMatchingEntities()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var storedEntities = new StoredEntity[]
            {
                new StoredEntity() { EntityId = id1, Type = "" },
                new StoredEntity() { EntityId = id2, Type = "" }
            };

            var eventStream = Mock.Of<IEventStream<TrackedEntityTestClass>>(MockBehavior.Strict);
            Mock.Get(eventStream).Setup(x => x.Find("Property", "Value")).Returns(storedEntities);

            var entityFactory = Mock.Of<IEntityFactory<TrackedEntityTestClass>>(MockBehavior.Strict);
            Mock.Get(entityFactory).Setup(x => x.Create(id1, "")).Returns(new TrackedEntityTestClass(id1));
            Mock.Get(entityFactory).Setup(x => x.Create(id2, "")).Returns(new TrackedEntityTestClass(id2));

            var repository = new Repository<TrackedEntityTestClass>(eventStream, entityFactory);

            var entity = repository.Find("Property", "Value");

            Assert.That(entity.Count(), Is.EqualTo(2));
            Mock.VerifyAll();
        }

        [TestCase]
        public void FindNoMatchingEntities()
        {
            var eventStream = Mock.Of<IEventStream<TrackedEntityTestClass>>(MockBehavior.Strict);
            Mock.Get(eventStream).Setup(x => x.Find("Property", "Value")).Returns(new StoredEntity[] { });

            var entityFactory = Mock.Of<IEntityFactory<TrackedEntityTestClass>>(MockBehavior.Strict);

            var repository = new Repository<TrackedEntityTestClass>(eventStream, entityFactory);

            var entities = repository.Find("Property", "Value");

            Assert.That(entities, Is.Empty);
            Mock.VerifyAll();
        }
    }
}
