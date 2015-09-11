namespace NuFridge.Shared.Application
{
    public interface IApplicationInstanceStore
    {
        ApplicationInstanceRecord GetInstance();
    }
}