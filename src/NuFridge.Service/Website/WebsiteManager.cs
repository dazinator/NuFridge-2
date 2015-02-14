using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using NuFridge.Common;
using NuFridge.Service.Website;

namespace NuFridge.Service
{
    public sealed class WebsiteManager : IDisposable
    {
        private IDisposable WebsiteApp { get; set; }

        public void Dispose()
        {
            if (WebsiteApp != null)
            {
                WebsiteApp.Dispose();
                WebsiteApp = null;
            }
        }

        public void Start(ServiceConfiguration config)
        {
            string baseAddress = config.ApiWebBinding;

            WebsiteApp = WebApp.Start<WebsiteStartup>(baseAddress);
        }
    }
}