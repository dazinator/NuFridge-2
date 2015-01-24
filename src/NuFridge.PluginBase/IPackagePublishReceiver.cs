using NuFridge.Service.Events;

namespace NuFridge.PluginBase
{
    public interface IPackagePublishReceiver
    {
        void Execute(NewPackageDetectedEvent eve);
    }
}