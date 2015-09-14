﻿using System;

namespace NuFridge.Shared.Scheduler.Servers
{
    public class JobServerDownload : JobServerInstance
    {
        public override string QueueName => "download";

        public override int WorkerCount => Math.Min(Environment.ProcessorCount * 2, 6);
    }
}