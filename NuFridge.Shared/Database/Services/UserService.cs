using System;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Repository;

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
            var totalCount = GetCount();

            if (totalCount == 0)
            {
                User user = new User("administrator");
                user.IsActive = true;
                user.EmailAddress = "admin@nufridge.com";
                user.LastUpdated = DateTime.Now;
                user.SetPassword("password");
                user.DisplayName = "Administrator";

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

        public void Insert(User user)
        {
            _userRepository.Insert(user);
        }
    }

    public interface IUserService
    {
        void CreateAdministratorUserIfNotExist();
        User Find(string username);
        User Find(int userId);
        int GetCount();
    }
}