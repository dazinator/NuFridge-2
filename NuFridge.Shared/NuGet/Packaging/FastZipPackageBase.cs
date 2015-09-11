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
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;

namespace NuFridge.Shared.NuGet.Packaging
{
    public abstract class FastZipPackageBase : IDisposable
    {
        private ZipFile _zipFile;

        public abstract Stream GetStream();

        public void Dispose()
        {
            if (_zipFile == null)
                return;
            _zipFile.Close();
            _zipFile = null;
        }

        public Stream GetZipEntryStream(string path)
        {
            if (_zipFile == null)
                _zipFile = new ZipFile(GetStream());
            ZipEntry entry = _zipFile.GetEntry(path.Replace('\\', '/'));
            if (entry == null)
                throw new ArgumentException("Zip file is invalid.", "path");
            return _zipFile.GetInputStream(entry);
        }

        public static DateTime GetPackageCreatedDateTime(Stream stream)
        {
            return new ZipFile(stream).Cast<ZipEntry>().Where(f => f.Name.EndsWith(".nuspec")).Select(f => f.DateTime).FirstOrDefault();
        }
    }
}
