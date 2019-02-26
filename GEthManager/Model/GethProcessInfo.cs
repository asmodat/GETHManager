using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AsmodatStandard.Extensions;
using AsmodatStandard.Extensions.Collections;
using GEthManager.Extentions;

namespace GEthManager.Model
{
    public class GEthProcessInfo
    {
        public GEthProcessInfo(
            bool hasExited,
            Process process, 
            List<long> gethReStarts,
            DateTime startTime)
        {
            this.hasExited = hasExited;
            this.processInfo = process?.ToProcessInfo();
            this.startTime = startTime;

            var now = DateTime.UtcNow;
            var lastTicks = gethReStarts.LastOrDefault();
            lastTicks = lastTicks == 0 ? lastTicks : now.Ticks;
            restartLast = DateTime.UtcNow - new DateTime(lastTicks);

            List<long> dReStarts = new List<long>();
            restartCount = gethReStarts.Count;

            for(int i = 0; i < gethReStarts.Count - 2; i++)
                dReStarts.Add(gethReStarts[i + 1] - gethReStarts[i]);

            if (dReStarts.IsNullOrEmpty())
            {
                restartAverage = restartLast;
                restartMin = restartLast;
                restartMax = restartLast;
                restartCount = 0;
            }
            else
            {
                restartAverage = new TimeSpan((long)dReStarts.Average());
                restartMin = new TimeSpan(dReStarts.Min());
                restartMax = new TimeSpan(dReStarts.Max());
            }
        }

        public bool hasExited;

        public DateTime startTime;

        public TimeSpan restartLast;
        public TimeSpan restartAverage;
        public TimeSpan restartMin;
        public TimeSpan restartMax;
        public int restartCount;

        public ProcessInfo processInfo;
    }
}
