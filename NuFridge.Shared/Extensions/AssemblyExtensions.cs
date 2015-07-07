using System;
using System.Linq;
using System.Reflection;

namespace NuFridge.Shared.Extensions
{
    public static class AssemblyExtensions
    {
        public static string FullLocalPath(this Assembly assembly)
        {
            return Uri.UnescapeDataString(new UriBuilder(assembly.CodeBase).Path).Replace("/", "\\");
        }

        public static string GetFileVersion(this Assembly assembly)
        {
            AssemblyFileVersionAttribute versionAttribute = assembly.GetCustomAttributes(true).OfType<AssemblyFileVersionAttribute>().FirstOrDefault();
            if (versionAttribute != null)
                return versionAttribute.Version;
            return "Unknown";
        }

        public static string GetInformationalVersion(this Assembly assembly)
        {
            AssemblyInformationalVersionAttribute versionAttribute = assembly.GetCustomAttributes(true).OfType<AssemblyInformationalVersionAttribute>().FirstOrDefault();
            if (versionAttribute != null)
                return versionAttribute.InformationalVersion;
            return "Unknown";
        }
    }
}
