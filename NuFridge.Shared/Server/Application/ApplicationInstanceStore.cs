using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

namespace NuFridge.Shared.Server.Application
{
    public class ApplicationInstanceStore : IApplicationInstanceStore
    {
        private const RegistryHive Hive = RegistryHive.LocalMachine;
        private const RegistryView View = RegistryView.Registry32;
        private const string KeyName = "Software\\NuFridge";

        //TODO this method is bad
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

                        if (str == ApplicationInstanceRecord.SqlDataSourceKey)
                        {
                            var value = registryKey2.GetValue(str);
                            instanceRecord.SqlDataSource = value.ToString();
                            continue;
                        }

                        if (str == ApplicationInstanceRecord.SqlInitialCatalogKey)
                        {
                            var value = registryKey2.GetValue(str);
                            instanceRecord.SqlInitialCatalog = value.ToString();
                            continue;
                        }

                        if (str == ApplicationInstanceRecord.SqlUsernameKey)
                        {
                            var value = registryKey2.GetValue(str);
                            instanceRecord.SqlUsername = value.ToString();
                            continue;
                        }

                        if (str == ApplicationInstanceRecord.SqlPasswordKey)
                        {
                            var value = registryKey2.GetValue(str);
                            instanceRecord.SqlPassword = value.ToString();
                            continue;
                        }

                        if (str == ApplicationInstanceRecord.ListenPrefixesKey)
                        {
                            var value = registryKey2.GetValue(str);
                            instanceRecord.ListenPrefixes = value.ToString();
                            continue;
                        }
                    }
                }
            }

            if (!instanceRecord.IsValid())
            {
                throw new Exception("Registry settings are missing.");
            }

            return instanceRecord;
        }

        public ApplicationInstanceRecord GetInstance()
        {
            return Get();
        }
    }
}