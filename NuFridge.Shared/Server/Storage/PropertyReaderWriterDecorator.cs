namespace NuFridge.Shared.Server.Storage
{
    public class PropertyReaderWriterDecorator : IPropertyReaderWriter<object>
    {
        private readonly IPropertyReaderWriter<object> _original;

        public PropertyReaderWriterDecorator(IPropertyReaderWriter<object> original)
        {
            _original = original;
        }

        public virtual bool Read(object target, out object value)
        {
            if (_original == null)
            {
                value = null;
                return false;
            }

            return _original.Read(target, out value);
        }

        public virtual void Write(object target, object value)
        {
            _original.Write(target, value);
        }
    }
}
