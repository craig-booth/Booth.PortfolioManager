using System;
using System.Collections.Generic;
using System.Text;

using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using Booth.PortfolioManager.Domain.Users;


namespace Booth.PortfolioManager.Repository
{

    public interface IUserRepository : IRepository<User>
    {

    }

    public class UserRepository : IUserRepository
    {
        protected readonly IMongoCollection<BsonDocument> _Collection;
        public UserRepository(IPortfolioManagerDatabase database)
        {
            _Collection = database.GetCollection("Users");
        }

        public User Get(Guid id)
        {
            var bson = _Collection.Find(Builders<BsonDocument>.Filter.Eq("_id", id)).SingleOrDefault();
            if (bson == null)
                return null;

            var user = BsonSerializer.Deserialize<User>(bson);

            return user;
        }

        public IEnumerable<User> All()
        {
            var bsonElements = _Collection.Find("{}").ToList();

            foreach (var bson in bsonElements)
            {
                var user = BsonSerializer.Deserialize<User>(bson);

                yield return user;
            }
        }

        public void Add(User entity)
        {
            var bson = entity.ToBsonDocument();

            _Collection.InsertOne(bson);
        }

        public void Update(User entity)
        {
            var bson = entity.ToBsonDocument();

            _Collection.ReplaceOne(Builders<BsonDocument>.Filter.Eq("_id", entity.Id), bson);
        }

        public void Delete(Guid id)
        {
            _Collection.DeleteOne(Builders<BsonDocument>.Filter.Eq("_id", id));
        }

    }
}