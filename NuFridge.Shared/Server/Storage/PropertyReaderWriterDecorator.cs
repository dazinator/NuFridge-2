namespace NuFridge.Shared.Server.Storage
{
    public class PropertyReaderWriterDecorator : IPropertyReaderWriter<object>
    {
        private readonly IPropertyReaderWriter<object> _original;

        public PropertyReaderWriterDecorator(IPropertyReaderWriter<object> original)
        {
            _original = original;
        }

        public virtual object Read(object target)
        {
            return _original.Read(target);
        }

        public virtual void Write(object target, object value)
        {
            _original.Write(target, value);
        }
    }
}
