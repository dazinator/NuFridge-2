using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuFridge.Service.Data.Model;
using NuFridge.Service.Data.Repositories;
using NuFridge.Service.Plugin;
using NuFridge.Service.Plugins;
using NuGet.Lucene.Events;

namespace NuFridge.Service.Feeds
{
    public sealed class FeedManager : IDisposable
    {
        List<NuGetFeed> FeedServices { get; set; }

        public FeedManager()
        {
            FeedServices = new List<NuGetFeed>();
        }

        public void Dispose()
        {
            foreach (var feedService in FeedServices)
            {
                feedService.Dispose();
            }
        }

        private IEnumerable<Lazy<IPackagePublishReceiver>> PackagePublishRecievers { get; set; }

        private void LoadPlugins()
        {
            PackagePublishRecievers = new List<Lazy<IPackagePublishReceiver>>();

            Console.WriteLine("Loading NuGet feed plugins.");

            const string pluginFolder = "Plugins";

            if (!Directory.Exists(pluginFolder))
            {
                Directory.CreateDirectory(pluginFolder);
            }
            else
            {
                var files = Directory.GetFiles(pluginFolder, "*.dll");

                PackagePublishRecievers = PluginLoader.GetPlugins<IPackagePublishReceiver>(files);
            }
        }

        private void ConfigureEvents(NuGetFeed feed)
        {
            feed.PackagesAdded.Subscribe(delegate(IndexUpdateEvent eve)
            {
                var publishDataResults = eve.Packages.Select(package => new PackagePublishData {Id = package.Id}).ToList();

                foreach (var receiver in PackagePublishRecievers)
                {
                    receiver.Value.Execute(publishDataResults);
                }
            });
        }

        public void Start(ServiceConfiguration config)
        {
            LoadPlugins();

            Console.WriteLine("Starting NuGet feeds.");

            IRepository<Feed> feedRepository = new SqlCompactRepository<Feed>();

            var feeds = feedRepository.GetAll();

            bool success = true;

            if (!feeds.Any())
            {
                Console.WriteLine("No feeds found to start.");
            }
            else
            {
                foreach (var feedEntity in feeds)
                {
                    NuGetFeed feedService = new NuGetFeed(config);

                    if (!feedService.Start(feedEntity))
                    {
                        success = false;
                        Console.WriteLine("Failed to start " + feedEntity.Name + ".");
                        continue;
                    }

                    ConfigureEvents(feedService);

                    Console.WriteLine("Successfully started " + feedEntity.Name + ".");

                    FeedServices.Add(feedService);
                    break;
                }
            }

            if (!success)
            {
                Console.WriteLine("Warning: Not all feeds have successfully been started.");
            }
        }
    }
}