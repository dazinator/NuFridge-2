using Autofac;
using NuGet.Lucene.Web;
using Owin;

namespace NuFridge.Service.Feeds
{
    public class CustomStartup : Startup
    {
        readonly  NuGetFeed feed;

        public CustomStartup(NuGetFeed feed)
        {
            this.feed = feed;
        }

        protected override INuGetWebApiSettings CreateSettings()
        {
            return feed.CreateSettings();
        }

        public IContainer CreateDefaultContainer(IAppBuilder app)
        {
            return base.CreateContainer(app);
        }

        protected override IContainer CreateContainer(IAppBuilder app)
        {
            return feed.CreateContainer(app);
        }
    }
}
