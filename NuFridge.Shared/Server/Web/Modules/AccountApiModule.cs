using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Server.Web.Actions.AccountApi;

namespace NuFridge.Shared.Server.Web.Modules
{
    public class AccountApiModule : NancyModule
    {
        public AccountApiModule(IContainer container)
        {
            Post["api/account/register"] = p => container.Resolve<RegisterAccountAction>().Execute(p, this);
            Get["api/account/{id}"] = p => container.Resolve<GetAccountAction>().Execute(p, this);
            Get["api/signin"] = p => container.Resolve<SignInAction>().Execute(p, this);
        }
    }
}