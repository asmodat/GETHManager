using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using GEthManager.Processing;

namespace GEthManager.Services
{
    public class SyncScanService : BackgroundService
    {
        private readonly ManagerConfig _cfg;
        private BlockSyncManager _bsm;

        public SyncScanService(IOptions<ManagerConfig> cfg, BlockSyncManager bsm) 
        {
            _bsm = bsm;
            _cfg = cfg.Value;
        }

        protected override async Task Process()
        {
            var bn = _bsm.GetPrivateBlockNr();
            var delay = 100;
            if (bn != null && (DateTime.UtcNow - bn.TimeStamp).TotalSeconds < _cfg.privateRequestDelay)
            {
                var timeUntilNextExecution = (_cfg.privateRequestDelay * 1000) - (DateTime.UtcNow - bn.TimeStamp).TotalMilliseconds;
                if (timeUntilNextExecution > delay)
                    delay = (int)timeUntilNextExecution;
            }

            await Task.Delay(delay);
            await _bsm.TryUpdatePrivateSyncing();
        }
    }
}
