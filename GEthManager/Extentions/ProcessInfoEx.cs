﻿using GEthManager.Model;
using System.Diagnostics;

namespace GEthManager.Extentions
{
    public static class ProcessInfoEx
    {
        public static ProcessInfo ToProcessInfo(this Process p)
        {
            if (p == null)
                return null;

            return new ProcessInfo()
            {
                id = p.Id,
                processName = p.ProcessName,
                sessionId = p.SessionId,

                phisicalMemoryUsageMB = p.WorkingSet64 / (1024 * 1024),
                pagedMemorySizeMB = p.PagedMemorySize64 / (1024 * 1024),
                virtualMemorySizeMB = p.VirtualMemorySize64 / (1024 * 1024),
            };
        }
    }
}
