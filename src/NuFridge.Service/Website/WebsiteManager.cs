using System;
using System.IO;
using Microsoft.Owin.Hosting;
using NuFridge.Service.Feeds;
using NuFridge.Service.Logging;

namespace NuFridge.Service.Website
{
    public sealed class WebsiteManager : IDisposable
    {
        private static WebsiteManager _instance;

        protected WebsiteManager()
        {

        }

        public static WebsiteManager Instance()
        {
            if (_instance == null)
            {
                _instance = new WebsiteManager();
            }

            return _instance;
        }

        private static readonly ILog Logger = LogProvider.For<WebsiteManager>();

        private IDisposable WebsiteDisposable { get; set; }

        public void Dispose()
        {
         

            if (WebsiteDisposable != null)
            {
                Logger.Info("Stopping website.");

                WebsiteDisposable.Dispose();
                WebsiteDisposable = null;
            }
        }

        public void Start(ServiceConfiguration config)
        {
            Logger.Info("Starting website at " + config.WebsiteBinding + ".");

            string baseAddress = config.WebsiteBinding;

            WebsiteDisposable = WebApp.Start<WebisteStartupConfig>(baseAddress);
        }
    }
}