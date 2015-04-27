using System;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Owin.Hosting;
using NuFridge.Service.Logging;
using NuFridge.Service.Model;
using NuGet.Lucene.Web.Util;

namespace NuFridge.Service.Feeds
{
    public class NuGetFeed : IDisposable
    {
        private static readonly ILog Logger = LogProvider.For<NuGetFeed>();

        private CustomStartup _startup;
        private IDisposable _webApp;


        public void Dispose()
        {
            if (_webApp != null)
            {
                _webApp.Dispose();
                _webApp = null;
            }

            if (_startup != null)
            {
                _startup.WaitForShutdown(TimeSpan.FromMinutes(1));
                _startup = null;
            }


        }

        private Feed Feed { get; set; }
        public string Id { get; private set; }

        public bool Start(Feed feed)
        {
            Id = feed.Id;
            Feed = feed;

            var config = new ServiceConfiguration();

            var baseUrl = config.FeedWebBinding;

            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }

            var baseAddress = string.Format("{0}{1}", baseUrl, feed.Name);
            var feedDirectory = Path.Combine(config.FeedsHome, feed.Id);

            Logger.Info("Starting " + feed.Name + " at " + feedDirectory);

            try
            {

                _webApp = WebApp.Start(baseAddress, app =>
                {
                    _startup = new CustomStartup(feed);
                    _startup.Configuration(app);
                    
                });

                return true;
            }
            catch (Exception ex)
            {
                var baseException = ex.GetBaseException();

                Logger.Info("Exception: " + baseException.Message);

                return false;
            }
            finally
            {
                Feed = null;
            }
        }
    }
}