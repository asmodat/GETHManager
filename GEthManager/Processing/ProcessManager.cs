using Microsoft.AspNetCore.Mvc;
using System;
using AsmodatStandard.Extensions;
using AsmodatStandard.Extensions.IO;
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

        private DateTime gethStart = DateTime.MinValue;
        private Process geth;
        private List<long> gethReStarts = new List<long>();

        private ProcessInfo[] processList;
        private GEthProcessInfo gethProcessInfo;

        private string _errorLog = "";
        private string _outputLog = "";

        public ProcessManager(IOptions<ManagerConfig> cfg)
        {
            _cfg = cfg.Value;
        }

        public string GetOutputLog(int n)
        {
            if (n < 0 || n > _cfg.maxGethInMemoryOutputLogLength)
                throw new Exception($"Failed to get output log, fetch ({n}) out of range <1,{_cfg.maxGethInMemoryOutputLogLength}>");

            if (_outputLog.Length <= n)
                n = _outputLog.Length;

            var startIndex = Math.Max(0, _outputLog.Length - n);
            var str = _outputLog.Substring(startIndex, n);
            return str.ReplaceMany(("INFO [", "\n\rINFO ["), ("WARN [", "\n\rWARN ["), ("WARNING [", "\n\rWARNING ["), ("ERROR [", "\n\rERROR ["));
        }

        public string GetErrorLog(int n)
        {
            if (n < 0 || n > _cfg.maxGethInMemoryErrorLogLength)
                throw new Exception($"Failed to get output log, fetch ({n}) out of range <1,{_cfg.maxGethInMemoryErrorLogLength}>");

            if (_errorLog.Length <= n)
                n = _errorLog.Length;

            var startIndex = Math.Max(0, _errorLog.Length - n);
            var str = _errorLog.Substring(startIndex, n);
            return str.ReplaceMany(("INFO [", "\n\rINFO ["), ("WARN [", "\n\rWARN ["), ("WARNING [", "\n\rWARNING ["), ("ERROR [", "\n\rERROR ["));
        }

        public bool IsGethExited() => geth?.HasExited ?? true;

        public ProcessInfo[] GetRunningProcessesList() => processList;

        public GEthProcessInfo GetGethProcessInfo() => gethProcessInfo;

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

                gethProcessInfo = new GEthProcessInfo(
                        hasExited: this.IsGethExited(),
                        process: geth,
                        gethReStarts: gethReStarts,
                        startTime: gethStart);

                return true;
            }
            catch (Exception ex)
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

                gethReStarts.Add(DateTime.UtcNow.Ticks);
                if(gethReStarts.Count > 24 * 3600)
                    gethReStarts.RemoveAt(0);

                geth.Start();
                geth.BeginOutputReadLine();
                geth.BeginErrorReadLine();
                gethStart = DateTime.UtcNow;

                _outputLog += $"\n\r*** GETH Started Running at {gethStart.ToLongDateTimeString()} ***";
                Console.WriteLine("GETH Process Was STARTED");
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start GETH node at location: '{_cfg.gethStartFileName ?? "undefined"}', error: {ex.JsonSerializeAsPrettyException()}");
                Thread.Sleep(1000);
            }
        }

        private void Geth_Exited(object sender, EventArgs e)
        {
            _outputLog += $"\n\r*** GETH Finished Running at {DateTime.UtcNow.ToLongDateTimeString()} ***";
            Console.WriteLine("GETH Process Was STOPPED");
        }

        private void Geth_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            var data = e.Data;

            if (data.IsNullOrEmpty())
                return;

            if (_cfg.gethEnableConsole)
                Console.WriteLine(data);

            _errorLog += data;

            if (_errorLog.Length > 1024 * 1024)
                _errorLog = _errorLog.Substring(data.Length, _errorLog.Length - data.Length);

            TryUpdateOutputLog(_cfg.gethErrorLog, data, _cfg.maxGethErrorLogSize);
        }

        private void Geth_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            var data = e.Data;

            if (data.IsNullOrEmpty())
                return;

            if (_cfg.gethEnableConsole)
                Console.WriteLine(data);

            _outputLog += data;

            if (_outputLog.Length > 1024 * 1024)
                _outputLog = _outputLog.Substring(data.Length, _outputLog.Length - data.Length);

            TryUpdateOutputLog(_cfg.gethOutputLog, data, _cfg.maxGethOutputLogSize);
        }

        private void TryUpdateOutputLog(string file, string data, int maxLength)
        {
            if (!_cfg.gethEnableLog || data.IsNullOrEmpty())
                return;

            try
            {
                var fi = new FileInfo(file);
                fi.AppendAllText(data);

                if (fi.Length > maxLength)
                    fi.TrimEnd((long)(maxLength * 0.1));
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Failed to save log data to '{file}', error: {ex.Message ?? "undefined"}");
            }
        }


        public (string output, string error) TryRestart()
        {
            return TryExecuteCommand(
                fileName: _cfg.restartCommand,
                arrguments: _cfg.restartArguments,
                readError: true,
                readOutput: true,
                waitForExit_ms: 1000,
                timeout: 4000);
        }

        public (string output, string error) TryExecuteCommand(
            string fileName, 
            string arrguments,
            bool readOutput,
            bool readError,
            int waitForExit_ms,
            int timeout)
        {
            string output = null;
            string error = null;
            try
            {
                var process = new Process();
                process.StartInfo.FileName = fileName;
                process.StartInfo.Arguments = arrguments;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();

                char[] buffer = new char[256];

                var tStartEnd = DateTime.UtcNow.AddSeconds(timeout);

                if (readOutput)
                {
                    Task<int> read = null;

                    while (true)
                    {
                        if (read == null)
                            read = process.StandardOutput.ReadAsync(buffer, 0, buffer.Length);

                        read.Wait(100); // an arbitray timeout

                        if (read.IsCompleted)
                        {
                            if (read.Result > 0)
                            {
                                output += new string(buffer, 0, read.Result);
                                read = null; // ok, this task completed so we need to create a new one
                                continue;
                            }

                            // got -1, process ended
                            break;
                        }

                        if(DateTime.UtcNow > tStartEnd)
                        {
                            error += $"TIME OUT ERROR, {timeout}s elsapsed\n\r";
                            break;
                        }
                    }
                }

                if (readError)
                {
                    Task<int> read = null;

                    while (true)
                    {
                        if (read == null)
                            read = process.StandardError.ReadAsync(buffer, 0, buffer.Length);

                        read.Wait(100); // an arbitray timeout

                        if (read.IsCompleted)
                        {
                            if (read.Result > 0)
                            {
                                error += new string(buffer, 0, read.Result);
                                read = null; // ok, this task completed so we need to create a new one
                                continue;
                            }

                            // got -1, process ended
                            break;
                        }

                        if (DateTime.UtcNow > tStartEnd)
                        {
                            error += $"TIME OUT ERROR, {timeout}s elsapsed\n\r";
                            break;
                        }
                    }

                }

                process.WaitForExit(waitForExit_ms);

                return (output, error);
            }
            catch(Exception ex)
            {
                if(error == null)
                    error = ex.JsonSerializeAsPrettyException();
                
                Console.WriteLine($"Failed to execute command '{fileName} {arrguments}', error: {ex.JsonSerializeAsPrettyException()}");
                return (output, error);
            }
        }
    }
}
