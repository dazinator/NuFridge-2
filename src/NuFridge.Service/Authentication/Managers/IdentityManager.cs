using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NuFridge.Service.Authentication.Model;
using NuFridge.Service.Data.Repositories;

namespace NuFridge.Service.Authentication.Managers
{
    public class IdentityManager
    {
        private readonly RoleManager _roleManager = new RoleManager();

        private readonly UserManager _userManager = new UserManager();

        private NuFridgeContext _db = new NuFridgeContext(ConfigurationManager.ConnectionStrings["DefaultConnection"]);


        public bool RoleExists(string name)
        {
            return _roleManager.FindByNameAsync(name).Result != null;
        }

        private ApplicationRole GetRole(string id)
        {
            return _roleManager.FindByIdAsync(id).Result;
        }

        public bool CreateRole(string name, string description = "")
        {
            var task = _roleManager.CreateAsync(new ApplicationRole(name, description));
            task.Wait();
            return true;
        }

        public async Task<IdentityResult> RegisterUser(ApplicationUser userModel, string password)
        {
            var result = await _userManager.CreateAsync(userModel, password);

            return result;
        }

        public async Task<ApplicationUser> FindUser(string userName, string password)
        {
            ApplicationUser user = await _userManager.FindAsync(userName, password);

            return user;
        }

        public bool AddUserToRole(string userId, string roleName)
        {
            var idResult = _userManager.AddToRole(userId, roleName);
            return idResult.Succeeded;
        }


        public void ClearUserRoles(string userId)
        {
            var user = _userManager.FindById(userId);
            var currentRoles = new List<IdentityUserRole>();

            currentRoles.AddRange(user.Roles);
            foreach (var role in currentRoles)
            {
                _userManager.RemoveFromRole(userId, GetRole(role.RoleId).Name);
            }
        }

        public void CreateGroup(string groupName)
        {
            if (this.GroupNameExists(groupName))
            {
                throw new System.Exception("A group by that name already exists in the database. Please choose another name.");
            }

            var newGroup = new ApplicationGroup(groupName);
            _db.Groups.Add(newGroup);
            _db.SaveChanges();
        }


        public bool GroupNameExists(string groupName)
        {
            var g = _db.Groups.Where(gr => gr.Name == groupName);
            if (g.Count() > 0)
            {
                return true;
            }
            return false;
        }


        public void ClearUserGroups(string userId)
        {
            this.ClearUserRoles(userId);
            var user = _db.Users.Find(userId);
            user.Groups.Clear();
            _db.SaveChanges();
        }


        public void AddUserToGroup(string userId, int GroupId)
        {
            var group = _db.Groups.Find(GroupId);
            var user = _db.Users.Find(userId);

            var userGroup = new ApplicationUserGroup()
            {
                Group = group,
                GroupId = group.Id,
                User = user,
                UserId = user.Id
            };

            foreach (var role in group.Roles)
            {
                _userManager.AddToRoleAsync(userId, role.Role.Name).Wait();
            }
            
            if (!user.Groups.Any(gr => gr.GroupId == userGroup.GroupId))
            {
                user.Groups.Add(userGroup);
            }

            _db.SaveChanges();
        }


        public void ClearGroupRoles(int groupId)
        {
            var group = _db.Groups.Find(groupId);
            var groupUsers = _db.Users.Where(u => u.Groups.Any(g => g.GroupId == group.Id));

            foreach (var role in group.Roles)
            {
                var currentRoleId = role.RoleId;
                foreach (var user in groupUsers)
                {
                    // Is the user a member of any other groups with this role?
                    var groupsWithRole = user.Groups
                        .Where(g => g.Group.Roles
                            .Any(r => r.RoleId == currentRoleId)).Count();
                    // This will be 1 if the current group is the only one:
                    if (groupsWithRole == 1)
                    {
                        this.RemoveFromRole(user, role.Role);
                    }
                }
            }
            group.Roles.Clear();
            _db.SaveChanges();
        }

        private void RemoveFromRole(ApplicationUser user, ApplicationRole role)
        {
            var userRole = user.Roles.FirstOrDefault(rl => rl.RoleId == role.Id);
            if (userRole != null)
            {
                user.Roles.Remove(userRole);
                _db.SaveChanges();
            }
        }


        public void AddRoleToGroup(int groupId, string roleName)
        {
            var group = _db.Groups.Find(groupId);
            var role = _db.Roles.First(r => r.Name == roleName);
            if (!group.Roles.Any(rl => rl.GroupId == groupId && rl.RoleId == role.Id))
            {
                var newgroupRole = new ApplicationRoleGroup()
                {
                    GroupId = group.Id,
                    Group = group,
                    RoleId = role.Id,
                    Role = role
                };

                group.Roles.Add(newgroupRole);
                _db.SaveChanges();
            }

            // Add all of the users in this group to the new role:
            var groupUsers = _db.Users.Where(u => u.Groups.Any(g => g.GroupId == group.Id));
            foreach (var user in groupUsers)
            {
                if (!(_userManager.IsInRoleAsync(user.Id, roleName).Result))
                {
                    this.AddUserToRole(user.Id, role.Name);
                }
            }
        }


        public void DeleteGroup(int groupId)
        {
            var group = _db.Groups.Find(groupId);

            // Clear the roles from the group:
            this.ClearGroupRoles(groupId);
            _db.Groups.Remove(group);
            _db.SaveChanges();
        }
    }
}
