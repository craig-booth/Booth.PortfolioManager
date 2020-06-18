using System;
using System.Linq;

using Xunit;
using FluentAssertions;

using Booth.EventStore;
using Booth.PortfolioManager.Domain.Users;
using Booth.PortfolioManager.Domain.Users.Events;

namespace Booth.PortfolioManager.Domain.Test.Users
{
    public class UserTests
    {
        const string TestPassword = "The quick brown fox jumps over the lazy dog";
        const string HashedTestPassword = "3b5b0eac46c8f0c16fa1b9c187abc8379cc936f6508892969d49234c6c540e58";


        [Fact]
        public void CreateUserWithBlankUserName()
        {
            var user = new User(Guid.NewGuid());
            Action a = () => user.Create("", "secret");

            a.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void CreateUserWithBlankPassword()
        {
            var user = new User(Guid.NewGuid());
            Action a = () => user.Create("john", "");

            a.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void CreateUserSuccessfully()
        {
            var user = new User(Guid.NewGuid());
            user.Create("john", TestPassword);

            var events = user.FetchEvents().ToList();

            events.Should().SatisfyRespectively(
                event1 => event1.Should().BeOfType<UserCreatedEvent>().Which.Should().BeEquivalentTo(new { UserName = "john", Password = HashedTestPassword })
            );
        }

        [Fact]
        public void ApplyUserCreatedEvent()
        {
            var user = new User(Guid.NewGuid());

            var @event = new UserCreatedEvent(user.Id, 0, "john", "secret");
            user.ApplyEvents(new Event[] { @event });

            user.UserName.Should().Be("john");
        }

        [Fact]
        public void GetProperties()
        {
            var user = new User(Guid.NewGuid());
            user.Create("john", "secret");

            var properties = user.GetProperties();

            properties["UserName"].Should().Be("john");
        }


        [Fact]
        public void ChangeUserNameToBlank()
        {
            var user = new User(Guid.NewGuid());
            user.Create("john", "secret");

            Action a = () => user.ChangeUserName("");

            a.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ChangeUserNameSuccessfully()
        {
            var user = new User(Guid.NewGuid());
            user.Create("john", "secret");
            user.ChangeUserName("bill");
            var events = user.FetchEvents().ToList();

            events.Should().SatisfyRespectively(
                event1 => event1.Should().BeOfType<UserCreatedEvent>(),
                event2 => event2.Should().BeOfType<UserNameChangedEvent>().Which.Should().BeEquivalentTo(new { UserName = "bill" })
            );
        }

        [Fact]
        public void ApplyChangeUserNameEvent()
        {
            var user = new User(Guid.NewGuid());

            var @event = new UserNameChangedEvent(user.Id, 0, "bill");
            user.ApplyEvents(new Event[] { @event });

            user.UserName.Should().Be("bill");
        }

        [Fact]
        public void ChangePasswordToBlank()
        {
            var user = new User(Guid.NewGuid());
            user.Create("john", "secret");

            Action a = () => user.ChangePassword("");

            a.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ChangePasswordSameValue()
        {
            var user = new User(Guid.NewGuid());
            user.Create("john", "secret");

            Action a = () => user.ChangePassword("secret");

            a.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ChangePasswordSuccessfully()
        {
            var user = new User(Guid.NewGuid());
            user.Create("john", "secret");
            user.ChangePassword(TestPassword);
            var events = user.FetchEvents().ToList();

            events.Should().SatisfyRespectively(
                event1 => event1.Should().BeOfType<UserCreatedEvent>(),
                event2 => event2.Should().BeOfType<PasswordChangedEvent>().Which.Should().BeEquivalentTo(new { Password = HashedTestPassword })
            );
        }

        [Fact]
        public void ApplyPasswordChangedEvent()
        {
            var user = new User(Guid.NewGuid());

            var @event = new PasswordChangedEvent(user.Id, 0, HashedTestPassword);
            user.ApplyEvents(new Event[] { @event });

            user.PasswordCorrect(TestPassword).Should().BeTrue();
        }

        [Fact]
        public void CheckValidPassword()
        {
            var user = new User(Guid.NewGuid());
            user.Create("john", "secret");

            user.PasswordCorrect("secret").Should().BeTrue();
        }

        [Fact]
        public void CheckInValidPassword()
        {
            var user = new User(Guid.NewGuid());
            user.Create("john", "secret");

            user.PasswordCorrect("wrong").Should().BeFalse();
        }
    }
}

