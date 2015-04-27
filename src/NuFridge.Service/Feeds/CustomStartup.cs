using Autofac;
using NuFridge.Service.Feeds.NuGet.Lucene.Web;
using NuFridge.Service.Model;
using NuGet.Lucene.Web;
using Owin;

namespace NuFridge.Service.Feeds
{
    public class CustomStartup : Startup
    {
        private readonly Feed _feed;
        public CustomStartup(Feed feed)
        {
            _feed = feed;
        }

        protected override INuGetWebApiSettings CreateSettings()
        {
            var settings = new NuGetFeedSettings(_feed);

            return settings;
        }
    }
}
