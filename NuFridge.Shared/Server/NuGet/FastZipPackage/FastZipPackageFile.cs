/* 
* @author themotleyfool https://github.com/themotleyfool/NuGet.Lucene
* Apache License
* Version 2.0, January 2004
* http://www.apache.org/licenses/
*
* Copyright 2008-2012 by themotleyfool
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using System.Text;
using NuGet;

namespace NuFridge.Shared.Server.NuGet.FastZipPackage
{
    public class FastZipPackageFile : IPackageFile, IFrameworkTargetable
    {
        private readonly IFastZipPackage _fastZipPackage;
        private FrameworkName _targetFramework;
        private string _effectivePath;
        private bool _targetFrameworkParsed;

        public string Path { get; }

        public string EffectivePath
        {
            get
            {
                if (!(TargetFramework != null))
                    return Path;
                return _effectivePath;
            }
        }

        public FrameworkName TargetFramework
        {
            get
            {
                if (_targetFrameworkParsed)
                    return _targetFramework;
                _targetFrameworkParsed = true;

                var path = Uri.UnescapeDataString(Path);

                _targetFramework = VersionUtility.ParseFrameworkNameFromFilePath(path, out _effectivePath);
                if (_targetFramework != VersionUtility.UnsupportedFrameworkName)
                    return _targetFramework;
                string[] strArray1 = path.Split('/', '\\');
                if (strArray1.Length < 3)
                {
                    _targetFramework = null;
                    return _targetFramework;
                }
                string[] strArray2 = strArray1[1].Split(new char[1]
        {
          '-'
        }, 2);
                string identifier = strArray2[0];
                string version = "0.0";
                string profile = strArray2.Length == 2 ? strArray2[1] : "";
                for (int index = 0; index < identifier.Length; ++index)
                {
                    if (char.IsDigit(identifier[index]))
                    {
                        char[] chArray = identifier.Substring(index).ToCharArray();
                        identifier = identifier.Substring(0, index);
                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.Append(chArray[0]);
                        for (int startIndex = 1; startIndex < chArray.Length; ++startIndex)
                        {
                            stringBuilder.Append('.');
                            if (startIndex > 2)
                            {
                                stringBuilder.Append(chArray, startIndex, chArray.Length - startIndex);
                                break;
                            }
                            stringBuilder.Append(chArray[startIndex]);
                        }
                        if (stringBuilder.Length == 1)
                            stringBuilder.Append(".0");
                        version = stringBuilder.ToString();
                        break;
                    }
                }
                try
                {
                    _targetFramework = new FrameworkName(identifier, new Version(version), profile);
                }
                catch (ArgumentException)
                {
                }
                catch (FormatException)
                {
                }
                return _targetFramework;
            }
        }

        IEnumerable<FrameworkName> IFrameworkTargetable.SupportedFrameworks
        {
            get
            {
                if (TargetFramework != null)
                    yield return TargetFramework;
            }
        }

        internal FastZipPackageFile(IFastZipPackage fastZipPackage, string path)
        {
            _fastZipPackage = fastZipPackage;
            Path = Normalize(path);
        }

        private string Normalize(string path)
        {
            return path.Replace('/', System.IO.Path.DirectorySeparatorChar).TrimStart(System.IO.Path.DirectorySeparatorChar);
        }

        public Stream GetStream()
        {
            return _fastZipPackage.GetZipEntryStream(Path);
        }

        public override string ToString()
        {
            return Path;
        }
    }
}
