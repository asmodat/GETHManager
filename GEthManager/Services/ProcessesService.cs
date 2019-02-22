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
    public class ProcessesService : BackgroundService
    {
        private readonly ManagerConfig _cfg;
        private DateTime timestamp;
        private readonly ProcessManager _pm;

        public ProcessesService(IOptions<ManagerConfig> cfg, ProcessManager pm) 
        {
            _pm = pm;
            _cfg = cfg.Value;
            timestamp = DateTime.UtcNow;
        }

        protected override async Task Process()
        {
            var delay = 100;
            var intensity = _cfg.processesCheckIntensity;
            if ((DateTime.UtcNow - timestamp).TotalSeconds < intensity)
            {
                var timeUntilNextExecution = intensity - (DateTime.UtcNow - timestamp).TotalMilliseconds;
                if (timeUntilNextExecution > delay)
                    delay = (int)timeUntilNextExecution;
            }

            await Task.Delay(delay);
            _pm.TryUpdateRunningProcessesList();
            timestamp = DateTime.UtcNow;
        }
    }
}
