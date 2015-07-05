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
                            break;
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

            if (!instanceRecord.IsValid())
            {
                throw new Exception("Registry install directory is missing.");
            }

            return instanceRecord;
        }

        public ApplicationInstanceRecord GetInstance()
        {
            return Get();
        }
    }
}