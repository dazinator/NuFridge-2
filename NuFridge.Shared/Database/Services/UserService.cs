using System;
using System.Collections.Generic;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Repository;
using NuFridge.Shared.Server.Web;
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
            var user = Find("system", false);

            if (user == null)
            {
                user = new User("system");
                user.IsActive = true;
                user.IsService = true;
                user.EmailAddress = "";
                user.LastUpdated = DateTime.UtcNow;
                user.DisplayName = "System";

                var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_!£$%^&*(),.;:#";
                var stringChars = new char[30];
                var random = new Random();

                for (int i = 0; i < stringChars.Length; i++)
                {
                    stringChars[i] = chars[random.Next(chars.Length)];
                }

                user.Password = new string(stringChars); ;
                user.Password = "abcd1";
                Insert(user);
            }
        }

        private void ConfigureUserEntity(User user, bool includePassword)
        {
            if (!includePassword)
            {
                user.PasswordSalt = null;
                user.PasswordHashed = null;
            }
        }

        public User Find(string username, bool includePassword)
        {
            var user =  _userRepository.Find(username);
            if (user != null)
            {
                ConfigureUserEntity(user, includePassword);
            }

            return user;
        }

        public User Find(int userId, bool includePassword)
        {
            var user = _userRepository.Find(userId);
            if (user != null)
            {
                ConfigureUserEntity(user, includePassword);
            }

            return user;
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
                User existingUser = Find(user.Id, true);
                user.PasswordHashed = existingUser.PasswordHashed;
                user.PasswordSalt = existingUser.PasswordSalt;
            }

            _userRepository.Update(user);
        }

        public IUserIdentity ValidateSignInRequest(SignInRequest signInRequest)
        {
            string username = signInRequest.Email;

            var user = _userRepository.Find(username);

            if (user == null)
            {
                return null;
            }

            ICryptoService cryptoService = new PBKDF2();

            string signInAttemptPassword = cryptoService.Compute(signInRequest.Password, user.PasswordSalt);
            bool isPasswordCorrect = cryptoService.Compare(signInAttemptPassword, user.PasswordHashed);

            if (!isPasswordCorrect)
            {
                return null;
            }

            return new TemporaryAdminUserIdentity { UserName = user.Username, Claims = new List<string>() };
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
        User Find(string username, bool includePassword);
        User Find(int userId, bool includePassword);
        int GetCount();
        void Insert(User user);
        void Update(User user);
        IUserIdentity ValidateSignInRequest(SignInRequest signInRequest);
    }
}