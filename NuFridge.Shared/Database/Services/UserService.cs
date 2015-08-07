using System;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Repository;
using SimpleCrypto;

namespace NuFridge.Shared.Database.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public int GetCount()
        {
            return _userRepository.GetCount();
        }

        public void CreateAdministratorUserIfNotExist()
        {
            var user = Find("system");

            if (user == null)
            {
                user = new User("system");
                user.IsActive = true;
                user.IsService = true;
                user.EmailAddress = "";
                user.LastUpdated = DateTime.Now;
                user.DisplayName = "System";

                var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_!£$%^&*(),.;:#";
                var stringChars = new char[30];
                var random = new Random();

                for (int i = 0; i < stringChars.Length; i++)
                {
                    stringChars[i] = chars[random.Next(chars.Length)];
                }

                user.Password = new string(stringChars); ;

                Insert(user);
            }
        }

        public User Find(string username)
        {
            return _userRepository.Find(username);
        }

        public User Find(int userId)
        {
            return _userRepository.Find(userId);
        }

        private void SetPassword(User user)
        {
            ICryptoService cryptoService = new PBKDF2();

            user.PasswordSalt = cryptoService.GenerateSalt();
            user.PasswordHashed = cryptoService.Compute(user.Password);
        }

        public void Update(User user)
        {
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                SetPassword(user);
            }
            else if (string.IsNullOrWhiteSpace(user.PasswordHashed) || string.IsNullOrWhiteSpace(user.PasswordSalt))
            {
                User existingUser = Find(user.Id);
                user.PasswordHashed = existingUser.PasswordHashed;
                user.PasswordSalt = existingUser.PasswordSalt;
            }

            _userRepository.Update(user);
        }

        public void Insert(User user)
        {
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                SetPassword(user);
            }

            _userRepository.Insert(user);
        }
    }

    public interface IUserService
    {
        void CreateAdministratorUserIfNotExist();
        User Find(string username);
        User Find(int userId);
        int GetCount();
        void Insert(User user);
        void Update(User user);
    }
}