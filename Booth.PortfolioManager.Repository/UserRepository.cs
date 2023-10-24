using System;
using System.Threading.Tasks;

using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using Booth.PortfolioManager.Domain.Users;


namespace Booth.PortfolioManager.Repository
{

    public interface IUserRepository : IRepository<User>
    {
        User GetUserByUserName(string userName);
        Task<User> GetUserByUserNameAsync(string userName);
    }

    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(IPortfolioManagerDatabase database)
            : base(database, "Users")
        {

        }

        public User GetUserByUserName(string userName)
        {
            return FindFirst("userName", userName);
        }

        public Task<User> GetUserByUserNameAsync(string userName)
        {
            throw new NotImplementedException();
        }
    }
}