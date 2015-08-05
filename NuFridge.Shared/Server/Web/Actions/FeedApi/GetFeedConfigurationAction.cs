using System.Linq;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.FeedApi
{
    public class GetFeedConfigurationAction : IAction
    {
        private readonly IStore _store;

        public GetFeedConfigurationAction(IStore store)
        {
            _store = store;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            using (var dbContext = new DatabaseContext())
            {
                int feedId = int.Parse(parameters.id);

                return dbContext.FeedConfigurations.AsNoTracking().FirstOrDefault(fc => fc.FeedId == feedId);
            }
        }
    }
}