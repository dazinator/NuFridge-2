using Autofac;
using NuFridge.Shared.Server.Web;
using NuFridge.Shared.Server.Web.Actions;
using NuFridge.Shared.Server.Web.Actions.AccountApi;
using NuFridge.Shared.Server.Web.Actions.DashboardApi;
using NuFridge.Shared.Server.Web.Actions.DiagnosticsApi;
using NuFridge.Shared.Server.Web.Actions.FeedApi;
using NuFridge.Shared.Server.Web.Actions.NuGetApi;

namespace NuFridge.Shared.Server.Modules
{
    public class WebModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterType<WebBootstrapper>().As<IPortalBootstrapper>().SingleInstance();
            builder.RegisterModule(new WebInfrastructureModule());
            builder.RegisterModule(new AuthenticationModule());
            builder.RegisterType<WebServerInitializer>().As<IWebServerInitializer>().SingleInstance();

            //Account api module
            builder.RegisterType<GetAccountAction>().AsSelf();
            builder.RegisterType<RegisterAccountAction>().AsSelf();
            builder.RegisterType<SignInAction>().AsSelf();

            //Dashboard api module
            builder.RegisterType<GetDashboardAction>().AsSelf();
            builder.RegisterType<GetFeedDownloadCountAction>().AsSelf();
            builder.RegisterType<GetFeedPackageCountAction>().AsSelf();

            //Diagnostics api module
            builder.RegisterType<GetDiagnosticInformationAction>().AsSelf();

            //Feed api module
            builder.RegisterType<DeleteFeedAction>().AsSelf();
            builder.RegisterType<FeedSearchAction>().AsSelf();
            builder.RegisterType<GetFeedAction>().AsSelf();
            builder.RegisterType<GetFeedConfigurationAction>().AsSelf();
            builder.RegisterType<GetFeedsAction>().AsSelf();
            builder.RegisterType<InsertFeedAction>().AsSelf();
            builder.RegisterType<SaveFeedConfigurationAction>().AsSelf();
            builder.RegisterType<UpdateFeedAction>().AsSelf();

            //NuGet api module
            builder.RegisterType<DeletePackageAction>().AsSelf();
            builder.RegisterType<DownloadPackageAction>().AsSelf();
            builder.RegisterType<GetODataPackagesAction>().AsSelf();
            builder.RegisterType<GetODataPackagesCountAction>().AsSelf();
            builder.RegisterType<RedirectToDownloadPackageAction>().AsSelf();
            builder.RegisterType<UploadPackageAction>().AsSelf();
            builder.RegisterType<TabCompletionPackageIdsAction>().AsSelf();
            builder.RegisterType<TabCompletionPackageVersionsAction>().AsSelf();
            builder.RegisterType<RedirectToApiV2Action>().AsSelf();
            builder.RegisterType<GetODataMetadataAction>().AsSelf();
            builder.RegisterType<GetODataRootAction>().AsSelf();
        }
    }
}