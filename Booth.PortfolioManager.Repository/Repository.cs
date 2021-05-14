using System;
using System.Collections.Generic;
using System.Text;

using Booth.EventStore;

namespace Booth.PortfolioManager.Repository
{
    public interface IRepository<T> where T : ITrackedEntity
    {
        T Get(Guid id);
        IEnumerable<T> All();
        void Add(T entity);
        void Update(T entity);
        void Delete(Guid id);
    }
}
