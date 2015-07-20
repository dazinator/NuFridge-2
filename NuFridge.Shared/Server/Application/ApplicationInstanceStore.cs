using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;

namespace NuFridge.Shared.Server.Application
{
    public class ApplicationInstanceStore : IApplicationInstanceStore
    {
        private const RegistryHive Hive = RegistryHive.LocalMachine;
        private const RegistryView View = RegistryView.Registry32;
        private const string KeyName = "Software\\NuFridge\\NuFridge";

        private ApplicationInstanceRecord Get()
        {
            var instanceRecord = new ApplicationInstanceRecord();

            using (RegistryKey registryKey1 = RegistryKey.OpenBaseKey(Hive, View))
            {
                using (RegistryKey registryKey2 = registryKey1.OpenSubKey(KeyName, false))
                {
                    if (registryKey2 == null)
                        return instanceRecord;

                    foreach (string str in registryKey2.GetValueNames())
                    {
                        if (str == ApplicationInstanceRecord.InstallDirectoryKey)
                        {
                            var value = registryKey2.GetValue(str);
                            instanceRecord.InstallDirectory = value.ToString();
                            continue;
                        }
                        if (str == ApplicationInstanceRecord.NuGetFrameworkNamesKey)
                        {
                            var value = registryKey2.GetValue(str);
                            instanceRecord.NuGetFrameworkNames = value.ToString();
                            continue;
                        }
                    }
                }
            }

#if DEBUG
            if (string.IsNullOrWhiteSpace(instanceRecord.InstallDirectory))
            {
                instanceRecord.InstallDirectory = Directory.GetParent(Assembly.GetEntryAssembly().Location).FullName;
            }
#endif

            if (string.IsNullOrWhiteSpace(instanceRecord.NuGetFrameworkNames))
            {
                instanceRecord.NuGetFrameworkNames = String.Empty;
            }

            if (!instanceRecord.IsValid())
            {
                throw new Exception("Registry install directory is missing.");
            }



            return instanceRecord;
        }

        public void Save(ApplicationInstanceRecord record)
        {
            using (RegistryKey registryKey1 = RegistryKey.OpenBaseKey(Hive, View))
            {
                RegistryKey registryKey2 = registryKey1.OpenSubKey(KeyName, true);

                if (registryKey2 == null)
                {
                    try
                    {
                        registryKey2 = registryKey1.CreateSubKey(KeyName, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None);
                    }
                    catch (Exception ex)
                    {
                       throw new Exception("Failed to create a subkey in the registry when updating the known NuGet framework names.", ex);
                    }
                }

                try
                {
                    registryKey2.SetValue(ApplicationInstanceRecord.NuGetFrameworkNamesKey, record.NuGetFrameworkNames);
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to update a subkey in the registry when updating the known NuGet framework names.", ex);
                }

            }
        }

        public ApplicationInstanceRecord GetInstance()
        {
            return Get();
        }
    }
}