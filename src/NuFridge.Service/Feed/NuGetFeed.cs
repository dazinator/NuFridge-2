using System;
using System.Linq;
using System.Reactive.Linq;
using Autofac;
using Lucene.Net.Index;
using Lucene.Net.Linq;
using Microsoft.Owin.Hosting;
using NuFridge.Service.Feed.NuGet.Lucene.Web;
using NuGet.Lucene;
using NuGet.Lucene.Events;
using NuGet.Lucene.Web;
using NuGet.Lucene.Web.Controllers;
using NuGet.Lucene.Web.Models;
using NuGet.Lucene.Web.SignalR.Hubs;
using NuGet.Lucene.Web.Util;
using Owin;

namespace NuFridge.Service.Feed
{
    public class NuGetFeed : IDisposable
    {
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

        public void ConfigureEvents()
        {
            var controller = container.Resolve<ILucenePackageRepository>();

            controller.Indexer.PackagesAdded.Subscribe(delegate(IndexUpdateEvent @event)
            {
                
            });

            controller.Indexer.PackagesDeleted.Subscribe(delegate(IndexUpdateEvent @event)
            {
                
            });

            controller.Indexer.PackagesDownloaded.Subscribe(delegate(IndexUpdateEvent @event)
            {
                
            });
        }

        public void Start(string feedName, string feedDirectory, string baseAddress)
        {
            Console.WriteLine("Starting " + feedName + " feed at " + baseAddress + ".");

            BaseAddress = baseAddress;
            FeedDirectory = feedDirectory;
            webapp = WebApp.Start(baseAddress, WebAppStartup);

            Console.WriteLine("Successfully started " + feedName + " feed.");
        }

       

        public string BaseAddress { get; private set; }
        public string FeedDirectory { get; private set; }

        public void OptimizeIndexing()
        {
            var controller = container.Resolve<IndexingController>();

            controller.Optimize();
        }

        public EventHandler<EventArgs> NewPackageDetected;



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
            var settings = new NuGetFeedSettings(FeedDirectory);

            return settings;
        }

        public virtual IContainer CreateContainer(IAppBuilder app)
        {
            container = startup.CreateDefaultContainer(app);
            return container;
        }
    }
}