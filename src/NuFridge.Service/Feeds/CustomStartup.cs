using Autofac;
using NuFridge.Service.Feeds.NuGet.Lucene.Web;
using NuFridge.Service.Model;
using NuGet.Lucene.Web;
using Owin;

namespace NuFridge.Service.Feeds
{
    public class CustomStartup : Startup
    {
        private Feed _feed;
        public  IContainer Container;

        public CustomStartup(Feed feed)
        {
            _feed = feed;
        }

        protected override INuGetWebApiSettings CreateSettings()
        {
            var settings = new NuGetFeedSettings(_feed);

            _feed = null;

            return settings;
        }

        public IContainer CreateDefaultContainer(IAppBuilder app)
        {
            return  Container = base.CreateContainer(app);
        }

        protected override IContainer CreateContainer(IAppBuilder app)
        {
            return CreateDefaultContainer(app);
        }
    }
}
