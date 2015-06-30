using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.Security;

namespace NuFridge.Shared.Server.Web.Actions.AccountApi
{
    public class GetAccountAction : IAction
    {
        public dynamic Execute(dynamic parameters, global::Nancy.INancyModule module)
        {
            module.RequiresAuthentication();

            return null;
        }
    }
}