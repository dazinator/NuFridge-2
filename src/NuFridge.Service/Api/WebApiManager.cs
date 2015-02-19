using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using NuFridge.Service.Feeds;
using NuFridge.Service.Logging;

namespace NuFridge.Service.Api
{
    public sealed class WebApiManager : IDisposable
    {
        private static readonly ILog Logger = LogProvider.For<WebApiManager>(); 

        private IDisposable WebApiApp { get; set; }

        public void Dispose()
        {
            if (WebApiApp != null)
            {
                WebApiApp.Dispose();
                WebApiApp = null;
            }
        }

        public void Start(ServiceConfiguration config)
        {
            Logger.Info("Starting website.");

            string baseAddress = config.ApiWebBinding;

            WebApiApp = WebApp.Start<ApiStartup>(baseAddress);
        }
    }
}