using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using TaskManager.Annotations;

namespace TaskManager
{
    public class ViewModel : INotifyPropertyChanged
    {
        public ViewModel()
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            timer.Tick += UpdateProcesses;
            timer.Start();

            // Initialize priority levels
            PriorityLevels = new ObservableCollection<string>
            {
                "Idle",
                "Below Normal",
                "Normal",
                "Above Normal",
                "High",
                "Real Time"
            };

            SelectedPriority = "Normal"; // Default priority
        }

        private Process _selectedProcess;
        public Process SelectedProcess
        {
            get => _selectedProcess;
            set
            {
                _selectedProcess = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ProcessListItem> Processes { get; } = new ObservableCollection<ProcessListItem>();

        public ObservableCollection<string> PriorityLevels { get; } // List of priority levels

        private string _selectedPriority;
        public string SelectedPriority
        {
            get => _selectedPriority;
            set
            {
                _selectedPriority = value;
                OnPropertyChanged();
            }
        }

        public void UpdateProcesses(object sender, EventArgs e)
        {
            var currentIds = Processes.Select(p => p.Id).ToList();
            var runningProcesses = Process.GetProcesses();

            foreach (var process in runningProcesses)
            {
                if (!currentIds.Remove(process.Id)) // It's a new process
                {
                    Processes.Add(new ProcessListItem(process));
                }
            }

            // Remove processes that no longer exist
            foreach (var id in currentIds)
            {
                var processToRemove = Processes.FirstOrDefault(p => p.Id == id);
                if (processToRemove != null)
                {
                    Processes.Remove(processToRemove);
                }
            }

            // Refresh memory, CPU, and runtime
            foreach (var processItem in Processes)
            {
                OnPropertyChanged(nameof(processItem.MemoryUsage));
                OnPropertyChanged(nameof(processItem.CpuUsage));
                OnPropertyChanged(nameof(processItem.RunTime));
            }
        }

        public void ChangePriority()
        {
            if (SelectedProcess == null || string.IsNullOrEmpty(SelectedPriority)) return;

            try
            {
                ProcessPriorityClass priority = (ProcessPriorityClass)Enum.Parse(typeof(ProcessPriorityClass), SelectedPriority.Replace(" ", ""));
                SelectedProcess.PriorityClass = priority;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error changing priority: {ex.Message}");
            }
        }

        public void KillSelectedProcess()
        {
            try
            {
                SelectedProcess?.Kill();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error killing process: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
