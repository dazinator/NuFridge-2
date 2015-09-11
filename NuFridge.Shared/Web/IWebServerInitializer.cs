using System;

namespace NuFridge.Shared.Web
{
    public interface IWebServerInitializer : IDisposable
    {
        void Start();

        void Stop();
    }
}