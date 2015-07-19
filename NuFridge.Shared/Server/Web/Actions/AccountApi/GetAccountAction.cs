using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.Security;
using NuFridge.Shared.Model;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.AccountApi
{
    public class GetAccountAction : IAction
    {
        private readonly IStore _store;

        public GetAccountAction(IStore store)
        {
            _store = store;
        }

        public dynamic Execute(dynamic parameters, global::Nancy.INancyModule module)
        {
            string username = parameters.username;

            module.RequiresAuthentication();

            if (!string.IsNullOrWhiteSpace(username))
            {
                //TODO once roles are in place
                if (module.Context.CurrentUser.UserName.ToLower() == "administrator")
                {
                    return GetUser(username);
                }
            }
            else
            {
                return GetUser(module.Context.CurrentUser.UserName);
            }

            return null;
        }

        private User GetUser(string username)
        {
            using (ITransaction transaction = _store.BeginTransaction())
            {
                var user = transaction.Query<User>().Where("username = @username").Parameter("username", username).First();

                user.PasswordHashed = null;

                return user;
            }
        }
    }
}