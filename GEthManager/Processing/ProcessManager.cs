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
using GEthManager.Extentions;

namespace GEthManager.Processing
{
    public class ProcessManager
    {
        private readonly ManagerConfig _cfg;

        private Process geth;

        private ProcessInfo[] processList;


        public ProcessManager(IOptions<ManagerConfig> cfg)
        {
            _cfg = cfg.Value;
        }

        public bool IsGethExited() => geth?.HasExited ?? true;

        public ProcessInfo[] GetRunningProcessesList() => processList;

        public bool TryUpdateRunningProcessesList()
        {
            try
            {
                var processes = Process.GetProcesses();
                if (processes.IsNullOrEmpty())
                {
                    processList = new ProcessInfo[0];
                    return true;
                }
                processList = processes.Select(x => x.ToProcessInfo()).ToArray();
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Failed TryUpdateRunningProcessesList, error: {ex.JsonSerializeAsPrettyException()}");
                return false;
            }
        }

        public bool TryTerminateProcesses(IEnumerable<string> processesList)
        {
            try
            {
                var processes = Process.GetProcesses(Environment.MachineName);

                if (processes.IsNullOrEmpty())
                    return true;

                var success = true;

                foreach(var pName in processesList)
                {
                    if (pName.IsNullOrWhitespace())
                        continue;

                    var name = pName.Trim().ToLower();

                    var targets = processes.Where(x => (!x.ProcessName.IsNullOrEmpty() && x.ProcessName.Trim().ToLower() == name) || x.Id.ToString() == name);

                    if (targets.IsNullOrEmpty())
                        continue;

                    foreach(var target in targets)
                    {
                        try
                        {
                            target.Kill();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed To Terminate Process: '{target.ProcessName}', id: {target.Id}, error: {ex.JsonSerializeAsPrettyException()}");
                            success = false;
                        }
                    }
                }

                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed TryUpdateRunningProcessesList, error: {ex.JsonSerializeAsPrettyException()}");
                return false;
            }
        }

        public void ReStartGETH()
        {
            if (!_cfg.gethStartKillProcesses.IsNullOrEmpty())
                TryTerminateProcesses(_cfg.gethStartKillProcesses);

            try
            {
                geth = new Process();
                geth.StartInfo.FileName = _cfg.gethStartFileName;
                geth.StartInfo.Arguments = _cfg.gethStartArguments;
                geth.StartInfo.CreateNoWindow = true;
                geth.StartInfo.UseShellExecute = false;
                geth.StartInfo.RedirectStandardOutput = true;
                geth.StartInfo.RedirectStandardError = true;

                geth.OutputDataReceived += Geth_OutputDataReceived;
                geth.ErrorDataReceived += Geth_ErrorDataReceived;
                geth.Exited += Geth_Exited;

                geth.Start();
                geth.BeginOutputReadLine();
                geth.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start GETH node at location: '{_cfg.gethStartFileName ?? "undefined"}', error: {ex.JsonSerializeAsPrettyException()}");
            }
        }

        private void Geth_Exited(object sender, EventArgs e)
        {
            Console.WriteLine("GETH Process Was Exited");
        }

        private void Geth_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            var data = e.Data;
            if (_cfg.gethEnableConsole && !data.IsNullOrEmpty())
                Console.WriteLine(data);
        }

        private void Geth_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            var data = e.Data;
            if (_cfg.gethEnableConsole && !data.IsNullOrEmpty())
                Console.WriteLine(data);
        }
    }
}
