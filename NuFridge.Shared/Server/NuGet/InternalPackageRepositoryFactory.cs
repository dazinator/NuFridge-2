using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.OData.Formatter;
using NuFridge.Shared.Server.Configuration;
using NuFridge.Shared.Server.FileSystem;
using NuFridge.Shared.Server.Storage;
using NuGet;

namespace NuFridge.Shared.Server.NuGet
{
    public class InternalPackageRepositoryFactory : IInternalPackageRepositoryFactory
    {
        private readonly IHomeConfiguration _config;
        private volatile ISet<FrameworkName> _foundFrameworkNames = new HashSet<FrameworkName>();
        private readonly object sync = new object();

        private Func<int, IInternalPackageRepository> CreateRepoFunc { get; set; }

        public InternalPackageRepositoryFactory(Func<int, IInternalPackageRepository> createRepoFunc, IHomeConfiguration config)
        {
            _config = config;
            CreateRepoFunc = createRepoFunc;

            LoadFrameworkNames(config.NuGetFrameworkNames);
        }

        public void Init()
        {
            _config.Save();
        }

        public void AddFrameworkNames(string names)
        {
            bool updateConfig = false;
            var split =
                names.Split(new[] {"|"}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(VersionUtility.ParseFrameworkName)
                    .Where(it => it != VersionUtility.UnsupportedFrameworkName);

            lock (sync)
            {
                foreach (var s in split)
                {
                    if (!_foundFrameworkNames.Contains(s))
                    {
                        _foundFrameworkNames.Add(s);
                        updateConfig = true;
                    }
                }
            }

            if (updateConfig)
            {
                lock (sync)
                {
                    _config.NuGetFrameworkNames = string.Join("|", _foundFrameworkNames.Select(VersionUtility.GetShortFrameworkName));
                    _config.Save();
                }
            }
        }

        public void LoadFrameworkNames(string names)
        {
            lock (sync)
            {
                if (!string.IsNullOrWhiteSpace(names))
                {
                    var items =
                        names.Split(new[] {"|"}, StringSplitOptions.RemoveEmptyEntries)
                            .Select(VersionUtility.ParseFrameworkName)
                            .Where(it => it != VersionUtility.UnsupportedFrameworkName);

                    _foundFrameworkNames = new HashSet<FrameworkName>(_foundFrameworkNames.Union(items));
                }
            }
        }

        public ISet<FrameworkName> GetFrameworkNames()
        {
            return _foundFrameworkNames;
        }

        public IInternalPackageRepository Create(int feedId)
        {
            var packageRepository = CreateRepoFunc(feedId);

            return packageRepository;
        }
    }

    public interface IInternalPackageRepositoryFactory
    {
        IInternalPackageRepository Create(int feedId);
        ISet<FrameworkName> GetFrameworkNames();
        void AddFrameworkNames(string names);
        void Init();
    }
}