using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.Domain.Users;

namespace Booth.PortfolioManager.Web.Services
{
    public interface IUserService
    {
        Task<ServiceResult<User>> AuthenticateAsync(string userName, string password);
    }

    class UserService : IUserService
    {
        private readonly IUserRepository _UserRepository;

        public UserService(IUserRepository userRepository)
        {
            _UserRepository = userRepository;
        }

        public async Task<ServiceResult<User>> AuthenticateAsync(string userName, string password)
        {
            var user = await _UserRepository.GetUserByUserNameAsync(userName);
            if (user == null)
                return ServiceResult<User>.Error("User not found");

            if (!user.PasswordCorrect(password))
                return ServiceResult<User>.Error("Incorrect Password");

            return ServiceResult<User>.Ok(user);
        }
    }
}
