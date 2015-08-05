using System.Collections.Generic;
using Nancy.Security;

namespace NuFridge.Shared.Server.Web
{
    public class TemporaryAdminUserIdentity : IUserIdentity
    {
        public string UserName { get; set; }

        public IEnumerable<string> Claims { get; set; }
    }
}