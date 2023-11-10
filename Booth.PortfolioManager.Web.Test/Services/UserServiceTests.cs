using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Xunit;
using FluentAssertions;
using Moq;

using Booth.PortfolioManager.Repository; 
using Booth.PortfolioManager.Domain.Users;
using Booth.PortfolioManager.Web.Services;


namespace Booth.PortfolioManager.Web.Test.Services
{
    public class UserServiceTests
    {

        [Fact]
        public async Task AuthenticateUserNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var repository = mockRepository.Create<IUserRepository>();
            repository.Setup(x => x.GetUserByUserNameAsync("user")).Returns(Task.FromResult(default(User)));

            var service = new UserService(repository.Object);

            var result = await service.AuthenticateAsync("user", "password");

            result.Should().HaveErrorStatus().WithError("User not found");

            mockRepository.Verify();
        }

        [Fact]
        public async Task AuthenticatePasswordWrong()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var user = new User(Guid.NewGuid());
            user.Create("user", "password");

            var repository = mockRepository.Create<IUserRepository>();
            repository.Setup(x => x.GetUserByUserNameAsync("user")).Returns(Task.FromResult(user));

            var service = new UserService(repository.Object);

            var result = await service.AuthenticateAsync("user", "wrong");

            result.Should().HaveErrorStatus().WithError("Incorrect Password");

            mockRepository.Verify();
        }

        [Fact]
        public async Task AuthenticateSuccessful()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var user = new User(Guid.NewGuid());
            user.Create("user", "password");

            var repository = mockRepository.Create<IUserRepository>();
            repository.Setup(x => x.GetUserByUserNameAsync("user")).Returns(Task.FromResult(user));

            var service = new UserService(repository.Object);

            var result = await service.AuthenticateAsync("user", "password");

            result.Result.Should().Be(user);

            mockRepository.Verify();
        }

    }
}
