using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.EventStore;
using Booth.PortfolioManager.Domain.Users;

namespace Booth.PortfolioManager.Web.Services
{
    public interface IUserService
    {
        ServiceResult<User> Authenticate(string userName, string password);
    }

    class UserService : IUserService
    {
        private readonly IRepository<User> _UserRepository;

        public UserService(IRepository<User> userRepository)
        {
            _UserRepository = userRepository;
        }

        public ServiceResult<User> Authenticate(string userName, string password)
        {
            var user = _UserRepository.FindFirst("UserName", userName);
            if (user == null)
                return ServiceResult<User>.Error("User not found");

            if (! user.PasswordCorrect(password))
                return ServiceResult<User>.Error("Incorrect Password");

            return ServiceResult<User>.Ok(user);
        }
    }
}
