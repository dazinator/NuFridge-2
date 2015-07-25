using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.Configuration;

namespace NuFridge.Shared.Server.Web.Listeners
{
    public class ShutdownPageListener : IShutdownPageListener
    {
        private bool ShowStartupPage { get; set; }
        private readonly HttpListener _listener = new HttpListener();
        private Thread _listenerThread;
        private readonly ILog _log = LogProvider.For<ShutdownPageListener>();
        private string TextStatus { get; set; } = "Loading.";
        private string HtmlPageFormat { get; set; }

        private readonly IWebPortalConfiguration _portalConfiguration;

        public ShutdownPageListener(IWebPortalConfiguration portalConfiguration)
        {
            _portalConfiguration = portalConfiguration;
        }

        public void UpdateStatus(string status)
        {
            TextStatus = status;
        }

        public void Start()
        {
            _log.Debug("Starting the shutdown page.");

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

            string dataToOutput = HtmlPageFormat;

            if (context.Request.AcceptTypes != null && context.Request.AcceptTypes.Contains("text/html"))
            {
                if (dataToOutput == null)
                {
                    Assembly assembly =
                        Assembly.LoadFile(
                            Path.Combine(Directory.GetParent(Assembly.GetEntryAssembly().Location).FullName,
                                "NuFridge.Website.dll"));
                    string resourceNamespaceRoot = "NuFridge.Website";

                    string indexPageName = resourceNamespaceRoot + ".Shutdown.index.html";
                    string jqueryJsName = resourceNamespaceRoot + ".Scripts.jquery-1.9.1.min.js";
                    string customCssName = resourceNamespaceRoot + ".Semantic.custom.css";
                    string semanticUiCssName = resourceNamespaceRoot + ".Semantic.semantic.css";

                    using (Stream stream = assembly.GetManifestResourceStream(indexPageName))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            dataToOutput = reader.ReadToEnd();
                        }
                    }

                    using (Stream stream = assembly.GetManifestResourceStream(jqueryJsName))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            dataToOutput = dataToOutput.Replace("%jqueryscript%", "<script>" + reader.ReadToEnd() + "</script>");
                        }
                    }

                    using (Stream stream = assembly.GetManifestResourceStream(customCssName))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            dataToOutput = dataToOutput.Replace("%customcss%", "<style>" + reader.ReadToEnd() + "</style>");
                        }
                    }

                    using (Stream stream = assembly.GetManifestResourceStream(semanticUiCssName))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            dataToOutput = dataToOutput.Replace("%semanticuicss%", "<style>" + reader.ReadToEnd() + "</style>");
                        }
                    }

                    HtmlPageFormat = dataToOutput;
                }

                dataToOutput = dataToOutput.Replace("%status%", TextStatus);

                context.Response.ContentType = "text/html";
            }
            else
            {
                dataToOutput = TextStatus;
                context.Response.ContentType = "plain/text";
            }

            context.Response.AddHeader("NuFridge-Status", "shutdown");

            byte[] data = Encoding.UTF8.GetBytes(dataToOutput);
            context.Response.ContentLength64 = data.Length;
            var output = context.Response.OutputStream;
            output.Write(data, 0, data.Length);



            context.Response.StatusCode = 200;
            context.Response.Close();
        }

        public void Stop()
        {
            _log.Debug("Stopping the shutdown page.");

            ShowStartupPage = false;

            _listener.Stop();
        }
    }

    public interface IShutdownPageListener
    {
        void Start();
        void Stop();
        void UpdateStatus(string status);
    }
}
