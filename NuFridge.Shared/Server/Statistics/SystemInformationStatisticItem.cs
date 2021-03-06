﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuFridge.Shared.Server.Statistics
{
    public class SystemInformationStatisticItem
    {
        public string FeedName { get; set; }
        public int DownloadCount { get; set; }
        public string Color { get; set; }

        public SystemInformationStatisticItem()
        {

        }

        public SystemInformationStatisticItem(string feedName, int downloadCount, string color)
        {
            FeedName = feedName;
            DownloadCount = downloadCount;
            Color = color;
        }
    }
}
