using System;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;

using Booth.EventStore;
using Booth.PortfolioManager.Domain.Users.Events;

namespace Booth.PortfolioManager.Domain.Users
{

    public class User : TrackedEntity, IEntityProperties
    {
        public string UserName { get; private set; }
        private string _Password;
        public bool Administator { get; private set; }

        private readonly Dictionary<string, string> _Properties = new Dictionary<string, string>();

        public User(Guid id)
            : base(id)
        {
        }

        public void Create(string userName, string password)
        {
            if (!ValidateUserName(userName))
                throw new ArgumentException("Username is not valid");

            if (!ValidatePassword(password))
                throw new ArgumentException("Password is not valid");

            var hashedPassword = HashPassword(password);
            var @event = new UserCreatedEvent(Id, Version, userName, hashedPassword);

            Apply(@event);
            PublishEvent(@event);
        }

        public IDictionary<string, string> GetProperties()
        {
            return _Properties;
        }

        public void Apply(UserCreatedEvent @event)
        {
            Version++;

            UserName = @event.UserName;
            _Password = @event.Password;

            _Properties["UserName"] = UserName;
        }

        public void ChangeUserName(string newUserName)
        {
            if (!ValidateUserName(newUserName))
                throw new ArgumentException("Username is not valid");

            var @event = new UserNameChangedEvent(Id, Version, newUserName);

            Apply(@event);
            PublishEvent(@event);
        }

        public void Apply(UserNameChangedEvent @event)
        {
            Version++;

            UserName = @event.UserName;
            _Properties["UserName"] = UserName;
        }

        public void AddAdministratorPrivilage()
        { 
            var @event = new UserAdministratorChangedEvent(Id, Version, true);

            Apply(@event);
            PublishEvent(@event);
        }

        public void RemoveAdministratorPrivilage()
        {
            var @event = new UserAdministratorChangedEvent(Id, Version, false);

            Apply(@event);
            PublishEvent(@event);
        }

        public void Apply(UserAdministratorChangedEvent @event)
        {
            Version++;

            Administator = @event.Administrator;
        }

        public void ChangePassword(string newPassword)
        {
            if (!ValidatePassword(newPassword))
                throw new ArgumentException("Password is not valid");

            var hashedPassword = HashPassword(newPassword);

            if (hashedPassword == _Password)
                throw new ArgumentException("Password must not be the same as your existing password");

            var @event = new PasswordChangedEvent(Id, Version, hashedPassword);

            Apply(@event);
            PublishEvent(@event);
        }

        public void Apply(PasswordChangedEvent @event)
        {
            Version++;

            _Password = @event.Password;
        }

        public bool PasswordCorrect(string password)
        {
            var hashedPassword = HashPassword(password);

            return (hashedPassword == _Password);
        }

        private bool ValidateUserName(string userName)
        {
            if (userName == "")
                return false;

            return true;
        }

        private bool ValidatePassword(string password)
        {
            if (password == "")
                return false;

            return true;
        }

        private string HashPassword(string password)
        {
            using (var hashFunction = SHA256.Create())
            {
                var bytes = Encoding.Unicode.GetBytes(password);
                var hash = hashFunction.ComputeHash(bytes, 0, bytes.Length);

                // Convert to hex
                var stringBuilder = new StringBuilder();
                foreach (var b in hash)
                {
                    var hex = b.ToString("x2");
                    stringBuilder.Append(hex);
                }
                var encodedPassword = stringBuilder.ToString();

                return encodedPassword;
            }
        }
    }
}
