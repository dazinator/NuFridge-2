using System.Linq;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
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

        public dynamic Execute(dynamic parameters, INancyModule module)
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

            return new Response {StatusCode = HttpStatusCode.Unauthorized};
        }

        private User GetUser(string username)
        {
            using (var dbContext = new DatabaseContext())
            {
                var user = dbContext.Users.FirstOrDefault(usr => usr.Username == username);

                user.PasswordHashed = null;

                return user;
            }
        }
    }
}