using GEthManager.Model;
using System.Diagnostics;

namespace GEthManager.Extentions
{
    public static class ProcessInfoEx
    {
        public static ProcessInfo ToProcessInfo(this Process p)
        {
            if (p == null)
                return null;

            try
            {
                
                return new ProcessInfo()
                {
                    id = p.Id,
                    processName = p.ProcessName,
                    sessionId = p.SessionId,
                    phisicalMemoryUsageMB = (float)p.WorkingSet64 / (1024 * 1024),
                    pagedMemorySizeMB = (float)p.PagedMemorySize64 / (1024 * 1024),
                    virtualMemorySizeMB = (float)p.VirtualMemorySize64 / (1024 * 1024),
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
