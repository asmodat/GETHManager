using System.IO;
using AsmodatStandard.Extensions;
using GEthManager.Model;

namespace GEthManager.Ententions
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
}
