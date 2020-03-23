using System;
using System.Linq;

using NUnit.Framework;

using Booth.EventStore;
using Booth.PortfolioManager.Domain.Users;
using Booth.PortfolioManager.Domain.Users.Events;

namespace Booth.PortfolioManager.Domain.Test.Users
{
    class UserTests
    {
        const string TestPassword = "The quick brown fox jumps over the lazy dog";
        const string HashedTestPassword = "3b5b0eac46c8f0c16fa1b9c187abc8379cc936f6508892969d49234c6c540e58";


        [TestCase]
        public void CreateUserWithBlankUserName()
        {
            var user = new User(Guid.NewGuid());
            Assert.That(() => user.Create("", "secret"), Throws.TypeOf(typeof(ArgumentException)));
        }

        [TestCase]
        public void CreateUserWithBlankPassword()
        {
            var user = new User(Guid.NewGuid());
            Assert.That(() => user.Create("john", ""), Throws.TypeOf(typeof(ArgumentException)));
        }

        [TestCase]
        public void CreateUserSuccessfully()
        {
            var user = new User(Guid.NewGuid());
            user.Create("john", TestPassword);

            var events = user.FetchEvents().ToList();

            Assert.Multiple(() => {
                Assert.That(events, Has.Count.EqualTo(1));
                var e1 = events[0] as UserCreatedEvent;
                Assert.That(e1.UserName, Is.EqualTo("john"));
                Assert.That(e1.Password, Is.EqualTo(HashedTestPassword));
            });
        }

        [TestCase]
        public void ApplyUserCreatedEvent()
        {
            var user = new User(Guid.NewGuid());

            var @event = new UserCreatedEvent(user.Id, 0, "john", "secret");
            user.ApplyEvents(new Event[] { @event });

            Assert.That(user.UserName, Is.EqualTo("john"));
        }

        [TestCase]
        public void GetProperties()
        {
            var user = new User(Guid.NewGuid());
            user.Create("john", "secret");

            var properties = user.GetProperties();

            Assert.That(properties["UserName"], Is.EqualTo("john"));
        }


        [TestCase]
        public void ChangeUserNameToBlank()
        {
            var user = new User(Guid.NewGuid());
            user.Create("john", "secret");

            Assert.That(() => user.ChangeUserName(""), Throws.TypeOf(typeof(ArgumentException)));
        }

        [TestCase]
        public void ChangeUserNameSuccessfully()
        {
            var user = new User(Guid.NewGuid());
            user.Create("john", "secret");
            user.ChangeUserName("bill");
            var events = user.FetchEvents().ToList();

            Assert.Multiple(() => {
                Assert.That(events, Has.Count.EqualTo(2));

                var e2 = events[1] as UserNameChangedEvent;
                Assert.That(e2.UserName, Is.EqualTo("bill"));
            });
        }

        [TestCase]
        public void ApplyChangeUserNameEvent()
        {
            var user = new User(Guid.NewGuid());

            var @event = new UserNameChangedEvent(user.Id, 0, "bill");
            user.ApplyEvents(new Event[] { @event });

            Assert.That(user.UserName, Is.EqualTo("bill"));
        }

        [TestCase]
        public void ChangePasswordToBlank()
        {
            var user = new User(Guid.NewGuid());
            user.Create("john", "secret");

            Assert.That(() => user.ChangePassword(""), Throws.TypeOf(typeof(ArgumentException)));
        }

        [TestCase]
        public void ChangePasswordSameValue()
        {
            var user = new User(Guid.NewGuid());
            user.Create("john", "secret");

            Assert.That(() => user.ChangePassword("secret"), Throws.TypeOf(typeof(ArgumentException)));
        }

        [TestCase]
        public void ChangePasswordSuccessfully()
        {
            var user = new User(Guid.NewGuid());
            user.Create("john", "secret");
            user.ChangePassword(TestPassword);
            var events = user.FetchEvents().ToList();

            Assert.Multiple(() => {
                Assert.That(events, Has.Count.EqualTo(2));

                var e2 = events[1] as PasswordChangedEvent;
                Assert.That(e2.Password, Is.EqualTo(HashedTestPassword)); 
            });
        }

        [TestCase]
        public void ApplyPasswordChangedEvent()
        {
            var user = new User(Guid.NewGuid());

            var @event = new PasswordChangedEvent(user.Id, 0, HashedTestPassword);
            user.ApplyEvents(new Event[] { @event });

            Assert.That(user.PasswordCorrect(TestPassword), Is.True);
        }

        [TestCase]
        public void CheckValidPassword()
        {
            var user = new User(Guid.NewGuid());
            user.Create("john", "secret");

            Assert.That(user.PasswordCorrect("secret"), Is.True);
        }

        [TestCase]
        public void CheckInValidPassword()
        {
            var user = new User(Guid.NewGuid());
            user.Create("john", "secret");

            Assert.That(user.PasswordCorrect("wrong"), Is.False);
        }
    }
}

