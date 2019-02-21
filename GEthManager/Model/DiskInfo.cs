using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AsmodatStandard.Extensions;
using Newtonsoft.Json;

namespace GEthManager.Model
{
    public static class DiskInfoEx
    {
        public static DiskInfo ToDiskInfo(this DriveInfo di)
        {
            if (di == null)
                return null;

            return new DiskInfo()
            {
                name = di.Name?.ToLower().ReplaceMany((" ", ""), (":", ""), ("/", ""), ("\\", "")) ?? "undefined",
                label = di.VolumeLabel,
                format = di.DriveFormat,
                type = di.DriveType.ToString(),
                totalSize = di.TotalSize,
                availableFreeSpace = di.TotalSize <= 0 ? 0 : (float)((double)di.AvailableFreeSpace / di.TotalSize) * 100,
                totalFreeSpace = di.TotalSize <= 0 ? 0 : (float)((double)di.TotalFreeSpace / di.TotalSize) * 100,
            };
        }
    }

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
