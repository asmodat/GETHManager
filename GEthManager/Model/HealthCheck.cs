using System.IO;
using AsmodatStandard.Extensions;

namespace GEthManager.Model
{
    public class HealthCheck
    {
        public bool isHealthy;

        public long ourBlock;
        public long lastBlock;

        public float cpuUsed;
        public float cpuMax;

        public float ramFree;
        public float ramMin;

        public float diskUsed;
        public float diskMax;
    }
}
