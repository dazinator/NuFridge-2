using System;
using NuFridge.Shared.Database;
using NuFridge.Shared.Scheduler;
using NuFridge.Shared.Web;
using NuFridge.Shared.Web.Listeners;

namespace NuFridge.Shared.Application
{
    public class ServerEngine : IServerEngine
    {
        private readonly Lazy<IWebServerInitializer> _webHostInitializer;
        private readonly Lazy<IStoreInitializer> _storeInitializer;
        private readonly Lazy<IStartupPageListener> _startupPageListener;
        private readonly IShutdownPageListener _shutdownPageListener;
        private readonly Lazy<IJobServerManager> _jobServer; 

        public ServerEngine(Lazy<IWebServerInitializer> webHostInitializer, Lazy<IStoreInitializer> storeInitializer, Lazy<IStartupPageListener> startupPageListener, Lazy<IJobServerManager> jobServer, IShutdownPageListener shutdownPageListener)
        {
            _webHostInitializer = webHostInitializer;
            _storeInitializer = storeInitializer;
            _startupPageListener = startupPageListener;
            _jobServer = jobServer;
            _shutdownPageListener = shutdownPageListener;
        }

        public void Start()
        {
            //Start the loading page http listener
            _startupPageListener.Value.Start();
            
            //Upgrade the database
            _storeInitializer.Value.Initialize(_startupPageListener.Value.UpdateStatus);

            //Stop the loading page http listener
            _startupPageListener.Value.Stop();

            //Run the full web server http listener
            _webHostInitializer.Value.Start();

            //Start the job server
            _jobServer.Value.Start(_startupPageListener.Value.UpdateStatus);
        }

        public void Stop()
        {
            _webHostInitializer.Value.Stop();

            _shutdownPageListener.Start();

            _jobServer.Value.Stop(_shutdownPageListener.UpdateStatus);

            _shutdownPageListener.UpdateStatus("Performing final cleanup before shutdown");

            _webHostInitializer.Value.Dispose();

            _shutdownPageListener.Stop();
        }
    }
}