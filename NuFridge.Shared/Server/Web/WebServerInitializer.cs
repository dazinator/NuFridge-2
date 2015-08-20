using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Nancy.Hosting.Self;
using NuFridge.Shared.Extensions;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.Configuration;
using NuFridge.Shared.Server.Web.Nancy;

namespace NuFridge.Shared.Server.Web
{
    public class WebServerInitializer : IWebServerInitializer
    {
        private readonly IWebPortalConfiguration _configuration;
        private readonly IPortalBootstrapper _portalBootstrapper;
        private readonly IHomeConfiguration _homeConfiguration;
        private readonly ILog _log = LogProvider.For<WebServerInitializer>();
        private CustomNancyHost _host;

        public WebServerInitializer(IWebPortalConfiguration configuration, IPortalBootstrapper portalBootstrapper, IHomeConfiguration homeConfiguration)
        {
            _configuration = configuration;
            _portalBootstrapper = portalBootstrapper;
            _homeConfiguration = homeConfiguration;
        }

        public void Start()
        {
            _log.Info("Starting web server.");

            Uri[] listenPrefixes = GetListenPrefixes(_log, _configuration.ListenPrefixes);
            IPortalBootstrapper portalBootstrapper = _portalBootstrapper;
            HostConfiguration hostConfiguration = new HostConfiguration();
            hostConfiguration.UnhandledExceptionCallback = (OnException);
            HostConfiguration configuration = hostConfiguration;
            Uri[] uriArray = listenPrefixes;
            _host = new CustomNancyHost(portalBootstrapper, configuration, _homeConfiguration, uriArray)
            {
                AuthenticationSchemeSelector = request => AuthenticationSchemes.Anonymous
            };
            try
            {
                _host.Start();
            }
            catch (AutomaticUrlReservationCreationFailureException ex)
            {
                _log.ErrorException(ex.Message, ex);

                throw new Exception(ExceptionExtensions.SuggestUrlReservations(listenPrefixes));
            }
            catch (HttpListenerException ex)
            {
                _log.ErrorException(ex.Message, ex);

                string message = ex.SuggestSolution(listenPrefixes);
                if (message != null)
                    throw new Exception(message, ex);
                throw;
            }
            foreach (Uri uri in listenPrefixes)
                _log.InfoFormat("NuFridge server running at: {0}://localhost:{1}{2}", uri.Scheme, uri.Port, uri.PathAndQuery);

            _log.Info("Web server is ready to process requests");
        }


        public static Uri[] GetListenPrefixes(ILog log, string listenPrefixes)
        {
            List<Uri> list = new List<Uri>();
            if (!string.IsNullOrWhiteSpace(listenPrefixes))
            {
                string str1 = listenPrefixes;
                char[] chArray = new char[2]
                {
                    ';',
                    ','
                };
                foreach (string str2 in str1.Split(chArray).Select(v => v.Trim()).Where(v => v.Length > 0).ToArray())
                {
                    log.Trace("Adding listen prefix: " + str2);
                    try
                    {
                        list.Add(new Uri(str2.TrimEnd('/') + "/", UriKind.Absolute));
                    }
                    catch (Exception ex)
                    {
                        log.ErrorException(ex.Message, ex);
                        throw new UriFormatException("Unable to parse listen prefix '" + str2 + "': " + ex.Message, ex);
                    }
                }
            }
            if (list.Count == 0)
            {
                log.Trace("No urls provided to listen on. Using the following default: http://localhost:8050/");
                list.Add(new Uri("http://localhost:8050/", UriKind.Absolute));
            }
            return list.ToArray();
        }


        private void OnException(Exception exception)
        {
            HttpListenerException listenerException = exception as HttpListenerException;
            if (listenerException != null && (listenerException.ErrorCode == 1229 || listenerException.ErrorCode == 64 || listenerException.ErrorCode == 995))
                return;
            _log.ErrorException("Unhandled exception from web server: " + exception.Message, exception);
        }

        public void Dispose()
        {
            try
            {
                _host.Dispose();
            }
            catch (Exception ex)
            {
                _log.ErrorException(ex.Message, ex);
            }
        }

        public void Stop()
        {
            if (_host == null)
                return;
            _log.Info("Shutting down the web server");
            try
            {
                _host.Stop();
            }
            catch (Exception ex)
            {
                _log.ErrorException(ex.Message, ex);
            }
        }
    }
}
