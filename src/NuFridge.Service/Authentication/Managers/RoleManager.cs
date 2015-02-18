using Microsoft.AspNet.Identity;
using NuFridge.Service.Authentication.Model;
using NuFridge.Service.Authentication.Stores;

namespace NuFridge.Service.Authentication.Managers
{
    public class RoleManager: RoleManager<ApplicationRole, string>
    {
        public RoleManager() : base(new ApplicationRoleStore())
        {
            
        }
    }
}
