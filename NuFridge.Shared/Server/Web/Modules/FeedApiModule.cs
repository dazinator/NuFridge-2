using Autofac;
using Nancy;
using NuFridge.Shared.Server.Web.Actions.FeedApi;

namespace NuFridge.Shared.Server.Web.Modules
{
    public class FeedApiModule : NancyModule
    {
        public FeedApiModule(IContainer container)
        {
            Get["api/feeds"] = p => container.Resolve<GetFeedsAction>().Execute(p, this);
            Get["api/feeds/search"] = p => container.Resolve<FeedSearchAction>().Execute(p, this);
            Get["api/feeds/{id}"] = p => container.Resolve<GetFeedAction>().Execute(p, this);
            Get["api/feeds/{id}/config"] = p => container.Resolve<GetFeedConfigurationAction>().Execute(p, this);
            Put["api/feeds/{id}/config"] = p => container.Resolve<SaveFeedConfigurationAction>().Execute(p, this);
            Post["api/feeds"] = p => container.Resolve<InsertFeedAction>().Execute(p, this);
            Put["api/feeds/{id}"] = p => container.Resolve<UpdateFeedAction>().Execute(p, this);
            Delete["api/feeds/{id}"] = p => container.Resolve<DeleteFeedAction>().Execute(p, this);
            Post["api/feeds/{id}/upload"] = p => container.Resolve<UploadPackageFromUrlAction>().Execute(p, this);
            Post["api/feeds/{id}/reindex"] = p => container.Resolve<ReindexPackagesAction>().Execute(p, this);
            Post["api/feeds/{id}/import"] = p => container.Resolve<ImportPackagesFromFeedAction>().Execute(p, this);
            Get["api/feeds/{id}/audit"] = p => container.Resolve<GetPackageAuditHistoryAction>().Execute(p, this);
        }
    }
}