using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Repository;
using NuFridge.Shared.Security;
using NuFridge.Shared.Web;
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
            return _userRepository.GetCount(true);
        }

        public void CreateSystemUserIfNotExist()
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
                var stringChars = new char[50];
                var random = new Random();

                for (int i = 0; i < stringChars.Length; i++)
                {
                    stringChars[i] = chars[random.Next(chars.Length)];
                }

                user.Password = new string(stringChars); ;
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

            if (!user.IsActive || user.IsService)
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

            return new LocalUserIdentity
            {
                UserName = user.Username,
                Claims = new List<string>
                {
                    Claims.SystemAdministrator,
                    Claims.CanInsertFeed
                }
            };
        }

        public int GetLoggedInUserId(INancyModule module)
        {
            if (!string.IsNullOrWhiteSpace(module.Context.CurrentUser?.UserName))
            {
                var user = Find(module.Context.CurrentUser.UserName, false);
                if (user != null)
                {
                    return user.Id;
                }
            }

            return 0;
        }

        public IEnumerable<User> GetAll(int pageNumber, int rows, out int totalResults, bool includePassword)
        {
            var users = _userRepository.GetAll(pageNumber, rows, out totalResults);

            if (!includePassword)
            {
                foreach (var user in users)
                {
                    ConfigureUserEntity(user, false);
                }
            }

            return users;
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
        void CreateSystemUserIfNotExist();
        User Find(string username, bool includePassword);
        User Find(int userId, bool includePassword);
        int GetCount();
        void Insert(User user);
        void Update(User user);
        IUserIdentity ValidateSignInRequest(SignInRequest signInRequest);
        int GetLoggedInUserId(INancyModule module);
        IEnumerable<User> GetAll(int pageNumber, int rows, out int totalResults, bool includePassword);
    }
}