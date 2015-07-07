using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;

namespace NuFridge.Shared.Server.Web.Actions
{
    public interface IAction
    {
        dynamic Execute(dynamic parameters, INancyModule module);
    }
}