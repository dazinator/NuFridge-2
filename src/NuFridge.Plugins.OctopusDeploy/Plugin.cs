﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NuFridge.Service.Plugin;

namespace NuFridge.Plugins.OctopusDeploy
{
    public class Plugin : IPackagePublishReceiver 
    {
        public void Execute(List<NuGet.Lucene.LucenePackage> packages)
        {
            //Do something
        }
    }
}