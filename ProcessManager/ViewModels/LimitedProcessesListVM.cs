using ProcessManager.Watchdog;
using ProcessManager.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using ProcessManager.Commands.LimitedProcessesListWindowCommands;
using System.Collections.Specialized;

namespace ProcessManager.ViewModels
{
    public class LimitedProcessesListVM : IDisposable
    {
        /// <summary>
        /// Informazioni sui limiti.
        /// </summary>
        public ObservableCollection<Tuple<string, byte>> LimitsData { get; }

        private Dispatcher WindowDispatcherValue;
        private bool disposedValue;

        /// <summary>
        /// Oggetto <see cref="Dispatcher"/> usato per aggiornare la lista.
        /// </summary>
        public Dispatcher WindowDispatcher
        {
            get => WindowDispatcherValue;
            set
            {
                if (WindowDispatcherValue is null)
                {
                    WindowDispatcherValue = value;
                }
            }
        }

        /// <summary>
        /// Comando per terminare un processo.
        /// </summary>
        public ICommand TerminateProcessCommand { get; }

        /// <summary>
        /// Comando per visualizzare informazioni su un limite.
        /// </summary>
        public ICommand ShowLimitInfoCommand { get; }


        public LimitedProcessesListVM()
        {
            TerminateProcessCommand = new TerminateProcessCommand();
            ShowLimitInfoCommand = new ShowLimitInfoCommand();
            LimitsData = new();
            foreach (CpuUsageLimitsData limit in ProcessLimiter.ProcessCpuLimits)
            {
                foreach (ProcessInfo info in limit.LimitedProcesses)
                {
                    LimitsData.Add(new Tuple<string, byte>(info.Name, limit.UsageLimit));
                }
            }
        }

        /// <summary>
        /// Esegue la sottoscrizione agli eventi di aggiornamento della collezione contenente le informazioni sui limiti.
        /// </summary>
        public void SubscribeToChangeEvents()
        {
            foreach (CpuUsageLimitsData limit in ProcessLimiter.ProcessCpuLimits)
            {
                limit.LimitedProcesses.CollectionChanged += LimitedProcesses_CollectionChanged;
            }
            ProcessLimiter.ProcessCpuLimits.CollectionChanged += ProcessCpuLimits_CollectionChanged;
        }

        private void LimitedProcesses_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action is NotifyCollectionChangedAction.Add)
            {
                CpuUsageLimitsData Limit;
                foreach (ProcessInfo info in e.NewItems)
                {
                    Limit = ProcessLimiter.ProcessCpuLimits.FirstOrDefault(limit => limit.ExecutablePaths.Contains(info.FullPath));
                    if (Limit is not null)
                    {
                        WindowDispatcher.Invoke(() => LimitsData.Add(new Tuple<string, byte>(info.Name, Limit.UsageLimit)));
                    }
                }
            }
            else if (e.Action is NotifyCollectionChangedAction.Remove)
            {
                Tuple<string, byte> RemovedProcess;
                foreach (ProcessInfo info in e.OldItems)
                {
                    RemovedProcess = LimitsData.FirstOrDefault(limit => limit.Item1 == info.FullPath);
                    if (RemovedProcess is not null)
                    {
                        _ = WindowDispatcher.Invoke(() => LimitsData.Remove(RemovedProcess));
                    }
                }
            }
        }

        private void ProcessCpuLimits_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action is NotifyCollectionChangedAction.Add)
            {
                foreach (CpuUsageLimitsData item in e.NewItems)
                {
                    item.LimitedProcesses.CollectionChanged += LimitedProcesses_CollectionChanged;
                }
            }
            else if (e.Action is NotifyCollectionChangedAction.Remove)
            {
                foreach (CpuUsageLimitsData item in e.OldItems)
                {
                    item.LimitedProcesses.CollectionChanged -= LimitedProcesses_CollectionChanged;
                    for (int i = 0; i < LimitsData.Count; i++)
                    {
                        if (LimitsData[i].Item2 == item.UsageLimit)
                        {
                            _ = WindowDispatcher.Invoke(() => LimitsData.Remove(LimitsData[i]));
                        }
                    }
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ProcessLimiter.ProcessCpuLimits.CollectionChanged -= ProcessCpuLimits_CollectionChanged;
                    foreach (CpuUsageLimitsData limit in ProcessLimiter.ProcessCpuLimits)
                    {
                        limit.LimitedProcesses.CollectionChanged -= LimitedProcesses_CollectionChanged;
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}