using Microsoft.AspNetCore.Mvc;
using System;
using AsmodatStandard.Extensions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Net.Http;
using GEthManager.Model;
using System.Text;
using Microsoft.Extensions.Hosting;
using System.Threading;
using GEthManager.Processing;

namespace GEthManager.Services
{
    public class PublicScanService : BackgroundService
    {
        private readonly ManagerConfig _cfg;
        private BlockSyncManager _bsm;

        public PublicScanService(IOptions<ManagerConfig> cfg, BlockSyncManager bsm) 
        {
            _bsm = bsm;
            _cfg = cfg.Value;
        }

        protected override async Task Process()
        {
            var bn = _bsm.GetPublicBlockNr();
            var delay = 100;
            if (bn != null && (DateTime.UtcNow - bn.TimeStamp).TotalSeconds < _cfg.publicRequestDelay)
            {
                var timeUntilNextExecution = (_cfg.publicRequestDelay * 1000) - (DateTime.UtcNow - bn.TimeStamp).TotalMilliseconds;
                if (timeUntilNextExecution > delay)
                    delay = (int)timeUntilNextExecution;
            }

            await Task.Delay(delay);
            await _bsm.TryUpdatePublicBlockHeight();
        }
    }
}
