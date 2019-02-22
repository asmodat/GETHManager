using System.IO;
using AsmodatStandard.Extensions;

namespace GEthManager.Model
{
    public class DiskInfo
    {
        public string name { get; set; }
        public string label { get; set; }
        public string format { get; set; }
        public string type { get; set; }
        public long totalSize { get; set; }

        /// <summary>
        /// percentage
        /// </summary>
        public float availableFreeSpace { get; set; }

        /// <summary>
        /// percentage
        /// </summary>
        public float totalFreeSpace { get; set; }
    }
}
