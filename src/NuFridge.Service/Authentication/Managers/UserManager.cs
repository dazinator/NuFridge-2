using Microsoft.AspNet.Identity;
using NuFridge.Service.Authentication.Model;
using NuFridge.Service.Authentication.Stores;

namespace NuFridge.Service.Authentication.Managers
{
    public class UserManager : UserManager<ApplicationUser, string>
    {
        public UserManager() : base(new ApplicationUserStore())
        {
            
        }
    }
}
