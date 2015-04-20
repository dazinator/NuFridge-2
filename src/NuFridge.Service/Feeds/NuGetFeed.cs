using System;
using System.IO;
using Autofac;
using Microsoft.Owin.Hosting;
using NuFridge.Service.Feeds.NuGet.Lucene.Web;
using NuFridge.Service.Logging;
using NuFridge.Service.Model;
using NuGet.Lucene;
//using NuGet.Lucene.Events;
using NuGet.Lucene.Web;
using NuGet.Lucene.Web.Controllers;
using NuGet.Lucene.Web.Models;
using Owin;

namespace NuFridge.Service.Feeds
{
    public class NuGetFeed : IDisposable
    {
        private static readonly ILog Logger = LogProvider.For<NuGetFeed>();

        private IDisposable webapp;

        public void Dispose()
        {
            if (webapp != null)
            {
                webapp.Dispose();
                webapp = null;
            }
        }

        //public IObservable<IndexUpdateEvent> PackagesAdded
        //{
        //    get
        //    {
        //        var controller = container.Resolve<ILucenePackageRepository>();

        //        return controller.Indexer.PackagesAdded;
        //    }
        //}

        //public IObservable<IndexUpdateEvent> PackagesDeleted
        //{
        //    get
        //    {
        //        var controller = container.Resolve<ILucenePackageRepository>();

        //        return controller.Indexer.PackagesDeleted;
        //    }
        //}

        //public IObservable<IndexUpdateEvent> PackagesDownloaded
        //{
        //    get
        //    {
        //        var controller = container.Resolve<ILucenePackageRepository>();

        //        return controller.Indexer.PackagesDownloaded;
        //    }
        //}

        private Feed Feed { get; set; }
        public string Id { get; private set; }

        private void WebAppStartup(IAppBuilder app)
        {
            var startup = new CustomStartup(Feed);
            startup.Configuration(app);
        }



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
                webapp = WebApp.Start(baseAddress, WebAppStartup);

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