using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;

namespace NuFridge.Shared.Server.Web
{
    public interface IWebPackage : IPackage
    {
        Uri Uri { get; }
    }
}
