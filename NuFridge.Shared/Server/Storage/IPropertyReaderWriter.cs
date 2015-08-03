namespace NuFridge.Shared.Server.Storage
{
    public interface IPropertyReaderWriter<TCast>
    {
        bool Read(object target, out TCast value);

        void Write(object target, TCast value);
    }
}
