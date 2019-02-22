using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using GEthManager.Processing;

namespace GEthManager.Services
{
    public class GethProcessService : BackgroundService
    {
        private readonly ManagerConfig _cfg;
        private DateTime timestamp;
        private readonly ProcessManager _pm;

        public GethProcessService(IOptions<ManagerConfig> cfg, ProcessManager pm) 
        {
            _pm = pm;
            _cfg = cfg.Value;
            timestamp = DateTime.UtcNow;
        }

        protected override async Task Process()
        {
            var delay = 100;
            var intensity = _cfg.gethReStartIntensity;
            if ((DateTime.UtcNow - timestamp).TotalSeconds < intensity)
            {
                var timeUntilNextExecution = intensity - (DateTime.UtcNow - timestamp).TotalMilliseconds;
                if (timeUntilNextExecution > delay)
                    delay = (int)timeUntilNextExecution;
            }

            await Task.Delay(delay);

            if(_pm.IsGethExited())
                _pm.ReStartGETH();

            timestamp = DateTime.UtcNow;
        }
    }
}
