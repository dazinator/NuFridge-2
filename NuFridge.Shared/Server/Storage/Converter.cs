using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace NuFridge.Shared.Server.Storage
{
    public static class Converter
    {
        public static object Convert(object source, Type targetType)
        {
            if (source == null || source == DBNull.Value)
            {
                if (!targetType.IsValueType)
                    return null;
                return Activator.CreateInstance(targetType);
            }
            Type type = source.GetType();
            if (targetType.IsAssignableFrom(type))
                return source;
            if (targetType.IsEnum && type == typeof(string))
                return Enum.Parse(targetType, (string)source, true);
            TypeConverter converter1 = TypeDescriptor.GetConverter(targetType);
            if (converter1.CanConvertFrom(type))
                return converter1.ConvertFrom(source);
            TypeConverter converter2 = TypeDescriptor.GetConverter(type);
            if (converter2.CanConvertTo(targetType))
                return converter2.ConvertTo(source, targetType);
            MethodInfo methodInfo = targetType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public).FirstOrDefault(x =>
            {
                if (x.Name == "op_Implicit")
                    return targetType.IsAssignableFrom(x.ReturnType);
                return false;
            });
            if (methodInfo != null)
                return methodInfo.Invoke(null, new object[1]
        {
          source
        });
            IEnumerable source1 = source as IEnumerable;
            if (source1 != null && targetType.IsArray)
            {
                object[] objArray = source1.Cast<object>().ToArray();
                Type elementType = targetType.GetElementType();
                Array instance = Array.CreateInstance(elementType, objArray.Length);
                for (int index = 0; index < objArray.Length; ++index)
                {
                    object obj = Convert(objArray[index], elementType);
                    instance.SetValue(obj, index);
                }
                return instance;
            }
            if (targetType == typeof(string))
                return source.ToString();
            string uriString = source as string;
            if (uriString != null && targetType == typeof(Uri))
                return new Uri(uriString);
            return source;
        }
    }
}
