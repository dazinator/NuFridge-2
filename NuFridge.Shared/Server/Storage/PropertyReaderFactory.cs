using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace NuFridge.Shared.Server.Storage
{
    internal static class PropertyReaderFactory
    {
        private static readonly ConcurrentDictionary<string, object> Readers = new ConcurrentDictionary<string, object>();

        public static IPropertyReaderWriter<TCast> Create<TCast>(Type objectType, string propertyName)
        {
            string key = objectType.AssemblyQualifiedName + "-" + propertyName;
            IPropertyReaderWriter<TCast> propertyReaderWriter1 = null;
            if (Readers.ContainsKey(key))
                propertyReaderWriter1 = Readers[key] as IPropertyReaderWriter<TCast>;
            if (propertyReaderWriter1 != null)
                return propertyReaderWriter1;
            PropertyInfo property = objectType.GetProperty(propertyName);
            IPropertyReaderWriter<TCast> propertyReaderWriter2;
            if (property != null)
            {
                if (!typeof(TCast).IsAssignableFrom(property.PropertyType))
                    throw new InvalidOperationException(string.Format("Property type '{0}' for property '{1}.{2}' cannot be converted to type '{3}", property.PropertyType, property.DeclaringType == (Type)null ? "??" : property.DeclaringType.Name, property.Name, typeof(TCast).Name));
                Type type1 = typeof(Func<,>).MakeGenericType(property.DeclaringType, property.PropertyType);
                Type type2 = typeof(Action<,>).MakeGenericType(property.DeclaringType, property.PropertyType);
                Type type3 = typeof(DelegatePropertyReaderWriter<,,>).MakeGenericType(property.DeclaringType, property.PropertyType, typeof(TCast));
                MethodInfo getMethod = property.GetGetMethod();
                if (getMethod == null)
                    throw new ArgumentException(string.Format("The property '{0}' on type '{1}' does not contain a getter.", propertyName, property.DeclaringType));
                Delegate delegate1 = Delegate.CreateDelegate(type1, getMethod);
                MethodInfo setMethod = property.GetSetMethod(true);
                Delegate delegate2 = null;
                if (setMethod != null)
                    delegate2 = Delegate.CreateDelegate(type2, setMethod);
                propertyReaderWriter2 = (IPropertyReaderWriter<TCast>)Activator.CreateInstance(type3,  delegate1,  delegate2);
                Readers[key] = propertyReaderWriter2;
            }
            else
            {
                FieldInfo field = objectType.GetField(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);
                if (field == null)
                    throw new InvalidOperationException(string.Format("The type '{0}' does not define a property or field named '{1}'", objectType.FullName, propertyName));
                if (!typeof(TCast).IsAssignableFrom(field.FieldType))
                    throw new InvalidOperationException(string.Format("Field type '{0}' for field '{1}.{2}' cannot be converted to type '{3}", field.FieldType, field.DeclaringType == (Type)null ? "??" : field.DeclaringType.Name, field.Name, typeof(TCast).Name));
                propertyReaderWriter2 = new FieldReaderWriter<TCast>(field);
                Readers[key] = propertyReaderWriter2;
            }
            return propertyReaderWriter2;
        }

        private class DelegatePropertyReaderWriter<TInput, TReturn, TCast> : IPropertyReaderWriter<TCast> where TReturn : TCast
        {
            private readonly Func<TInput, TReturn> _caller;
            private readonly Action<TInput, TReturn> _writer;

            public DelegatePropertyReaderWriter(Func<TInput, TReturn> caller, Action<TInput, TReturn> writer)
            {
                _caller = caller;
                _writer = writer;
            }

            public bool Read(object target, out TCast value)
            {
                if (_caller == null)
                {
                    value = default(TCast);
                    return false;
                }

                value = _caller((TInput)target);
                return true;
            }

            public void Write(object target, TCast value)
            {
                TReturn @return = (TReturn)Converter.Convert(value, typeof(TReturn));
                _writer((TInput)target, @return);
            }
        }

        private class FieldReaderWriter<TCast> : IPropertyReaderWriter<TCast>
        {
            private readonly FieldInfo _field;

            public FieldReaderWriter(FieldInfo field)
            {
                _field = field;
            }

            public bool Read(object target, out TCast value)
            {
                value = (TCast)_field.GetValue(target);
                return true;
            }

            public void Write(object target, TCast value)
            {
                _field.SetValue(target, value);
            }
        }
    }
}
