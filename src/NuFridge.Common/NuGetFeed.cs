using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using Autofac;
using Microsoft.Owin.Hosting;
using NuFridge.Common.NuGet.Lucene.Web;
using NuFridge.DataAccess.Model;
using NuGet.Lucene;
using NuGet.Lucene.Events;
using NuGet.Lucene.Web;
using NuGet.Lucene.Web.Controllers;
using NuGet.Lucene.Web.Models;
using Owin;

namespace NuFridge.Common
{
    public class NuGetFeed : IDisposable
    {
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

        public IObservable<IndexUpdateEvent> PackagesAdded
        {
            get
            {
                var controller = container.Resolve<ILucenePackageRepository>();

                return controller.Indexer.PackagesAdded;
            }
        }

        public IObservable<IndexUpdateEvent> PackagesDeleted
        {
            get
            {
                var controller = container.Resolve<ILucenePackageRepository>();

                return controller.Indexer.PackagesDeleted;
            }
        }

        public IObservable<IndexUpdateEvent> PackagesDownloaded
        {
            get
            {
                var controller = container.Resolve<ILucenePackageRepository>();

                return controller.Indexer.PackagesDownloaded;
            }
        }

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

        public void Start(Feed feed)
        {
            var baseAddress = string.Format("{0}{1}", Config.FeedWebBinding, feed.Name);
            var feedDirectory = Path.Combine(Config.FeedsHome, feed.Name);

            Console.WriteLine("Starting " + feed.Name + " feed at " + baseAddress + ".");

            BaseAddress = baseAddress;
            FeedDirectory = feedDirectory;
            webapp = WebApp.Start(baseAddress, WebAppStartup);

            Console.WriteLine("Successfully started " + feed.Name + " feed.");
        }
    }
}