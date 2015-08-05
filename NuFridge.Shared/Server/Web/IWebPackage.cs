using System;
using NuGet;

namespace NuFridge.Shared.Server.Web
{
    public interface IWebPackage : IPackage
    {
        Uri Uri { get; }
    }
}
