using System;
using System.Collections.Generic;
using System.Text;

namespace Booth.PortfolioManager.Domain
{
    public interface IEntityFactory<T> where T : IEntity
    {
        T Create(Guid id, string storedEntityType);
    }

    public class DefaultEntityFactory<T> : IEntityFactory<T> where T : IEntity
    {
        public T Create(Guid id, string storedEntityType)
        {
            var entity = (T)Activator.CreateInstance(typeof(T), new Object[] { id });           

            return entity;
        }
    }
}
