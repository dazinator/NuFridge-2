using System.Collections.Generic;
using Nancy.Security;

namespace NuFridge.Shared.Server.Web
{
    public class LocalUserIdentity : IUserIdentity
    {
        public string UserName { get; set; }

        public IEnumerable<string> Claims { get; set; }
    }
}