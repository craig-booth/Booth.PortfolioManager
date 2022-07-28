using System;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace Booth.PortfolioManager.Domain.Users
{

    public class User : IEntity
    {
        public Guid Id { get; }

        public string UserName { get; private set; }

        public string Password { get; private set; }

        public bool Administrator { get; private set; }

        public User(Guid id)
        {
             Id = id;
        }

        public void Create(string userName, string password)
        {
            if (!ValidateUserName(userName))
                throw new ArgumentException("Username is not valid");

            if (!ValidatePassword(password))
                throw new ArgumentException("Password is not valid");


            UserName = userName;
            Password = HashPassword(password);
        }

        public void ChangeUserName(string newUserName)
        {
            if (!ValidateUserName(newUserName))
                throw new ArgumentException("Username is not valid");

            UserName = newUserName;
        }

        public void AddAdministratorPrivilage()
        {
            Administrator = true;
        }

        public void RemoveAdministratorPrivilage()
        {
            Administrator = false;
        }

        public void ChangePassword(string newPassword)
        {
            if (!ValidatePassword(newPassword))
                throw new ArgumentException("Password is not valid");

            var hashedPassword = HashPassword(newPassword);

            if (hashedPassword == Password)
                throw new ArgumentException("Password must not be the same as your existing password");

            Password = hashedPassword;
        }


        public bool PasswordCorrect(string password)
        {
            var hashedPassword = HashPassword(password);

            return (hashedPassword == Password); 
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
