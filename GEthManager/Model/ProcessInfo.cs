using System.Diagnostics;

namespace GEthManager.Model
{
    public class ProcessInfo
    {
        public int id { get; set; }
        public string processName { get; set; }
        public int sessionId { get; set; }

        public long phisicalMemoryUsageMB { get; set; }
        public long pagedMemorySizeMB { get; set; }
        public long virtualMemorySizeMB { get; set; }
    }
}
