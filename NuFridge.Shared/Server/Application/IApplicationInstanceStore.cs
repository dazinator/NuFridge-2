namespace NuFridge.Shared.Server.Application
{
    public interface IApplicationInstanceStore
    {
        ApplicationInstanceRecord GetInstance();
    }
}