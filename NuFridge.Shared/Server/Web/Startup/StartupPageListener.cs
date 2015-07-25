using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.Configuration;

namespace NuFridge.Shared.Server.Web.Startup
{
    public class StartupPageListener : IStartupPageListener
    {
        private bool ShowStartupPage { get; set; }
        private readonly HttpListener _listener = new HttpListener();
        private Thread _listenerThread;
        private readonly ILog _log = LogProvider.For<StartupPageListener>();
        private string TextStatus { get; set; } = "Loading.";

        private readonly IWebPortalConfiguration _portalConfiguration;

        public StartupPageListener(IWebPortalConfiguration portalConfiguration)
        {
            _portalConfiguration = portalConfiguration;
        }

        public void UpdateStatus(string status)
        {
            TextStatus = status;
        }

        public void Start()
        {
            _log.Debug("Starting the loading page.");

            Uri[] listenPrefixes = WebServerInitializer.GetListenPrefixes(_log, _portalConfiguration.ListenPrefixes);

            foreach (Uri uri in listenPrefixes)
            {
                string prefix = uri.ToString();
                if (!uri.Host.Contains("."))
                    prefix = prefix.Replace("localhost", "+");
                _listener.Prefixes.Add(prefix);
            }

            _listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;

            _listener.Start();

            ShowStartupPage = true;

            _listenerThread = new Thread(StartListenerThread);
            _listenerThread.Start();
        }

        private void StartListenerThread()
        {
            while (ShowStartupPage)
            {
                var result = _listener.BeginGetContext(ListenerCallBack, _listener);
                result.AsyncWaitHandle.WaitOne();
            }
        }

        private void ListenerCallBack(IAsyncResult result)
        {
            HttpListenerContext context;

            try
            {
                context = _listener.EndGetContext(result);
            }
            catch (HttpListenerException)
            {
                return;
            }

            byte[] data = Encoding.UTF8.GetBytes(TextStatus);

            var output = context.Response.OutputStream;
            output.Write(data, 0, data.Length);

            context.Response.StatusCode = 200;
            context.Response.Close();
        }

        public void Stop()
        {
            _log.Debug("Stopping the loading page.");

            ShowStartupPage = false;

            _listener.Stop();
        }
    }

    public interface IStartupPageListener
    {
        void Start();
        void Stop();
        void UpdateStatus(string status);
    }
}
