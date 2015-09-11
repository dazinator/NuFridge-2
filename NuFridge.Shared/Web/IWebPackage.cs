using System;
using NuGet;

namespace NuFridge.Shared.Web
{
    public interface IWebPackage : IPackage
    {
        Uri Uri { get; }
    }
}
