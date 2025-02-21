using System;
using System.Diagnostics;
using System.Management;

namespace TaskManager
{
    public class ProcessListItem
    {
        private readonly PerformanceCounter _cpuCounter;

        public int Id => Process.Id;
        public string ProcessName => Process.ProcessName;
        public bool KeepAlive { get; set; }
        public Process Process { get; }

        public string MemoryUsage => $"{Process.WorkingSet64 / 1024 / 1024} MB";

        public string CpuUsage
        {
            get
            {
                try
                {
                    return $"{_cpuCounter?.NextValue():F2} %";
                }
                catch
                {
                    return "N/A";
                }
            }
        }

        public string RunTime
        {
            get
            {
                try
                {
                    return (DateTime.Now - Process.StartTime).ToString(@"hh\:mm\:ss");
                }
                catch
                {
                    return "N/A";
                }
            }
        }

        public ProcessListItem(Process process)
        {
            Process = process;

            // Initialize CPU Usage Counter
            _cpuCounter = new PerformanceCounter("Process", "% Processor Time", process.ProcessName, true);
            _cpuCounter.NextValue(); // The first call always returns 0, so we call it once before usage.
        }
    }
}
