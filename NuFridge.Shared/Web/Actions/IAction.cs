using Nancy;

namespace NuFridge.Shared.Web.Actions
{
    public interface IAction
    {
        dynamic Execute(dynamic parameters, INancyModule module);
    }
}