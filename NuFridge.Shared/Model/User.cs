using System;
using System.Security.Cryptography;
using System.Text;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Security;

namespace NuFridge.Shared.Model
{
    public class User : IEntity
    {
        public int Id { get; protected set; }

        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }

        public string PasswordHashed { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastUpdated { get; set; }


        public User()
            : this(null)
        {
        }

        public User(string username)
        {
            Username = username;
            IsActive = true;
        }

        public void SetPassword(string plainTextPassword)
        {
            PasswordHashed = PasswordHasher.HashPassword(plainTextPassword);
        }

        public bool ValidatePassword(string plainTextPassword)
        {
            if (PasswordHasher.VerifyPassword(plainTextPassword, PasswordHashed))
                return true;

            PasswordHashed = PasswordHasher.HashPassword(plainTextPassword);
            return true;
        }

        public string Name
        {
            get { return DisplayName ?? Username; }
        }
    }
}
