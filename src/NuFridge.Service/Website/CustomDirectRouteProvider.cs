using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

namespace NuFridge.Service.Website
{
    public class CustomDirectRouteProvider : DefaultDirectRouteProvider
    {
        protected override string GetRoutePrefix(HttpControllerDescriptor controllerDescriptor)
        {
            var routePrefix = base.GetRoutePrefix(controllerDescriptor);
            var controllerBaseType = controllerDescriptor.ControllerType.BaseType;

            if (controllerDescriptor.ControllerType.AssemblyQualifiedName != Assembly.GetExecutingAssembly().FullName)
            {
                //TODO: Check for extra slashes
                routePrefix = "api/{tenantid}/" + routePrefix;
            }

            return routePrefix;
        }
    }
}