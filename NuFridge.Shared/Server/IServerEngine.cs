using Hangfire;

namespace NuFridge.Shared.Server
{
    public interface IServerEngine
    {
        void Start();

        void Stop();

        BackgroundJobServerOptions BackgroundJobServerOptions { get; }
    }
}
