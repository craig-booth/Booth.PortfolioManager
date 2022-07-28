using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Booth.PortfolioManager.Domain;
using Booth.PortfolioManager.Repository;

namespace Booth.PortfolioManager.IntegrationTest.TestFixture
{
    internal class InMemoryRepository<T> : IRepository<T> where T : IEntity
    {
        protected Dictionary<Guid, T> _Entities = new Dictionary<Guid, T>();

        public void Add(T entity)
        {
            _Entities.Add(entity.Id, entity);
        }

        public IEnumerable<T> All()
        {
            return _Entities.Values;
        }

        public void Delete(Guid id)
        {
            _Entities.Remove(id);
        }

        public T Get(Guid id)
        {
            return _Entities[id];
        }

        public void Update(T entity)
        {
            _Entities[entity.Id] = entity;
        }
    }
}
