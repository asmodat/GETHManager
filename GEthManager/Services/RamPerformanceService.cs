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
    public class RamPerformanceService : BackgroundService
    {
        private readonly ManagerConfig _cfg;
        private PerformanceManager _pm;
        private readonly DateTime timestamp;

        public RamPerformanceService(IOptions<ManagerConfig> cfg, PerformanceManager pm) 
        {
            _pm = pm;
            _cfg = cfg.Value;
            timestamp = DateTime.UtcNow;
        }


        protected override async Task Process()
        {
            var delay = 10;
            if ((DateTime.UtcNow - timestamp).TotalSeconds < _cfg.ramCountIntensity)
            {
                var timeUntilNextExecution = _cfg.ramCountIntensity - (DateTime.UtcNow - timestamp).TotalMilliseconds;
                if (timeUntilNextExecution > delay)
                    delay = (int)timeUntilNextExecution;
            }

            await Task.Delay(delay);
            _pm.TryUpdateRamPerformanceCounter();
        }
    }
}
