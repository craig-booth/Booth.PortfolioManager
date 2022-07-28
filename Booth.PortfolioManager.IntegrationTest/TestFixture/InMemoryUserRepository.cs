using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Booth.PortfolioManager.Domain.Users;
using Booth.PortfolioManager.Repository;

namespace Booth.PortfolioManager.IntegrationTest.TestFixture
{
    internal class InMemoryUserRepository : InMemoryRepository<User>, IUserRepository
    {
        public User GetUserByUserName(string userName)
        {
            return _Entities.Values.FirstOrDefault(x => x.UserName == userName);
        }
    }
}
