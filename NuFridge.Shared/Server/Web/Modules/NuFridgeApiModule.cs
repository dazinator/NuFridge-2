using Autofac;
using Nancy;
using NuFridge.Shared.Server.Web.Actions.NuFridgeApi;

namespace NuFridge.Shared.Server.Web.Modules
{
    public class NuFridgeApiModule : NancyModule
    {
        public NuFridgeApiModule(IContainer container)
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

            Post["api/account/register"] = p => container.Resolve<RegisterAccountAction>().Execute(p, this);
            Get["api/account/{username?}"] = p => container.Resolve<GetAccountAction>().Execute(p, this);
            Post["api/signin"] = p => container.Resolve<SignInAction>().Execute(p, this);

            Get["api/dashboard"] = p => container.Resolve<GetDashboardAction>().Execute(p, this);
            Get["api/stats/feedpackagecount"] = p => container.Resolve<GetFeedPackageCountAction>().Execute(p, this);
            Get["api/stats/feeddownloadcount"] = p => container.Resolve<GetFeedDownloadCountAction>().Execute(p, this);

            Get["api/setup"] = p => container.Resolve<CheckIfPerformedFirstTimeSetupAction>().Execute(p, this);
            Post["api/setup"] = p => container.Resolve<SetupAction>().Execute(p, this);
        }
    }
}