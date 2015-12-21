using Autofac;
using Nancy;
using NuFridge.Shared.Web.Actions.NuFridgeApi;

namespace NuFridge.Shared.Web.Modules
{
    public class NuFridgeApiModule : NancyModule
    {
        public NuFridgeApiModule(IContainer container)
        {
            Get["api/feeds"] = p => container.Resolve<GetFeedsAction>().Execute(p, this);
            Get["api/feeds/search"] = p => container.Resolve<FeedSearchAction>().Execute(p, this);
            Get["api/feeds/{id}"] = p => container.Resolve<GetFeedAction>().Execute(p, this);
            Get["api/feedgroups/{id}"] = p => container.Resolve<GetFeedGroupAction>().Execute(p, this);
            Put["api/feedgroups/{id}"] = p => container.Resolve<UpdateFeedGroupAction>().Execute(p, this);
            Post["api/feedgroups"] = p => container.Resolve<InsertFeedGroupAction>().Execute(p, this);
            Get["api/feeds/{id}/config"] = p => container.Resolve<GetFeedConfigurationAction>().Execute(p, this);
            Put["api/feeds/{id}/config"] = p => container.Resolve<SaveFeedConfigurationAction>().Execute(p, this);
            Post["api/feeds"] = p => container.Resolve<InsertFeedAction>().Execute(p, this);
            Put["api/feeds/{id}"] = p => container.Resolve<UpdateFeedAction>().Execute(p, this);
            Delete["api/feeds/{id}"] = p => container.Resolve<DeleteFeedAction>().Execute(p, this);
            Post["api/feeds/{id}/upload"] = p => container.Resolve<UploadPackageFromUrlAction>().Execute(p, this);
            Post["api/feeds/{id}/reindex"] = p => container.Resolve<ReindexPackagesAction>().Execute(p, this);
            Get["api/feeds/{id}/import/{jobid:int}/report"] = p => container.Resolve<DownloadPackageImportReport>().Execute(p, this);
            Post["api/feeds/{id}/import"] = p => container.Resolve<ImportPackagesFromFeedAction>().Execute(p, this);
            Delete["api/feeds/{id}/import/{jobid:int}"] = p => container.Resolve<CancelImportPackagesFromFeedAction>().Execute(p, this);
            Get["api/feeds/{id}/history"] = p => container.Resolve<GetPackageAuditHistoryAction>().Execute(p, this);
            Get["api/feeds/{id}/jobs"] = p => container.Resolve<GetBackgroundJobsAction>().Execute(p, this);
            Get["api/jobs"] = p => container.Resolve<GetBackgroundJobsAction>().Execute(p, this);
            Post["api/account/register"] = p => container.Resolve<RegisterAccountAction>().Execute(p, this);
            Get["api/account/{userid?}"] = p => container.Resolve<GetAccountAction>().Execute(p, this);
            Get["api/accounts"] = p => container.Resolve<GetAccountsAction>().Execute(p, this);
            Post["api/account/{userid:int}"] = p => container.Resolve<UpdateAccountAction>().Execute(p, this);
            Post["api/signin"] = p => container.Resolve<SignInAction>().Execute(p, this);

            Get["api/dashboard"] = p => container.Resolve<GetDashboardAction>().Execute(p, this);
            Get["api/stats/feedpackagecount"] = p => container.Resolve<GetFeedPackageCountAction>().Execute(p, this);
            Get["api/stats/feeddownloadcount"] = p => container.Resolve<GetFeedDownloadCountAction>().Execute(p, this);

            Get["api/setup"] = p => container.Resolve<CheckIfPerformedFirstTimeSetupAction>().Execute(p, this);
            Post["api/setup"] = p => container.Resolve<SetupAction>().Execute(p, this);
        }
    }
}