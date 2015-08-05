using System;

namespace NuFridge.Shared.Server.Web
{
    public interface IWebServerInitializer : IDisposable
    {
        void Start();

        void Stop();
    }
}