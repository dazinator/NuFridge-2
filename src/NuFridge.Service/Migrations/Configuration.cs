using System;
using System.Configuration;
using System.Data.Entity.Migrations;
using System.Linq;
using NuFridge.Service.Authentication;
using NuFridge.Service.Authentication.Managers;
using NuFridge.Service.Authentication.Model;
using NuFridge.Service.Data;
using NuFridge.Service.Data.Model;
using NuFridge.Service.Data.Repositories;

namespace NuFridge.Service.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<NuFridgeContext>
    {
        IdentityManager _idManager = new IdentityManager();
        NuFridgeContext _db = new NuFridgeContext(ConfigurationManager.ConnectionStrings["DefaultConnection"]);

        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(NuFridgeContext context)
        {
            var applicationUser = context.Users.FirstOrDefault(usr => usr.UserName == "administrator");

            if (applicationUser == null)
            {
                var authRepository = new IdentityManager();

                authRepository.RegisterUser(new ApplicationUser()
                {
                    UserName = "administrator",
                    FirstName = "Administrator",
                    LastName = "Administrator",
                }, "password").Wait();
            }

            if (!context.FeedGroups.Any() && !context.Feeds.Any())
            {
                var feedGroup = new FeedGroup
                {
                    Name = "Default",
                    Id = Guid.NewGuid().ToString()
                };
                context.FeedGroups.Add(feedGroup);

                var feed = new Feed { GroupId = feedGroup.Id, Name = "Default", Id = Guid.NewGuid().ToString() };
                context.Feeds.Add(feed);

                context.SaveChanges();
            }

            this.AddGroups();
            this.AddRoles();
            this.AddRolesToGroups();
            this.AddUsersToGroups();

        }

        string[] _initialGroupNames =
       new string[] { "Administrators", "Users" };
        public void AddGroups()
        {
            foreach (var groupName in _initialGroupNames)
            {
                if (!_idManager.GroupNameExists(groupName))
                {
                    _idManager.CreateGroup(groupName);
                }
            }
        }


        void AddRoles()
        {
            if (!_idManager.RoleExists("Admin"))
            {
                _idManager.CreateRole("Admin", "Global access");
            }

            if (!_idManager.RoleExists("CanEditUser"))
            {
                _idManager.CreateRole("CanEditUser", "Add, modify, and delete users");
            }

            if (!_idManager.RoleExists("CanEditGroup"))
            {
                _idManager.CreateRole("CanEditGroup", "Add, modify, and delete groups");
            }

            if (!_idManager.RoleExists("CanEditRole"))
            {
                _idManager.CreateRole("CanEditRole", "Add, modify, and delete roles");
            }

            if (!_idManager.RoleExists("CanEditFeed"))
            {
                _idManager.CreateRole("CanEditFeed", "Add, modify, and delete feeds");
            }
        }


        string[] _adminRoleNames = new string[] { "Admin", "CanEditUser", "CanEditGroup", "CanEditRole", "CanEditFeed" };
        string[] _userRoleNames = new string[] { "CanEditFeed" };
        void AddRolesToGroups()
        {
            var allGroups = _db.Groups;
            var superAdmins = allGroups.First(g => g.Name == "Administrators");
            foreach (string name in _adminRoleNames)
            {
                _idManager.AddRoleToGroup(superAdmins.Id, name);
            }

            var users = _db.Groups.First(g => g.Name == "Users");
            foreach (string name in _userRoleNames)
            {
                _idManager.AddRoleToGroup(users.Id, name);
            }
        }

        void AddUsersToGroups()
        {
            var user = _db.Users.First(u => u.UserName == "administrator");
            var allGroups = _db.Groups;
            foreach (var group in allGroups)
            {
                _idManager.AddUserToGroup(user.Id, group.Id);
            }
        }
    }
}