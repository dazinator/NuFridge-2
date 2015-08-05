using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using Microsoft.Win32;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.Application;
using NuFridge.Shared.Server.Storage;
using NuGet;

namespace NuFridge.Shared.Server.NuGet
{
    public class FrameworkNamesRepository : IFrameworkNamesRepository
    {
        private readonly ILog _log = LogProvider.For<FrameworkNamesRepository>();
        private readonly ISet<FrameworkName> _foundFrameworkNames = new HashSet<FrameworkName>();
        private readonly object _sync = new object();
        private const string LegacyRegistryKeyName = "NuGetFrameworkNames";


        private readonly IStore _store;

        public FrameworkNamesRepository(IStore store)
        {
            _store = store;

            LoadRecordsFromDatabase();
        }

        private void LoadRecordsFromDatabase()
        {
            _log.Info("Loading framework names from the database into memory.");

            List<Framework> frameworks;

            using (var dbContext = new DatabaseContext())
            {
                frameworks = dbContext.Frameworks.ToList();
            }

            lock (_sync)
            {
                foreach (var framework in frameworks)
                {
                    var frameworkName = VersionUtility.ParseFrameworkName(framework.Name);

                    _foundFrameworkNames.Add(frameworkName);
                }
            }

            LoadLegacyRecordsFromRegistry();
        }

        private void LoadLegacyRecordsFromRegistry()
        {
            using (RegistryKey localMachineRegistryKey = RegistryKey.OpenBaseKey(ApplicationInstanceStore.Hive, ApplicationInstanceStore.View))
            {
                using (RegistryKey nufridgeReadOnlyRegistryKey = localMachineRegistryKey.OpenSubKey(ApplicationInstanceStore.KeyName, false))
                {
                    if (nufridgeReadOnlyRegistryKey == null)
                        return;

                    foreach (string name in nufridgeReadOnlyRegistryKey.GetValueNames())
                    {
                        if (name == LegacyRegistryKeyName)
                        {
                            object value = nufridgeReadOnlyRegistryKey.GetValue(name);
                            if (value != null)
                            {
                                Add(value.ToString());

                                try
                                {
                                    using (RegistryKey nufridgeWritableRegistryKey = localMachineRegistryKey.OpenSubKey(ApplicationInstanceStore.KeyName, true))
                                    {
                                        if (nufridgeWritableRegistryKey == null)
                                            return;

                                        nufridgeWritableRegistryKey.DeleteValue(LegacyRegistryKeyName, false);

                                        _log.Info("Deleted the legacy registry key called " + LegacyRegistryKeyName);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _log.WarnException("Failed to delete the legacy registry key called " + LegacyRegistryKeyName, ex);
                                }
                            }
                            break;
                        }
                    }


                }
            }
        }


        public void Add(string frameworkNames)
        {
            if (frameworkNames == null)
            {
                return;
            }

            var split =
                frameworkNames.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(VersionUtility.ParseFrameworkName)
                    .Where(it => it != VersionUtility.UnsupportedFrameworkName);

            lock (_sync)
            {
                using (var dbContext = new DatabaseContext())
                {
                    foreach (var s in split)
                    {

                        if (!_foundFrameworkNames.Contains(s))
                        {
                            _foundFrameworkNames.Add(s);

                            Framework framework = new Framework { Name = VersionUtility.GetShortFrameworkName(s) };


                            dbContext.Frameworks.Add(framework);

                        }

                    }
                    dbContext.SaveChanges();
                }
            }
        }

        public ISet<FrameworkName> Get()
        {
            return _foundFrameworkNames;
        }
    }

    public interface IFrameworkNamesRepository
    {
        void Add(string frameworkNames);
        ISet<FrameworkName> Get();
    }
}