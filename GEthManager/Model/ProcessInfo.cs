using System;
using System.Diagnostics;

namespace GEthManager.Model
{
    public class ProcessInfo
    {
        public int id { get; set; }
        public string processName { get; set; }
        public int sessionId { get; set; }

        public float phisicalMemoryUsageMB { get; set; }
        public float pagedMemorySizeMB { get; set; }
        public float virtualMemorySizeMB { get; set; }
        
    }
}
