﻿using System;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace NuFridge.Shared.Application
{
    public class ApplicationInstanceStore : IApplicationInstanceStore
    {
        public const RegistryHive Hive = RegistryHive.LocalMachine;
        public const RegistryView View = RegistryView.Registry32;
        public const string KeyName = "Software\\NuFridge\\NuFridge";

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
            instanceRecord.InstallDirectory = Directory.GetParent(Assembly.GetEntryAssembly().Location).FullName;
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