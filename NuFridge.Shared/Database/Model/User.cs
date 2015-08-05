using System;
using Dapper;
using NuFridge.Shared.Server.Security;

namespace NuFridge.Shared.Database.Model
{
    [Table("User", Schema = "NuFridge")]
    public class User : IUser
    {
        [Key]
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
    }

    public interface IUser
    {
        int Id { get; }

        string Username { get; set; }
        string DisplayName { get; set; }
        string EmailAddress { get; set; }

        string PasswordHashed { get; set; }
        bool IsActive { get; set; }
        DateTime LastUpdated { get; set; }

        void SetPassword(string plainTextPassword);
    }
}