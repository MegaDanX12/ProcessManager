using ProcessManager.Commands.LimitInfoWindowCommands;
using ProcessManager.Models;
using ProcessManager.Watchdog;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;

namespace ProcessManager.ViewModels
{
    public class JobInfoVM : INotifyPropertyChanged
    {
        /// <summary>
        /// Informazioni sul job.
        /// </summary>
        public JobInfo JobData { get; }

        private JobAssociatedProcessActivityInfo CurrentProcessDataValue;

        /// <summary>
        /// Informazioni su un processo limitato incluso nel job.
        /// </summary>
        public JobAssociatedProcessActivityInfo CurrentProcessData
        {
            get => CurrentProcessDataValue;
            private set
            {
                if (CurrentProcessDataValue != value)
                {
                    CurrentProcessDataValue = value;
                    NotifyPropertyChanged(nameof(CurrentProcessData));
                }
            }
        }

        /// <summary>
        /// Indice dell'oggetto attuale della lista.
        /// </summary>
        private int CurrentIndex;

        private bool IsFirstProcessDataValue;

        /// <summary>
        /// Indica se i dati del processo attualmente mostrati sono quelli del primo della lista.
        /// </summary>
        public bool IsFirstProcessData
        {
            get => IsFirstProcessDataValue;
            set
            {
                if (IsFirstProcessDataValue != value)
                {
                    IsFirstProcessDataValue = value;
                    NotifyPropertyChanged(nameof(IsFirstProcessData));
                }
            }
        }

        private bool IsLastProcessDataValue;

        /// <summary>
        /// Indica se i dati del processo attualmente mostrati sono quelli dell'ultimo della lista.
        /// </summary>
        public bool IsLastProcessData
        {
            get => IsLastProcessDataValue;
            set
            {
                if (IsLastProcessDataValue != value)
                {
                    IsLastProcessDataValue = value;
                    NotifyPropertyChanged(nameof(IsLastProcessData));
                }
            }
        }

        /// <summary>
        /// Comando per visualizzare le informazioni sul prossimo processo nella lista.
        /// </summary>
        public ICommand NextProcessCommand { get; }

        /// <summary>
        /// Comando per visualizzare le informazioni sul processo precedente nella lista.
        /// </summary>
        public ICommand FormerProcessCommand { get; }


        private readonly object LockObject = new();


        /// <summary>
        /// Timer per l'aggiornamento dei dati.
        /// </summary>
        private readonly Timer UpdateTimer;

        /// <summary>
        /// Limite a cui è associato il job.
        /// </summary>
        private readonly CpuUsageLimitsData Limit;

        public JobInfoVM(CpuUsageLimitsData LimitData)
        {
            Limit = LimitData;
            JobData = LimitData.GetAssociatedJobData();
            if (JobData.AssociatedProcessesInfo.Count > 0)
            {
                CurrentProcessDataValue = JobData.AssociatedProcessesInfo[0];
                CurrentIndex = 0;
                IsFirstProcessDataValue = true;
                IsLastProcessDataValue = false;
            }
            else
            {
                CurrentProcessDataValue = null;
                CurrentIndex = -1;
                IsFirstProcessDataValue = true;
                IsLastProcessDataValue = true;
            }
            UpdateTimer = new((state) => UpdateData(), null, 1000, Timeout.Infinite);
            LimitData.LimitedProcesses.CollectionChanged += LimitedProcesses_CollectionChanged;
            NextProcessCommand = new NextProcessCommand(this);
            FormerProcessCommand = new FormerProcessCommand(this);
        }

        /// <summary>
        /// Aggiorna i dati sul job.
        /// </summary>
        private void UpdateData()
        {
            JobInfo NewInfo = Limit.UpdateJobData();
            JobData.UpdateData(NewInfo);
            _ = UpdateTimer.Change(1000, Timeout.Infinite);
        }

        private void LimitedProcesses_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ProcessTimesAndIOData Data;
            lock (LockObject)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        if (CurrentIndex + 1 == e.NewStartingIndex)
                        {
                            IsLastProcessData = false;
                        }
                        foreach (ProcessInfo process in e.NewItems)
                        {
                            Data = process.GetProcessTimesAndIOData();
                            JobData.AssociatedProcessesInfo.Add(new((ulong)JobData.UserTime.Ticks, (ulong)JobData.KernelTime.Ticks, JobData.PageFaultCount, process, JobData.ReadOperationCount, JobData.WriteOperationCount, JobData.OtherOperationCount, JobData.ReadTransferCount, JobData.WriteTransferCount, JobData.OtherTransferCount));
                        }
                        if (CurrentIndex is -1)
                        {
                            CurrentProcessData = JobData.AssociatedProcessesInfo[0];
                            IsFirstProcessData = true;
                            if (JobData.AssociatedProcessesInfo.Count is 1)
                            {
                                IsLastProcessData = true;
                            }
                            else
                            {
                                IsLastProcessData = false;
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        if (e.OldStartingIndex == CurrentIndex)
                        {
                            CurrentIndex = 0;
                            CurrentProcessData = JobData.AssociatedProcessesInfo[CurrentIndex];
                            IsFirstProcessData = true;
                            IsLastProcessData = false;
                        }
                        foreach (ProcessInfo process in e.OldItems)
                        {
                            JobAssociatedProcessActivityInfo ProcessData = JobData.AssociatedProcessesInfo.Find(processdata => processdata.ProcessName == process.Name);
                            _ = JobData.AssociatedProcessesInfo.Remove(ProcessData);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Visualizza i dati del prossimo processo nella lista.
        /// </summary>
        public void NextProcess()
        {
            lock (LockObject)
            {
                if (CurrentIndex != JobData.AssociatedProcessesInfo.Count - 1)
                {
                    CurrentIndex += 1;
                    CurrentProcessData = JobData.AssociatedProcessesInfo[CurrentIndex];
                    IsLastProcessData = CurrentIndex == JobData.AssociatedProcessesInfo.Count - 1;
                }
            }
        }

        /// <summary>
        /// Visualizza i dati del processo precedente nella lista.
        /// </summary>
        public void FormerProcess()
        {
            lock (LockObject)
            {
                if (CurrentIndex != 0)
                {
                    CurrentIndex -= 1;
                    CurrentProcessData = JobData.AssociatedProcessesInfo[CurrentIndex];
                    IsFirstProcessData = CurrentIndex == 0;
                }
            }
        }

        private void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}