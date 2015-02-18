using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using NuFridge.Service.Authentication.Model;
using NuFridge.Service.Data.Repositories;

namespace NuFridge.Service.Authentication.Stores
{
    public class ApplicationRoleStore : IRoleStore<ApplicationRole>
    {
        private NuFridgeContext _context;

        public ApplicationRoleStore()
        {
            _context = new NuFridgeContext();

        }

        public ApplicationRoleStore(NuFridgeContext database)
        {
            _context = database;
        }

        public Task CreateAsync(ApplicationRole role)
        {
            if (role == null)
            {
                throw new ArgumentNullException("RoleIsRequired");
            }

            _context.Roles.Add(role);
            return _context.SaveChangesAsync();

        }

        public Task DeleteAsync(ApplicationRole role)
        {
            var roleEntity = _context.Roles.FirstOrDefault(x => x.Id == role.Id);
            if (roleEntity == null) throw new InvalidOperationException("No such role exists!");
            _context.Roles.Remove(roleEntity);
            return _context.SaveChangesAsync();
        }

        public Task<ApplicationRole> FindByIdAsync(string roleId)
        {
            var role = _context.Roles.FirstOrDefault(x => x.Id == roleId);

            return Task.FromResult(role);
        }

        public Task<ApplicationRole> FindByNameAsync(string roleName)
        {

            var role = _context.Roles.FirstOrDefault(x => x.Name == roleName);


            return Task.FromResult(role);
        }

        public Task UpdateAsync(ApplicationRole role)
        {

            return _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
