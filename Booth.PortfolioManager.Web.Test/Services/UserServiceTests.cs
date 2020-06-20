using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using FluentAssertions;
using Moq;

using Booth.EventStore;
using Booth.PortfolioManager.Domain.Users;
using Booth.PortfolioManager.Web.Services;


namespace Booth.PortfolioManager.Web.Test.Services
{
    public class UserServiceTests
    {

        [Fact]
        public void AuthenticateUserNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var repository = mockRepository.Create<IRepository<User>>();
            repository.Setup(x => x.FindFirst("UserName", "user")).Returns(default(User));

            var service = new UserService(repository.Object);

            var result = service.Authenticate("user", "password");

            result.Should().HaveErrorStatus().WithError("User not found");

            mockRepository.Verify();
        }

        [Fact]
        public void AuthenticatePasswordWrong()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var user = new User(Guid.NewGuid());
            user.Create("user", "password");

            var repository = mockRepository.Create<IRepository<User>>();
            repository.Setup(x => x.FindFirst("UserName", "user")).Returns(user);

            var service = new UserService(repository.Object);

            var result = service.Authenticate("user", "wrong");

            result.Should().HaveErrorStatus().WithError("Incorrect Password");

            mockRepository.Verify();
        }

        [Fact]
        public void AuthenticateSuccessful()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var user = new User(Guid.NewGuid());
            user.Create("user", "password");

            var repository = mockRepository.Create<IRepository<User>>();
            repository.Setup(x => x.FindFirst("UserName", "user")).Returns(user);

            var service = new UserService(repository.Object);

            var result = service.Authenticate("user", "password");

            result.Result.Should().Be(user);

            mockRepository.Verify();
        }

    }
}
