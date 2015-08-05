using Nancy;

namespace NuFridge.Shared.Server.Web.Actions
{
    public interface IAction
    {
        dynamic Execute(dynamic parameters, INancyModule module);
    }
}