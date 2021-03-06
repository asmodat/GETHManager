﻿using Microsoft.AspNetCore.Mvc;
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
    public class CpuPerformanceService : BackgroundService
    {
        private readonly ManagerConfig _cfg;
        private PerformanceManager _pm;
        private DateTime timestamp;

        public CpuPerformanceService(IOptions<ManagerConfig> cfg, PerformanceManager pm) 
        {
            _pm = pm;
            _cfg = cfg.Value;
            timestamp = DateTime.UtcNow;
        }


        protected override async Task Process()
        {
            var delay = 10;
            if ((DateTime.UtcNow - timestamp).TotalSeconds < _cfg.cpuCountIntensity)
            {
                var timeUntilNextExecution = _cfg.cpuCountIntensity - (DateTime.UtcNow - timestamp).TotalMilliseconds;
                if (timeUntilNextExecution > delay)
                    delay = (int)timeUntilNextExecution;
            }

            await Task.Delay(delay);
            _pm.TryUpdateCpuPerformanceCounter();
            timestamp = DateTime.UtcNow;
        }
    }
}
