using System;

namespace NuFridge.Shared.Server.Web
{
    public interface IWebServerInitializer : IDisposable
    {
        void Start();

        void Starting(string message);

        void Started();

        void Stopping(string message);

        void Stop();
    }
}
