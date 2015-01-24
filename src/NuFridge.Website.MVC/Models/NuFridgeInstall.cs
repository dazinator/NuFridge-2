﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NuFridge.Website.MVC.Models
{


    public class NuFridgeInstall
    {      

        /// <summary>
        /// The database system to use - SQL compact, Mongo etc.
        /// </summary>
        public string DatabaseSystem { get; set; }

        public string MongoDBServer { get; set; }
        public string MongoDBDatabase { get; set; }

        public int PortNumber { get; set; }
        public string IISWebsiteName { get; set; }
        public string PhysicalDirectory { get; set; }
    }
}