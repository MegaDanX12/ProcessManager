using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Win32.SafeHandles;
using static ProcessManager.NativeHelpers;

namespace ProcessManager.Models
{
    /// <summary>
    /// Contiene informazioni su un thread di un processo.
    /// </summary>
    public class ThreadInfo : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Handle nativo al thread.
        /// </summary>
        private readonly IntPtr ThreadHandle;

        /// <summary>
        /// ID univoco del thread.
        /// </summary>
        public uint TID { get; }

        /// <summary>
        /// Priorità di base.
        /// </summary>
        public string BasePriority { get; }

        /// <summary>
        /// Data e ora di creazione.
        /// </summary>
        public string CreationTime { get; }

        private string KernelTimeValue;

        /// <summary>
        /// Tempo di esecuzione kernel.
        /// </summary>
        public string KernelTime 
        {
            get
            {
                return KernelTimeValue;
            }
            private set
            {
                if (KernelTimeValue != value)
                {
                    KernelTimeValue = value;
                    NotifyPropertyChanged(nameof(KernelTime));
                }
            }
        }

        private string UserTimeValue;

        /// <summary>
        /// Tempo di esecuzione utente.
        /// </summary>
        public string UserTime 
        {
            get
            {
                return UserTimeValue;
            }
            private set
            {
                if (UserTimeValue != value)
                {
                    UserTimeValue = value;
                    NotifyPropertyChanged(nameof(UserTime));
                }
            }
        }

        private string CycleTimeValue;

        /// <summary>
        /// Numero di cicli di esecuzione.
        /// </summary>
        public string CycleTime 
        {
            get
            {
                return CycleTimeValue;
            }
            private set
            {
                if (CycleTimeValue != value)
                {
                    CycleTimeValue = value;
                    NotifyPropertyChanged(nameof(CycleTime));
                }
            }
        }

        private string PriorityValue;

        /// <summary>
        /// Priorità.
        /// </summary>
        public string Priority 
        {
            get
            {
                return PriorityValue;
            }
            private set
            {
                if (PriorityValue != value)
                {
                    PriorityValue = value;
                    NotifyPropertyChanged(nameof(Priority));
                }
            }
        }

        private ulong AffinityValue;

        /// <summary>
        /// Priorità.
        /// </summary>
        public ulong Affinity
        {
            get
            {
                return AffinityValue;
            }
            private set
            {
                if (AffinityValue != value)
                {
                    AffinityValue = value;
                    NotifyPropertyChanged(nameof(Affinity));
                }
            }
        }

        /// <summary>
        /// Indirizzo di partenza.
        /// </summary>
        public string StartAddress { get; }

        private string DynamicPriorityValue;

        /// <summary>
        /// Priorità dinamica.
        /// </summary>
        public string DynamicPriority 
        {
            get
            {
                return DynamicPriorityValue;
            }
            private set
            {
                if (DynamicPriorityValue != value)
                {
                    DynamicPriorityValue = value;
                    NotifyPropertyChanged(nameof(DynamicPriority));
                }
            }
        }

        private string IdealProcessorValue;
        private bool disposedValue;

        /// <summary>
        /// Processore ideale.
        /// </summary>
        public string IdealProcessor 
        {
            get
            {
                return IdealProcessorValue;
            }
            private set
            {
                if (IdealProcessorValue != value)
                {
                    IdealProcessorValue = value;
                    NotifyPropertyChanged(nameof(IdealProcessor));
                }
            }
        }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="ThreadInfo"/>.
        /// </summary>
        /// <param name="Handle">Handle nativo al thread, usato per aggiornare i dati.</param>
        /// <param name="TID">ID del thread.</param>
        /// <param name="BasePriority">Priorità base.</param>
        /// <param name="CycleTime">Numero di cicli di esecuzione.</param>
        /// <param name="CreationTime">Data e ora di creazione.</param>
        /// <param name="KernelTime">Tempo di esecuzione kernel.</param>
        /// <param name="UserTime">Tempo di esecuzione utente.</param>
        /// <param name="StartAddress">Indirizzo di partenza.</param>
        /// <param name="Priority">Priorità.</param>
        /// <param name="Affinity">Affinità del thread.</param>
        /// <param name="DynamicPriority">Priorità dinamica.</param>
        /// <param name="IdealProcessor">Processore ideale.</param>
        public ThreadInfo(IntPtr Handle, uint TID, string BasePriority, string CycleTime, string CreationTime, string KernelTime, string UserTime, string StartAddress, string Priority, ulong Affinity, string DynamicPriority, string IdealProcessor)
        {
            ThreadHandle = Handle;
            this.TID = TID;
            this.BasePriority = BasePriority;
            CycleTimeValue = CycleTime;
            this.CreationTime = CreationTime;
            KernelTimeValue = KernelTime;
            UserTimeValue = UserTime;
            this.StartAddress = StartAddress;
            IdealProcessorValue = IdealProcessor;
            PriorityValue = Priority;
            AffinityValue = Affinity;
            DynamicPriorityValue = DynamicPriority;
        }

