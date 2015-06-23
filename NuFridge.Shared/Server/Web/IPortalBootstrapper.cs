using System;
using Nancy.Bootstrapper;

namespace NuFridge.Shared.Server.Web
{
    public interface IPortalBootstrapper : INancyBootstrapper, IDisposable
    {
        ServerState State { get; set; }

        string StatusMessage { get; set; }
    }
}
