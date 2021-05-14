using System;
using System.Collections.Generic;
using System.Text;

using MongoDB.Driver;
using MongoDB.Bson;

using Booth.PortfolioManager.Domain.Users;
using Booth.PortfolioManager.Domain.Users.Events;

namespace Booth.PortfolioManager.Repository
{

    public class UserRepository : IRepository<User>
    {
        private readonly IMongoCollection<BsonDocument> _Collection;
        public UserRepository(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);

            _Collection = database.GetCollection<BsonDocument>("Users");
        }

        public User Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<User> All()
        {
            throw new NotImplementedException();
        }

        public void Add(User user)
        {
            var document = new BsonDocument()
            {
                { "id", new BsonString(user.Id.ToString()) },
                { "userName", new BsonString(user.UserName) },
                { "password", new BsonString(user.Password) },
                { "administrator", new BsonBoolean(user.Administator) }
            };

            _Collection.InsertOne(document);
        }

        public void Update(User user)
        {
            var events = user.FetchEvents();

            var update = Builders<BsonDocument>.Update.Set("", "");
            foreach (var @event in events)
            {
                switch (@event)
                {
                    case PasswordChangedEvent passwordChangedEvent:
                        update = update.Set("password", new BsonString(user.Password));
                        break;
                    case UserAdministratorChangedEvent administratorChangedEvent:
                        update = update.Set("administrator", new BsonBoolean(user.Administator));
                        break;
                    case UserNameChangedEvent nameChangedEvent:
                        update = update.Set("userName", new BsonString(user.UserName));
                        break;
                }
            }          

            _Collection.UpdateOne(Builders<BsonDocument>.Filter.Eq("id", user.Id), update);
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }



    }
}