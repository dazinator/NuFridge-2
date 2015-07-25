using System;
using NuFridge.Shared.Logging;

namespace NuFridge.Shared.Server.Web
{
    public interface IWebServerInitializer : IDisposable
    {
        void Start();

        void Stop();
    }
}