using Autofac;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;

namespace NuFridge.Shared.Autofac
{
    public class ModelModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            LoadModels(builder);
        }

        protected virtual void LoadModels(ContainerBuilder builder)
        {
            builder.RegisterType<Feed>().As<IFeed>();
            builder.RegisterType<FeedConfiguration>().As<IFeedConfiguration>();
            builder.RegisterType<InternalPackage>().As<IInternalPackage>();
            builder.RegisterType<Statistic>().As<IStatistic>();
            builder.RegisterType<Framework>().As<IFramework>();
        }
    }
}