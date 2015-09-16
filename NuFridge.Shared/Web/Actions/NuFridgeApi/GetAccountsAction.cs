using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using NuFridge.Shared.Database.Services;

namespace NuFridge.Shared.Web.Actions.NuFridgeApi
{
    public class GetAccountsAction : IAction
    {
        private readonly IUserService _userService;

        public GetAccountsAction(IUserService userService)
        {
            _userService = userService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            int totalResults;

            int pageNumber = module.Request.Query["page"];
            int size = module.Request.Query["size"];

            var users = _userService.GetAll(pageNumber, size, out totalResults, false);

            return module.Negotiate.WithModel(new
            {
                Users = users,
                Total = totalResults
            }).WithStatusCode(HttpStatusCode.OK);
        }
    }
}