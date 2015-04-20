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

        private ServiceConfiguration Config { get; set; }

        public NuGetFeed(ServiceConfiguration config)
        {
            Config = config;
        }

        private CustomStartup startup;
        private IDisposable webapp;
        private IContainer container;

        public void Dispose()
        {
            if (webapp != null)
            {
                webapp.Dispose();
                webapp = null;
            }

            if (startup != null)
            {
                startup.WaitForShutdown(TimeSpan.FromMinutes(1));
                startup = null;
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

        public string BaseAddress { get; private set; }
        public string FeedDirectory { get; private set; }

        public void OptimizeIndexing()
        {
            var controller = container.Resolve<IndexingController>();

            controller.Optimize();
        }

        public void SynchronizePackages()
        {
            var controller = container.Resolve<IndexingController>();
            
            controller.Synchronize(new SynchronizationOptions()
            {
                Mode = SynchronizationMode.Incremental
            });
        }


        public virtual void WebAppStartup(IAppBuilder app)
        {
            startup = new CustomStartup(this);
            startup.Configuration(app);
        }

        public virtual INuGetWebApiSettings CreateSettings()
        {
            var settings = new NuGetFeedSettings(Config, FeedDirectory);

            return settings;
        }

        public virtual IContainer CreateContainer(IAppBuilder app)
        {
            container = startup.CreateDefaultContainer(app);
            return container;
        }

        public bool Start(Feed feed)
        {
            var baseUrl = Config.FeedWebBinding;

            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }

            var baseAddress = string.Format("{0}{1}", baseUrl, feed.Name);
            var feedDirectory = Path.Combine(Config.FeedsHome, feed.Id);

            BaseAddress = baseAddress;
            FeedDirectory = feedDirectory;

            Logger.Info("Starting " + feed.Name + " at " + feedDirectory);

            var settings = CreateSettings();

            if (!Directory.Exists(feedDirectory))
            {
                Directory.CreateDirectory(feedDirectory);
            }

            if (!Directory.Exists(settings.PackagesPath))
            {
                Directory.CreateDirectory(settings.PackagesPath);
            }

            if (!Directory.Exists(settings.LucenePackagesIndexPath))
            {
                Directory.CreateDirectory(settings.LucenePackagesIndexPath);
            }

            if (!Directory.Exists(settings.LuceneUsersIndexPath))
            {
                Directory.CreateDirectory(settings.LuceneUsersIndexPath);
            }

            if (!Directory.Exists(settings.SymbolsPath))
            {
                Directory.CreateDirectory(settings.SymbolsPath);
            }



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
        }
    }
}