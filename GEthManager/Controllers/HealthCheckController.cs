using Microsoft.AspNetCore.Mvc;
using System;
using AsmodatStandard.Extensions;
using AsmodatStandard.Extensions.Collections;
using System.Threading.Tasks;
using GEthManager.Processing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using GEthManager.Model;
using System.Diagnostics;
using System.Linq;
using AsmodatStandard.Extensions.AspNetCore;
using GEthManager.Ententions;
using Microsoft.AspNetCore.Authorization;

namespace GEthManager.Controllers
{
    [Route("api/HealthCheck")]
    public class HealthCheckController : Controller
    {
        private readonly ManagerConfig _cfg;

        private ProcessManager _pm;
        private BlockSyncManager _bs;
        private PerformanceManager _prm;

        public HealthCheckController(
            IHttpContextAccessor accessor,
            IHostingEnvironment hostingEnvironment,
            ProcessManager pm,
            BlockSyncManager bs,
            PerformanceManager prm,
            IOptions<ManagerConfig> cfg)
        {
            _pm = pm;
            _bs = bs;
            _cfg = cfg.Value;
            _prm = prm;
        }

        [HttpGet("GEth")]
        public IActionResult GEth()
        {
            var lastSync = _bs.GetPrivateSyncing()?.TryGetHighestBlock() ?? -1;
            var lastBlock = _bs.GetLastBlockNr(apiOnly: true)?.blockNumber ?? -1;

            lastBlock = Math.Max(lastBlock, lastSync);

            var ourSync = _bs.GetPrivateSyncing()?.TryGetCurrentBlock() ?? -1;
            var ourBlock = _bs.GetPrivateBlockNr()?.blockNumber ?? -1;

            ourBlock = Math.Max(ourBlock, ourSync);

            var blockTime = _bs.GetAverageBlockTime();
            var syncSpeed = _bs.GetAverageSyncSpeed();

            float syncState = 0;

            if(lastBlock > 0 && ourBlock > 0)
                syncState = 100 - (((float)(lastBlock - ourBlock) / lastBlock) * 100);

            var isHealthy = true;

            var index = _prm.TimeFrameIndexes().FindIndex(x => x == _cfg.healthCheckTimeFrame);
            if (index < 0)
                index = 0;

            var cpuIntensity = _prm.GetMedianCpuResults()[index];

            if (cpuIntensity <= 0)
                cpuIntensity = _prm.GetMedianCpuResults().FirstOrDefault(x => x > 0);

            var ramFree = _prm.GetMedianRamResults()[index];

            if (ramFree <= 0)
                ramFree = _prm.GetMedianRamResults().FirstOrDefault(x => x > 0);

            var driveInfo = _prm.GetDriveInfo()?.FirstOrDefault(x => x.name == _cfg.healthCheckDiskName);
            var diskSpace = (100 - (driveInfo?.availableFreeSpace ?? 0));

            if (lastBlock <= 0 || ourBlock <= 0)
                isHealthy = false;

            if(lastBlock - ourBlock > _cfg.healthCheckBlockDelay)
                isHealthy = false;

           if(cpuIntensity > _cfg.healthCheckCPU)
                isHealthy = false;

            if (diskSpace > _cfg.healthCheckDiskSpace)
                isHealthy = false;

            if (ramFree < _cfg.healthCheckRAM)
                isHealthy = false;

            var hc = new HealthCheck()
            {
                isHealthy = isHealthy,
                lastBlock = lastBlock,
                ourBlock = ourBlock,
                cpuUsed = cpuIntensity,
                cpuMax = _cfg.healthCheckCPU,
                ramFree = ramFree,
                ramMin = _cfg.healthCheckRAM,
                diskUsed = diskSpace,
                diskMax = _cfg.healthCheckDiskSpace,
                blockTimeAvg = blockTime,
                syncState = syncState,
                syncSpeedAvg = syncSpeed
            };

            if (!isHealthy)
                return StatusCode(StatusCodes.Status500InternalServerError, hc);

            return StatusCode(StatusCodes.Status200OK, hc);
        }
    }
}
