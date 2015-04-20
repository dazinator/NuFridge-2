using Microsoft.AspNet.Identity;
using NuFridge.Service.Authentication.Stores;
using NuFridge.Service.Model;

namespace NuFridge.Service.Authentication.Managers
{
    public class RoleManager: RoleManager<ApplicationRole, string>
    {
        public RoleManager() : base(new ApplicationRoleStore())
        {
            
        }
    }
}
