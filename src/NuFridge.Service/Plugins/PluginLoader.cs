using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuFridge.Service.Plugins
{
    public static class PluginLoader
    {
        public static IEnumerable<Lazy<T>> GetPlugins<T>(string assembly) where T : class
        {
            var file = new FileInfo(assembly);
            return GetPlugins<T>(new[] { file });
        }

        public static IEnumerable<Lazy<T>> GetPlugins<T>(string[] assemblies) where T : class
        {
            if (assemblies.Length == 0)
            {
                return new List<Lazy<T>>();
            }

            var files = assemblies.Select(x => new FileInfo(x));
            return GetPlugins<T>(files);
        }

        public static IEnumerable<Lazy<T>> GetPlugins<T>(FileInfo assembly) where T : class
        {
            return GetPlugins<T>(new[] { assembly });
        }

        public static IEnumerable<Lazy<T>> GetPlugins<T>(IEnumerable<FileInfo> assemblies) where T : class
        {
            if (assemblies == null || assemblies.All(x => x == null))
            {
                throw new ArgumentNullException();
            }
            return GetLazyExports<T>(assemblies);
        }

        public static IEnumerable<T> LoadPlugins<T>(string assembly) where T : class
        {
            var file = new FileInfo(assembly);
            return LoadPlugins<T>(new[] { file });
        }

        public static IEnumerable<T> LoadPlugins<T>(string[] assemblies) where T : class
        {
            var files = assemblies.Select(x => new FileInfo(x));
            return LoadPlugins<T>(files);
        }

        public static IEnumerable<T> LoadPlugins<T>(FileInfo assembly) where T : class
        {
            return LoadPlugins<T>(new[] { assembly });
        }

        public static IEnumerable<T> LoadPlugins<T>(IEnumerable<FileInfo> assemblies) where T : class
        {
            if (assemblies == null || assemblies.All(x => x == null))
            {
                throw new ArgumentNullException();
            }
            var lazyExports = GetLazyExports<T>(assemblies);
            return lazyExports.Select(x => x.Value);
        }

        private static IEnumerable<Lazy<T>> GetLazyExports<T>(IEnumerable<FileInfo> assemblies) where T : class
        {
            var assemblyCatalogs = assemblies.Where(a => a != null).Select(a => new AssemblyCatalog(a.FullName));
            var container = new CompositionContainer(new AggregateCatalog(assemblyCatalogs));
            return container.GetExports<T>();
        }
    }
}