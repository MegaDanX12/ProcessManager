using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ProcessManager.Models
{
    /// <summary>
    /// Informazioni su un job.
    /// </summary>
    public class JobInfo : INotifyPropertyChanged
    {
        private TimeSpan UserTimeValue;

        /// <summary>
        /// Tempo di esecuzione in modalità utente del job.
        /// </summary>
        public TimeSpan UserTime
        {
            get => UserTimeValue;
            private set
            {
                if (UserTimeValue != value)
                {
                    UserTimeValue = value;
                    NotifyPropertyChanged(nameof(UserTime));
                }
            }
        }

        private TimeSpan KernelTimeValue;

        /// <summary>
        /// Tempo di esecuzione in modalità kernel del job.
        /// </summary>
        public TimeSpan KernelTime
        {
            get => KernelTimeValue;
            private set
            {
                if (KernelTimeValue != value)
                {
                    KernelTimeValue = value;
                    NotifyPropertyChanged(nameof(KernelTime));
                }
            }
        }

        private TimeSpan TotalTimeValue;

        /// <summary>
        /// Tempo totale di esecuzione del job.
        /// </summary>
        public TimeSpan TotalTime
        {
            get => TotalTimeValue;
            private set
            {
                if (TotalTimeValue != value)
                {
                    TotalTimeValue = value;
                    NotifyPropertyChanged(nameof(TotalTime));
                }
            }
        }

        private uint PageFaultCountValue;

        /// <summary>
        /// Numero totale di page fault incontrati.
        /// </summary>
        public uint PageFaultCount
        {
            get => PageFaultCountValue;
            private set
            {
                if (PageFaultCountValue != value)
                {
                    PageFaultCountValue = value;
                    NotifyPropertyChanged(nameof(PageFaultCount));
                }
            }
        }

        private uint TotalProcessesValue;

        /// <summary>
        /// Numero totale di processi che sono e sono stati associati al job.
        /// </summary>
        public uint TotalProcesses
        {
            get => TotalProcessesValue;
            private set
            {
                if (TotalProcessesValue != value)
                {
                    TotalProcessesValue = value;
                    NotifyPropertyChanged(nameof(TotalProcesses));
                }
            }
        }

        private uint ActiveProcessesValue;

        /// <summary>
        /// Numero di processi attualmente associati al job.
        /// </summary>
        public uint ActiveProcesses
        {
            get => ActiveProcessesValue;
            private set
            {
                if (ActiveProcessesValue != value)
                {
                    ActiveProcessesValue = value;
                    NotifyPropertyChanged(nameof(ActiveProcesses));
                }
            }
        }

        private uint TerminatedProcessesValue;

        /// <summary>
        /// Numero di processi terminati a causa di violazione dei limiti dalla creazione del job.
        /// </summary>
        public uint TerminatedProcesses
        {
            get => TerminatedProcessesValue;
            private set
            {
                if (TerminatedProcessesValue != value)
                {
                    TerminatedProcessesValue = value;
                    NotifyPropertyChanged(nameof(TerminatedProcesses));
                }
            }
        }

        private ulong ReadOperationCountValue;

        /// <summary>
        /// Numero di operazioni di lettura eseguite.
        /// </summary>
        public ulong ReadOperationCount
        {
            get => ReadOperationCountValue;
            private set
            {
                if (ReadOperationCountValue != value)
                {
                    ReadOperationCountValue = value;
                    NotifyPropertyChanged(nameof(ReadOperationCount));
                }
            }
        }

        private ulong WriteOperationCountValue;

        /// <summary>
        /// Numero di operazioni di scrittura eseguite.
        /// </summary>
        public ulong WriteOperationCount
        {
            get => WriteOperationCountValue;
            private set
            {
                if (WriteOperationCountValue != value)
                {
                    WriteOperationCountValue = value;
                    NotifyPropertyChanged(nameof(WriteOperationCount));
                }
            }
        }

        private ulong OtherOperationCountValue;

        /// <summary>
        /// Numero di operazioni di I/O eseguite, diverse da lettura e scrittura.
        /// </summary>
        public ulong OtherOperationCount
        {
            get => OtherOperationCountValue;
            private set
            {
                if (OtherOperationCountValue != value)
                {
                    OtherOperationCountValue = value;
                    NotifyPropertyChanged(nameof(OtherOperationCount));
                }
            }
        }

        private ulong ReadTransferCountValue;

        /// <summary>
        /// Numero di byte letti.
        /// </summary>
        public ulong ReadTransferCount
        {
            get => ReadTransferCountValue;
            private set
            {
                if (ReadTransferCountValue != value)
                {
                    ReadTransferCountValue = value;
                    NotifyPropertyChanged(nameof(ReadTransferCount));
                }
            }
        }

        private ulong WriteTransferCountValue;

        /// <summary>
        /// Numero di byte scritti.
        /// </summary>
        public ulong WriteTransferCount
        {
            get => WriteTransferCountValue;
            private set
            {
                if (WriteTransferCountValue != value)
                {
                    WriteTransferCountValue = value;
                    NotifyPropertyChanged(nameof(WriteTransferCount));
                }
            }
        }

        private ulong OtherTransferCountValue;

        /// <summary>
        /// Numero di byte trasferiti durante operazioni diverse da lettura e scrittura.
        /// </summary>
        public ulong OtherTransferCount
        {
            get => OtherTransferCountValue;
            private set
            {
                if (OtherTransferCountValue != value)
                {
                    OtherTransferCountValue = value;
                    NotifyPropertyChanged(nameof(OtherTransferCount));
                }
            }
        }

        /// <summary>
        /// Informazioni sull'attività dei processi associati al job.
        /// </summary>
        public List<JobAssociatedProcessActivityInfo> AssociatedProcessesInfo { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="JobInfo"/>.
        /// </summary>
        /// <param name="UserTime">Tempo utente del job.</param>
        /// <param name="KernelTime">Tempo kernel del job.</param>
        /// <param name="PageFaultCount">Numero di page fault incontrati dal job.</param>
        /// <param name="TotalProcesses">Numero totale di processi assegnati al job fin dalla sua creazione.</param>
        /// <param name="ActiveProcesses">Numero di processi attualmente associati al job.</param>
        /// <param name="TerminatedProcesses">Numero di processi terminati per violazione di limiti del job fin dalla sua creazione.</param>
        /// <param name="ReadOperationCount">Numero di operazioni di lettura eseguite dal job.</param>
        /// <param name="WriteOperationCount">Numero di operazioni di scrittura eseguite dal job.</param>
        /// <param name="OtherOperationCount">Numero di operazioni diverse da lettura e scrittura eseguite dal job.</param>
        /// <param name="ReadTransferCount">Numero di byte trasferiti in operazioni di lettura eseguite dal job.</param>
        /// <param name="WriteTransferCount">Numero di byte trasferiti in operazioni di scrittura eseguite dal job.</param>
        /// <param name="OtherTransferCount">Numero di byte trasferiti in operazioni diverse da lettura e scrittura eseguite dal job.</param>
        /// <param name="LimitedProcesses">Lista di istanze di <see cref="ProcessInfo"/> associate ai processi attualmente associati al job.</param>
        /// <param name="GetProcessesData">Indica se recuperare i dati sull'attività dei processi associati al job.</param>
        public JobInfo(TimeSpan UserTime, TimeSpan KernelTime, uint PageFaultCount, uint TotalProcesses, uint ActiveProcesses, uint TerminatedProcesses, ulong ReadOperationCount, ulong WriteOperationCount, ulong OtherOperationCount, ulong ReadTransferCount, ulong WriteTransferCount, ulong OtherTransferCount, List<ProcessInfo> LimitedProcesses, bool GetProcessesData)
        {
            UserTimeValue = UserTime;
            KernelTimeValue = KernelTime;
            TotalTimeValue = UserTimeValue + KernelTimeValue;
            PageFaultCountValue = PageFaultCount;
            TotalProcessesValue = TotalProcesses;
            ActiveProcessesValue = ActiveProcesses;
            TerminatedProcessesValue = TerminatedProcesses;
            ReadOperationCountValue = ReadOperationCount;
            WriteOperationCountValue = WriteOperationCount;
            OtherOperationCountValue = OtherOperationCount;
            ReadTransferCountValue = ReadTransferCount;
            WriteTransferCountValue = WriteTransferCount;
            OtherTransferCountValue = OtherTransferCount;
            /*if (GetProcessesData)
            {
                AssociatedProcessesInfo = new();
                foreach (ProcessInfo process in LimitedProcesses)
                {
                    AssociatedProcessesInfo.Add(new((ulong)UserTime.Ticks, (ulong)KernelTime.Ticks, PageFaultCount, process, ReadOperationCount, WriteOperationCount, OtherOperationCount, ReadTransferCount, WriteTransferCount, OtherTransferCount));
                }
            }*/
        }

        /// <summary>
        /// Aggiorna i dati sul job.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="JobInfo"/> con le nuove informazioni.</param>
        public void UpdateData(JobInfo Info)
        {
            UserTime = Info.UserTime;
            KernelTime = Info.KernelTime;
            TotalTime = Info.UserTime + Info.KernelTime;
            PageFaultCount = Info.PageFaultCount;
            TotalProcesses = Info.TotalProcesses;
            ActiveProcesses = Info.ActiveProcesses;
            TerminatedProcesses = Info.TerminatedProcesses;
            ReadOperationCount = Info.ReadOperationCount;
            WriteOperationCount = Info.WriteOperationCount;
            OtherOperationCount = Info.OtherOperationCount;
            ReadTransferCount = Info.ReadTransferCount;
            WriteTransferCount = Info.WriteTransferCount;
            OtherTransferCount = Info.OtherTransferCount;
        }

        private void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}