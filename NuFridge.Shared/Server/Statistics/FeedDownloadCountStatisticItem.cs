﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuFridge.Shared.Server.Statistics
{
    public class FeedDownloadCountStatisticItem
    {
        public string FeedName { get; set; }
        public int DownloadCount { get; set; }
        public string Color { get; set; }

        public FeedDownloadCountStatisticItem()
        {

        }

        public FeedDownloadCountStatisticItem(string feedName, int downloadCount, string color)
        {
            FeedName = feedName;
            DownloadCount = downloadCount;
            Color = color;
        }
    }
}
