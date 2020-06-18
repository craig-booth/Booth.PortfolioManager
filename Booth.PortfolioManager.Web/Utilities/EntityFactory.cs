using System;
using System.Collections.Generic;
using System.Text;

using Booth.EventStore;

namespace Booth.PortfolioManager.Web.Utilities
{
    interface IEntityFactory<T> where T : IEntity
    {
        T Create(Guid id, string storedEntityType);
    }

    class DefaultEntityFactory<T> : IEntityFactory<T> where T : IEntity
    {
        public T Create(Guid id, string storedEntityType)
        {
            var entity = (T)Activator.CreateInstance(typeof(T), new Object[] { id });           

            return entity;
        }
    }
}