        /// <summary>
        /// Aggiorna i dati sul thread.
        /// </summary>
        public void Update()
        {
            ThreadInfo NewInfo = GetThreadDynamicInfo(Convert.ToUInt32(TID, CultureInfo.CurrentCulture));
            CycleTime = NewInfo.CycleTime;
            KernelTime = NewInfo.KernelTime;
            UserTime = NewInfo.UserTime;
            IdealProcessor = NewInfo.IdealProcessor;
            Priority = NewInfo.Priority;
            Affinity = NewInfo.Affinity;
            DynamicPriority = NewInfo.DynamicPriority;
            NewInfo.Dispose();
        }

        /// <summary>
        /// Recupera informazioni sulle finestre associate al thread.
        /// </summary>
        /// <returns>Un array di istanze di <see cref="WindowInfo"/> contenente le informazioni.</returns>
        public WindowInfo[] GetWindowsInfo()
        {
            uint PID = GetThreadAssociatedProcessID();
            if (PID != 0)
            {
                using SafeProcessHandle Handle = GetProcessHandle(PID);
                return GetThreadWindows(ThreadHandle, Handle);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Termina il thread.
        /// </summary>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool TerminateThread()
        {
            uint AssociatedProcessID = GetThreadAssociatedProcessID();
            if (AssociatedProcessID != 0)
            {
                using SafeProcessHandle ProcessHandle = GetProcessHandle(AssociatedProcessID);
                if (NativeHelpers.TerminateThread(ProcessHandle, ThreadHandle))
                {
                    LogEntry Entry = BuildLogEntryForInformation("Thread terminato", EventAction.ThreadTermination, ProcessHandle);
                    Logger.WriteEntry(Entry);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile terminare un thread, ID del processo non disponibile", EventAction.ThreadTermination, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Sospende l'esecuzione del thread.
        /// </summary>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool SuspendThread()
        {
            uint AssociatedProcessID = GetThreadAssociatedProcessID();
            if (AssociatedProcessID != 0)
            {
                using SafeProcessHandle ProcessHandle = GetProcessHandle(AssociatedProcessID);
                if (NativeHelpers.SuspendThread(ProcessHandle, ThreadHandle))
                {
                    LogEntry Entry = BuildLogEntryForInformation("Thread sospeso, TID: " + TID, EventAction.ThreadSuspension, ProcessHandle);
                    Logger.WriteEntry(Entry);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile sospendere un thread, ID del processo non disponibile", EventAction.ThreadSuspension, null);
                Logger.WriteEntry(Entry);
                return false;
            }
            
        }

        /// <summary>
        /// Riprende l'esecuzione del thread.
        /// </summary>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool ResumeThread()
        {
            uint AssociatedProcessID = GetThreadAssociatedProcessID();
            if (AssociatedProcessID != 0)
            {
                using SafeProcessHandle ProcessHandle = GetProcessHandle(AssociatedProcessID);
                if  (NativeHelpers.ResumeThread(ProcessHandle, ThreadHandle))
                {
                    LogEntry Entry = BuildLogEntryForInformation("Attività di un thread ripresa, TID: " + TID, EventAction.ThreadResume, ProcessHandle);
                    Logger.WriteEntry(Entry);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile riprendere l'attivita di un thread, ID del processo non disponibile", EventAction.ThreadResume, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Imposta la priorità di un thread.
        /// </summary>
        /// <param name="NewPriority">Nuova priorità da impostare.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool SetThreadPriority(string NewPriority)
        {
            uint AssociatedProcessID = GetThreadAssociatedProcessID();
            if (AssociatedProcessID != 0)
            {
                using SafeProcessHandle ProcessHandle = GetProcessHandle(AssociatedProcessID);
                if (NativeHelpers.SetThreadPriority(ProcessHandle, NewPriority, ThreadHandle))
                {
                    LogEntry Entry = BuildLogEntryForInformation("Cambiata priorità di un thread, TID: " + TID + ", nuova priorità: " + NewPriority, EventAction.ThreadPropertiesManipulation, ProcessHandle);
                    Logger.WriteEntry(Entry);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile impostare la priorità di un thread, ID del processo non disponibile", EventAction.ThreadPropertiesManipulation, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Recupera l'ID del processo associato al thread.
        /// </summary>
        /// <returns>L'ID del processo, 0 in caso di errore.</returns>
        public uint GetThreadAssociatedProcessID()
        {
            return NativeMethods.Win32ProcessFunctions.GetProcessIdOfThread(ThreadHandle);
        }

        private void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (ThreadHandle != IntPtr.Zero)
                {
                    CloseHandle(ThreadHandle);
                }
                disposedValue = true;
            }
        }

        ~ThreadInfo()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}