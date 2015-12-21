using Autofac;
using NuFridge.Shared.Web;
using NuFridge.Shared.Web.Actions.NuFridgeApi;
using NuFridge.Shared.Web.Actions.NuGetApiV2;
using NuFridge.Shared.Web.Actions.SymbolsApi;
using NuFridge.Shared.Web.Listeners;

namespace NuFridge.Shared.Autofac
{
    public class WebModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterType<WebBootstrapper>().As<IPortalBootstrapper>().SingleInstance();
            builder.RegisterModule(new AuthenticationModule());
            builder.RegisterType<WebServerInitializer>().As<IWebServerInitializer>().SingleInstance();
            builder.RegisterType<StartupPageListener>().As<IStartupPageListener>();
            builder.RegisterType<ShutdownPageListener>().As<IShutdownPageListener>();

            //Account api module
            builder.RegisterType<GetAccountAction>().AsSelf();
            builder.RegisterType<RegisterAccountAction>().AsSelf();
            builder.RegisterType<SignInAction>().AsSelf();

            //Dashboard api module
            builder.RegisterType<GetDashboardAction>().AsSelf();
            builder.RegisterType<GetFeedDownloadCountAction>().AsSelf();
            builder.RegisterType<GetFeedPackageCountAction>().AsSelf();

            //Feed api module
            builder.RegisterType<UpdateFeedGroupAction>().AsSelf();
            builder.RegisterType<GetFeedGroupAction>().AsSelf();
            builder.RegisterType<DeleteFeedAction>().AsSelf();
            builder.RegisterType<FeedSearchAction>().AsSelf();
            builder.RegisterType<GetFeedAction>().AsSelf();
            builder.RegisterType<UploadPackageFromUrlAction>().AsSelf();
            builder.RegisterType<GetFeedConfigurationAction>().AsSelf();
            builder.RegisterType<GetFeedsAction>().AsSelf();
            builder.RegisterType<InsertFeedAction>().AsSelf();
            builder.RegisterType<SaveFeedConfigurationAction>().AsSelf();
            builder.RegisterType<UpdateFeedAction>().AsSelf();
            builder.RegisterType<ReindexPackagesAction>().AsSelf();
            builder.RegisterType<ImportPackagesFromFeedAction>().AsSelf();
            builder.RegisterType<CancelImportPackagesFromFeedAction>().AsSelf();
            builder.RegisterType<GetPackageAuditHistoryAction>().AsSelf();
            builder.RegisterType<DownloadPackageImportReport>().AsSelf();
            builder.RegisterType<CheckIfPerformedFirstTimeSetupAction>().AsSelf();
            builder.RegisterType<SetupAction>().AsSelf();
            builder.RegisterType<UpdateAccountAction>().AsSelf();
            builder.RegisterType<GetBackgroundJobsAction>().AsSelf();
            builder.RegisterType<GetAccountsAction>().AsSelf();

            //NuGet api v2 module
            builder.RegisterType<BatchAction>().AsSelf();
            builder.RegisterType<GetUpdatesAction>().AsSelf();
            builder.RegisterType<GetUpdatesCountAction>().AsSelf();
            builder.RegisterType<DeletePackageAction>().AsSelf();
            builder.RegisterType<DownloadPackageAction>().AsSelf();
            builder.RegisterType<GetODataPackageAction>().AsSelf();
            builder.RegisterType<GetODataPackagesAction>().AsSelf();
            builder.RegisterType<GetODataPackagesCountAction>().AsSelf();
            builder.RegisterType<RedirectToDownloadPackageAction>().AsSelf();
            builder.RegisterType<RedirectToUploadPackageAction>().AsSelf();
            builder.RegisterType<UploadPackageAction>().AsSelf();
            builder.RegisterType<TabCompletionPackageIdsAction>().AsSelf();
            builder.RegisterType<TabCompletionPackageVersionsAction>().AsSelf();
            builder.RegisterType<RedirectToApiV2Action>().AsSelf();
            builder.RegisterType<GetODataMetadataAction>().AsSelf();
            builder.RegisterType<GetODataRootAction>().AsSelf();

            //Symbols module
            builder.RegisterType<UploadSymbolPackageAction>().AsSelf();
            builder.RegisterType<GetSymbolFileAction>().AsSelf();
            builder.RegisterType<GetSourceFilesAction>().AsSelf();
        }
    }
}