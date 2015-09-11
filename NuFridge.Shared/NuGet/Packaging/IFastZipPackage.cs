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
using NuGet;

namespace NuFridge.Shared.NuGet.Packaging
{
    public interface IFastZipPackage : IPackage, IPackageMetadata, IPackageName, IServerPackageMetadata, IDisposable
    {
        new bool Listed
        {
            set;
            get;
        }

        string GetFileLocation();

        Stream GetZipEntryStream(string path);
    }
}
