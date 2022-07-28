using System;
using System.Linq;

using Xunit;
using FluentAssertions;

using Booth.PortfolioManager.Domain.Users;

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

            user.Should().BeEquivalentTo(new { UserName = "john", Password = HashedTestPassword, Administrator = false });
        }

        [Fact]
        public void AddAdministratorPrivilege()
        {
            var user = new User(Guid.NewGuid());
            user.Create("john", TestPassword);
            user.AddAdministratorPrivilage();

            user.Administrator.Should().BeTrue();
        }

        [Fact]
        public void RemoveAdministratorPrivilege()
        {
            var user = new User(Guid.NewGuid());
            user.Create("john", TestPassword);

            user.AddAdministratorPrivilage();
            user.Administrator.Should().BeTrue();

            user.RemoveAdministratorPrivilage();
            user.Administrator.Should().BeFalse();
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
            
            user.Password.Should().Be(HashedTestPassword);
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

