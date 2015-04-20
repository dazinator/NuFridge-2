using Microsoft.AspNet.Identity;
using NuFridge.Service.Authentication.Stores;
using NuFridge.Service.Model;

namespace NuFridge.Service.Authentication.Managers
{
    public class UserManager : UserManager<ApplicationUser, string>
    {
        public UserManager() : base(new ApplicationUserStore())
        {
            
        }
    }
}
