using Nancy;
using Nancy.Security;
using NuFridge.Shared.Server.NuGet;

namespace NuFridge.Shared.Server.Web.Actions.NuFridgeApi
{
    public class DeleteFeedAction : IAction
    {
        private readonly IFeedManager _feedManager;

        public DeleteFeedAction(IFeedManager feedManager)
        {
            _feedManager = feedManager;
        }


        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            int feedId = int.Parse(parameters.id);

            if (!_feedManager.Exists(feedId))
            {
                return module.Negotiate.WithStatusCode(HttpStatusCode.NotFound);
            }

            _feedManager.Delete(feedId);

            return module.Negotiate.WithModel(new object());
        }
    }
}
