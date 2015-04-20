using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using NuFridge.Service.Model;
using NuFridge.Service.Repositories;

namespace NuFridge.Service.Authentication.Stores
{
    public class ApplicationUserStore : IUserEmailStore<ApplicationUser, string>, IUserRoleStore<ApplicationUser, string>, IUserPasswordStore<ApplicationUser, string>
    {
        private NuFridgeContext _context;
        private ApplicationRoleStore _roleManager = new ApplicationRoleStore();

        public ApplicationUserStore()
        {
            _context = new NuFridgeContext(ConfigurationManager.ConnectionStrings["DefaultConnection"]);

        }

        public ApplicationUserStore(NuFridgeContext database)
        {
            _context = database;
        }

        public Task CreateAsync(ApplicationUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("UserIsRequired");
            }

            _context.Users.Add(user);
            return _context.SaveChangesAsync();

        }

        public Task DeleteAsync(ApplicationUser user)
        {
            var userEntity = _context.Users.FirstOrDefault(x => x.Id == user.Id);
            if (userEntity == null) throw new InvalidOperationException("No such user exists!");
            _context.Users.Remove(userEntity);
            return _context.SaveChangesAsync();
        }

        public Task<ApplicationUser> FindByIdAsync(string userId)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == userId);

            return Task.FromResult(user);
        }

        public Task<ApplicationUser> FindByNameAsync(string userName)
        {

            var user = _context.Users.FirstOrDefault(x => x.UserName == userName);


            return Task.FromResult(user);
        }

        public Task UpdateAsync(ApplicationUser role)
        {

            return _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public Task<ApplicationUser> FindByEmailAsync(string email)
        {
            return Task.FromResult(_context.Users.FirstOrDefault(us => us.Email == email));
        }

        public Task<string> GetEmailAsync(ApplicationUser user)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(ApplicationUser user)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task SetEmailAsync(ApplicationUser user, string email)
        {
            user.Email = email;

            return Task.FromResult(_context.SaveChangesAsync());
        }

        public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed)
        {
            user.EmailConfirmed = confirmed;

            return Task.FromResult(_context.SaveChangesAsync());
        }

        public Task AddToRoleAsync(ApplicationUser user, string roleName)
        {
            if (!IsInRoleAsync(user, roleName).Result)
            {
                var role = _roleManager.FindByNameAsync(roleName).Result;
                if (role != null)
                {
                    user.Roles.Add(new Microsoft.AspNet.Identity.EntityFramework.IdentityUserRole()
                    {
                        RoleId = role.Id,
                        UserId = user.Id
                    });

                    return _context.SaveChangesAsync();
                }
            }

            return Task.FromResult(false);
        }

        public Task<IList<string>> GetRolesAsync(ApplicationUser user)
        {
            IList<string> roles = (from usrRole in user.Roles select _roleManager.FindByIdAsync(usrRole.RoleId).Result into role where role != null select role.Name).ToList();
            return Task.FromResult(roles);
        }

        public Task<bool> IsInRoleAsync(ApplicationUser user, string roleName)
        {
            var found = false;
            var role = _roleManager.FindByNameAsync(roleName).Result;
            if (role == null) return Task.FromResult(false);

            if (user.Roles.Any(rl => rl.RoleId == role.Id))
            {
                found = true;
            }

            return Task.FromResult(found);
        }

        public Task RemoveFromRoleAsync(ApplicationUser user, string roleName)
        {
            var role = _roleManager.FindByNameAsync(roleName).Result;
            if (role == null) return Task.FromResult(false);

            var usrRole = user.Roles.FirstOrDefault(rl => rl.RoleId == role.Id);
            if (usrRole != null)
            {
                user.Roles.Remove(usrRole);
                return _context.SaveChangesAsync();
            }

            return Task.FromResult(false);
        }

        public Task<string> GetPasswordHashAsync(ApplicationUser user)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(ApplicationUser user)
        {
            return Task.FromResult(true);
        }

        public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash)
        {
            user.PasswordHash = passwordHash;

            return Task.FromResult(_context.SaveChangesAsync());
        }
    }
}
