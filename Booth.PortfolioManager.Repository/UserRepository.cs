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

    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(IPortfolioManagerDatabase database)
            : base(database, "Users")
        {

        }

    }
}