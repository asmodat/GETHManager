using Microsoft.AspNetCore.Mvc;
using System;
using AsmodatStandard.Extensions;
using AsmodatStandard.Extensions.Collections;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Net.Http;
using GEthManager.Model;
using System.Text;
using System.Linq;
using System.Diagnostics;
using MathNet.Numerics.Statistics;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using GEthManager.Ententions;

namespace GEthManager.Processing
{
    public class PerformanceManager
    {
        private readonly ManagerConfig _cfg;
        private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;

        private float[] cpuSamples;
        private int cpuSamplePosition = 0;

        /// <summary>
        /// 1m, 2m, 3m, 5m, 10m, 15m, 30m, 1h
        /// </summary>
        private float[] cpuMedianResults;

        /// <summary>
        /// 1m, 2m, 3m, 5m, 10m, 15m, 30m, 1h
        /// </summary>
        private float[] ramMedianResults;

        /// <summary>
        /// Time Frame Indexes, describe how many minutes are taken into account for each ram/cpu MedianResults index
        /// </summary>
        private int[] timeFrameIndexes = new int[] { 0,1,2,3,4,5,10,15,30,45,60 };


        private int cpuSamplesPerMinute;
        private int ramSamplesPerMinute;

        private float[] ramSamples;
        private int ramSamplePosition = 0;

        private DiskInfo[] driveInfo;

        private readonly ProcessManager _pm;

        private bool IsWindows = false;// RuntimeEx.IsWindows();

        public PerformanceManager(
            IOptions<ManagerConfig> cfg,
            ProcessManager pm)
        {
            _cfg = cfg.Value;
            _pm = pm;

            if (IsWindows)
            {
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            }

            cpuSamplesPerMinute = Math.Max((int)(60 * ((double)1000 / _cfg.cpuCountIntensity)), 1);
            ramSamplesPerMinute = Math.Max((int)(60 * ((double)1000 / _cfg.ramCountIntensity)), 1);
            cpuSamples = new float[60 * cpuSamplesPerMinute];
            ramSamples = new float[60 * ramSamplesPerMinute];
            cpuMedianResults = new float[timeFrameIndexes.Length];
            ramMedianResults = new float[timeFrameIndexes.Length];
        }

        public void TryUpdateCpuPerformanceCounter()
        {
            try
            {
                var cpuSample = cpuCounter?.NextValue() ?? 0;

                if (IsWindows)
                {
                    while (cpuSample == 0)
                    {
                        cpuSample = cpuCounter?.NextValue() ?? 0;
                        Thread.Sleep(10);
                    }
                }
                else
                {
                    cpuSample = _pm.GetGethCpuUsage();
                }

                cpuSamplePosition = (cpuSamplePosition + 1) % cpuSamples.Length;
                cpuSamples[cpuSamplePosition] = cpuSample;
                var nonZeroSamples = cpuSamples.Where(x => x != 0)?.ToArray();

                for (int i = 0; i < timeFrameIndexes.Length; i++)
                {
                    var minutes = timeFrameIndexes[i];
                    var cpuSamplesCount = minutes * cpuSamplesPerMinute;

                    if (nonZeroSamples.Length <= cpuSamplesCount)
                        continue;
                    
                    if (minutes == 0)
                    {
                        cpuMedianResults[i] = cpuSample;
                        continue;
                    }
                    
                    var cpuSubSample = nonZeroSamples.TakeLastWithRotation(cpuSamplesCount, cpuSamplePosition);

                    cpuMedianResults[i] = cpuSubSample.Median();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to TryUpdateCpuPerformanceCounter, Error Message: {ex.JsonSerializeAsPrettyException()}");
            }
        }

        public void TryUpdateRamPerformanceCounter()
        {
            try
            {
                var ramSample = ramCounter?.NextValue() ?? 0;

                if (IsWindows)
                {
                    while (ramSample == 0)
                    {
                        ramSample = ramCounter?.NextValue() ?? 0;
                        Thread.Sleep(10);
                    }
                }
                else
                {
                    var ramUsage = _pm.GetProcessesRamUsage();
                    switch (_cfg.healthCheckRAMType?.ToLower().Trim() ?? "max")
                    {
                        case "min":
                            ramSample = Math.Max(ramUsage.paged, ramUsage.phisical);
                            break;
                        case "paged":
                            ramSample = ramUsage.paged;
                            break;
                        case "phisical":
                            ramSample = ramUsage.phisical;
                            break;
                        case "max":
                        default:
                            ramSample = Math.Max(ramUsage.paged, ramUsage.phisical);
                            break;
                    }

                    ramSample = _cfg.TotalRamMemory - ramSample;
                }

                ramSamplePosition = (ramSamplePosition + 1) % ramSamples.Length;
                ramSamples[ramSamplePosition % ramSamples.Length] = ramSample;
                var nonZeroSamples = ramSamples.Where(x => x != 0)?.ToArray();

                for (int i = 0; i < timeFrameIndexes.Length; i++)
                {
                    var minutes = timeFrameIndexes[i];
                    var ramSamplesCount = minutes * ramSamplesPerMinute;

                    if (nonZeroSamples.Length <= ramSamplesCount)
                        continue;

                    if (minutes == 0)
                    {
                        ramMedianResults[i] = ramSample;
                        continue;
                    }
                    
                    var ramSubSample = nonZeroSamples.TakeLastWithRotation(ramSamplesCount, ramSamplePosition);

                    ramMedianResults[i] = ramSubSample.Median();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to TryUpdateRamPerformanceCounter, Error Message: {ex.JsonSerializeAsPrettyException()}");
            }
        }

        public void TryUpdateDriveInfo()
        {
            try
            {
                driveInfo = DriveInfo.GetDrives().Select(x => x.ToDiskInfo()).ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to TryUpdateDriveInfo, Error Message: {ex.JsonSerializeAsPrettyException()}");
            }
        }

        public float[] GetMedianRamResults() => ramMedianResults;
        public float[] GetMedianCpuResults() => cpuMedianResults;

        public DiskInfo[] GetDriveInfo() => driveInfo;

        public int[] TimeFrameIndexes() => timeFrameIndexes;
    }
}
