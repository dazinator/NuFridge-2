using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.StaticFiles;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.Configuration;
using NuFridge.Shared.Server.Web.SignalR;
using Owin;

namespace NuFridge.Shared.Server.Web.Nancy
{
    [Serializable]
    public class CustomNancyHost : IDisposable
    {
        private readonly IList<Uri> _baseUriList;
        private readonly HostConfiguration _configuration;
        private readonly INancyBootstrapper _bootstrapper;
        private readonly IHomeConfiguration _homeConfiguration;
        private readonly ILog _log = LogProvider.For<CustomNancyHost>();
        private IDisposable _webApp;

        public AuthenticationSchemeSelector AuthenticationSchemeSelector { get; set; }

        public CustomNancyHost(INancyBootstrapper bootstrapper, HostConfiguration configuration, IHomeConfiguration homeConfiguration, params Uri[] baseUris)
        {
            _bootstrapper = bootstrapper;
            _homeConfiguration = homeConfiguration;
            _configuration = configuration ?? new HostConfiguration();
            _baseUriList = baseUris;
        }

        public void Dispose()
        {
            Stop();
            _bootstrapper.Dispose();
        }

        public void Start()
        {
            var urls = GetPrefixes();

            _webApp = WebApp.Start(urls.First(), delegate (IAppBuilder builder)
            {
                var fileSystem = new PhysicalFileSystem(_homeConfiguration.WebsiteDirectory);
                
                var fileServerOptions = new FileServerOptions
                {
                    EnableDirectoryBrowsing = false,
                    FileSystem = fileSystem,
                    StaticFileOptions = { FileSystem = fileSystem, ServeUnknownFileTypes = false},
                    DefaultFilesOptions = { DefaultFileNames = new List<string> {"index.html"}, FileSystem = fileSystem},
                    DirectoryBrowserOptions = { FileSystem = fileSystem},
                    EnableDefaultFiles = true
                };

                builder.UseFileServer(fileServerOptions);
                builder.MapSignalR("/signalr", new HubConfiguration { EnableDetailedErrors = true, EnableJavaScriptProxies = true, Resolver = GlobalHost.DependencyResolver, EnableJSONP = false});
                builder.UseNancy(options => options.Bootstrapper = _bootstrapper);
                builder.UseCors(CorsOptions.AllowAll);
            });
        }

        public void Stop()
        {
            _webApp.Dispose();
        }

        private IEnumerable<string> GetPrefixes()
        {
            foreach (Uri uri in _baseUriList)
            {
                string prefix = uri.ToString();
                if (_configuration.RewriteLocalhost && !uri.Host.Contains("."))
                    prefix = prefix.Replace("localhost", "+");
                yield return prefix;
            }
        }
    }
}