using Autofac;
using Nancy;
using NuFridge.Shared.Server.Web.Actions.AccountApi;

namespace NuFridge.Shared.Server.Web.Modules
{
    public class AccountApiModule : NancyModule
    {
        public AccountApiModule(IContainer container)
        {
            Post["api/account/register"] = p => container.Resolve<RegisterAccountAction>().Execute(p, this);
            Get["api/account/{username?}"] = p => container.Resolve<GetAccountAction>().Execute(p, this);
            Post["api/signin"] = p => container.Resolve<SignInAction>().Execute(p, this);
        }
    }
}