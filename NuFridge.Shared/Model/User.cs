using System;
using System.Security.Cryptography;
using System.Text;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Security;

namespace NuFridge.Shared.Model
{
    public class User : IEntity
    {
        public const string GuestUserId = "users-guest";
        public const string GuestLogin = "guest";

        public int Id { get; protected set; }

        public string Username { get; set; }

        public string DisplayName { get; set; }

        public string EmailAddress { get; set; }

        public string Notes { get; set; }

        public string PasswordHashed { get; set; }

        public bool IsActive { get; set; }

        public bool IsService { get; set; }

        public string ApiKey { get; set; }

        public Guid IdentificationToken { get; set; }

        public User()
            : this(null)
        {
        }

        public User(string username)
        {
            Username = username;
            IsActive = true;
            IdentificationToken = Guid.NewGuid();
        }

        public void SetPassword(string plainTextPassword)
        {
            PasswordHashed = PasswordHasher.HashPassword(plainTextPassword);
        }

        public bool ValidatePassword(string plainTextPassword)
        {
            if (PasswordHasher.VerifyPassword(plainTextPassword, PasswordHashed))
                return true;
            if (PasswordHashed != HashUsingLegacyApproach(plainTextPassword))
                return false;
            PasswordHashed = PasswordHasher.HashPassword(plainTextPassword);
            return true;
        }

        private string HashUsingLegacyApproach(string plainTextPassword)
        {
            return Convert.ToBase64String(new SHA1Managed().ComputeHash(Encoding.Default.GetBytes(Username + plainTextPassword)));
        }


        public string Name
        {
            get { return Username; }
        }
    }
}
