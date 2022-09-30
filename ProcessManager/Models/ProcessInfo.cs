using Microsoft.Win32.SafeHandles;
using ProcessManager.InfoClasses.ProcessStatisticsClasses;
using ProcessManager.WMI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace ProcessManager.Models
{
    /// <summary>
    /// Informazioni su un processo.
    /// </summary>
    public class ProcessInfo : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// ID univoco del processo.
        /// </summary>
        public uint PID { get; }

        /// <summary>
        /// Priorità del processo.
        /// </summary>
        private ProcessPriority PriorityValue;

        /// <summary>
        /// Priorità del processo.
        /// </summary>
        public ProcessPriority Priority
        {
            get => PriorityValue;
            private set
            {
                if (PriorityValue != value)
                {
                    PriorityValue = value;
                    NotifyPropertyChanged(nameof(Priority));
                }
            }
        }

        /// <summary>
        /// Memoria privata del processo.
        /// </summary>
        private string PrivateMemoryValue;

        /// <summary>
        /// Memoria privata del processo.
        /// </summary>
        public string PrivateMemory
        {
            get => PrivateMemoryValue;
            private set
            {
                if (PrivateMemoryValue != value)
                {
                    PrivateMemoryValue = value;
                    NotifyPropertyChanged(nameof(PrivateMemory));
                }
            }
        }

        /// <summary>
        /// Memoria privata del processo, in bytes.
        /// </summary>
        public ulong PrivateMemoryBytes { get; private set; }

        /// <summary>
        /// Nome del processo.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Data e ora di avvio del processo.
        /// </summary>
        public string StartTime { get; }

        /// <summary>
        /// Data e ora di avvio del processo (uso interno).
        /// </summary>
        public DateTime CreationTime { get; }

        /// <summary>
        /// Numero di thread del processo.
        /// </summary>
        private uint NumThreadsValue;

        /// <summary>
        /// Numero di thread del processo.
        /// </summary>
        public uint NumThreads
        {
            get => NumThreadsValue;
            private set
            {
                if (NumThreadsValue != value)
                {
                    NumThreadsValue = value;
                    NotifyPropertyChanged(nameof(NumThreads));
                }
            }
        }

        /// <summary>
        /// Percentuale di uso del processore.
        /// </summary>
        private decimal ProcessorUsageValue;

        /// <summary>
        /// Percentuale di uso del processore.
        /// </summary>
        public decimal ProcessorUsage
        {
            get => ProcessorUsageValue;
            private set
            {
                if (ProcessorUsageValue != value)
                {
                    ProcessorUsageValue = value;
                    NotifyPropertyChanged(nameof(ProcessorUsage));
                }
            }
        }

        /// <summary>
        /// Affinità del processo.
        /// </summary>
        private ulong AffinityValue;

        /// <summary>
        /// Affinità del processo.
        /// </summary>
        public ulong Affinity
        {
            get => AffinityValue;
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
        /// Descrizione dell'eseguibile.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Percorso completo.
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// Versione.
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Linea di comando.
        /// </summary>
        public string CommandLine { get; }

        /// <summary>
        /// Società che ha prodotto l'applicazione.
        /// </summary>
        public string CompanyName { get; }

        /// <summary>
        /// Nome dell'interfaccia COM.
        /// </summary>
        /// <remarks>Questo campo è utilizzato solo per processi dllhost.exe.</remarks>
        public string ComInterfaceName { get; }

        /// <summary>
        /// Nome del pacchetto che contiene l'applicazione.
        /// </summary>
        public string PackageName { get; }

        /// <summary>
        /// Indica se il processo è in esecuzione come amministratore.
        /// </summary>
        public bool? IsProcessElevated { get; }

        /// <summary>
        /// Indica se il processo fa parte di un job.
        /// </summary>
        private bool? IsProcessInJobValue;

        /// <summary>
        /// Indica se il processo fa parte di un job.
        /// </summary>
        public bool? IsProcessInJob
        {
            get => IsProcessInJobValue;
            private set
            {
                if (IsProcessInJobValue != value)
                {
                    IsProcessInJobValue = value;
                    NotifyPropertyChanged(nameof(IsProcessInJob));
                }
            }
        }

        /// <summary>
        /// indica se il processo è a 32 bit.
        /// </summary>
        public bool? Is32BitProcess { get; }

        /// <summary>
        /// Handle del processo a cui questa istanza fa riferimento.
        /// </summary>
        private readonly SafeProcessHandle AssociatedProcessHandle;

        /// <summary>
        /// Evento che notifica dell'aggiornamento di una proprietà.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Valore di priorità di un processo.
        /// </summary>
        public enum ProcessPriority
        {
            Unknown = 0,
            AboveNormal = 32768,
            BelowNormal = 16384,
            High = 128,
            Idle = 64,
            Normal = 32,
            RealTime = 256
        }
        #region Process Processor Usage Calculation Utility Fields
        /// <summary>
        /// Tempo precedente di esecuzione del processo.
        /// </summary>
        private decimal FormerProcessTime;

        /// <summary>
        /// Tempo di inattività precedente del processore.
        /// </summary>
        private decimal PreviousProcessorIdleTime;

        /// <summary>
        /// Tempo utente precedente del processore.
        /// </summary>
        private decimal PreviousProcessorUserTime;

        /// <summary>
        /// Tempo totale precedente del processore.
        /// </summary>
        private decimal PreviousProcessorTotalTime;
        #endregion
        private bool disposedValue;
        #region Process Limiter Utility Fields
        /// <summary>
        /// Numero di tick al momento dell'aggiunta del processo a un job del limitatore processi.
        /// </summary>
        public ulong? ProcessLimiterJobAddTicksCount { get; set; }

        /// <summary>
        /// Numero di page fault incontrati al momento dell'aggiunta del processo a un job del limitatore processi.
        /// </summary>
        public uint? ProcessLimiterJobAddPageFaultCount { get; set; }

        /// <summary>
        /// Numero di operazioni di lettura eseguite al momento dell'aggiunta del processo a un job del limitatore processi.
        /// </summary>
        public ulong? ProcessLimiterJobAddReadOperationCount { get; set; }

        /// <summary>
        /// Numero di operazioni di scrittura eseguite al momento dell'aggiunta del processo a un job del limitatore processi.
        /// </summary>
        public ulong? ProcessLimiterJobAddWriteOperationCount { get; set; }

        /// <summary>
        /// Numero di operazioni diverse da lettura e scrittura eseguite al momento dell'aggiunta del processo a un job del limitatore processi.
        /// </summary>
        public ulong? ProcessLimiterJobAddOtherOperationCount { get; set; }

        /// <summary>
        /// Numero di byte letti al momento dell'aggiunta del processo a un job del limitatore processi.
        /// </summary>
        public ulong? ProcessLimiterJobAddReadBytes { get; set; }

        /// <summary>
        /// Numero di byte scritti al momento dell'aggiunta del processo a un job del limitatore processi.
        /// </summary>
        public ulong? ProcessLimiterJobAddWriteBytes { get; set; }

        /// <summary>
        /// Numero di byte trasferiti in operazioni diverse da lettura e scrittura al momento dell'aggiunta del processo a un job del limitatore processi.
        /// </summary>
        public ulong? ProcessLimiterJobAddOtherBytes { get; set; }
        #endregion
        /// <summary>
        /// Evento che indica la chiusura del processo.
        /// </summary>
        public event EventHandler Exit;

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="ProcessInfo"/> recuperando i dati del processo a cui l'handle fornito fa riferimento.
        /// </summary>
        /// <param name="ProcessHandle">Handle del processo di cui raccogliere i dati.</param>
        /// <param name="Name">Nome del processo.</param>
        /// <param name="PID">ID del processo.</param>
        /// <param name="ThreadCount">Conteggio thread.</param>
        /// <param name="CommandLine">Linea di comando.</param>
        /// <param name="FullPath">Percorso completo dell'eseguibile del processo.</param>
        public ProcessInfo(SafeProcessHandle ProcessHandle, uint PID = uint.MaxValue, uint ThreadCount = uint.MaxValue, string Name = null, string CommandLine = null, string FullPath = null)
        {
            AssociatedProcessHandle = ProcessHandle;
            this.PID = PID != uint.MaxValue ? PID : NativeHelpers.GetProcessPID(ProcessHandle);
            this.FullPath = string.IsNullOrWhiteSpace(FullPath) ? NativeHelpers.GetProcessFullPathNT(ProcessHandle) : FullPath;
            CompanyName = NativeHelpers.GetExecutableCompanyName(ProcessHandle, FullPath);
            Version = NativeHelpers.GetProcessVersion(ProcessHandle);
            this.CommandLine = string.IsNullOrWhiteSpace(CommandLine) ? NativeHelpers.GetProcessCommandLine2(ProcessHandle) : CommandLine;
            PackageName = NativeHelpers.GetApplicationPackageName(ProcessHandle);
            IsProcessElevated = NativeHelpers.GetTokenElevationStatus(NativeHelpers.GetProcessTokenHandle(ProcessHandle));
            IsProcessInJobValue = NativeHelpers.IsProcessInJob(ProcessHandle);
            Is32BitProcess = NativeHelpers.IsProcess32Bit(ProcessHandle);
            if (!string.IsNullOrWhiteSpace(Name))
            {
                this.Name = Name == "[System Process]" ? "System Idle Process" : Name;
            }
            else
            {
                this.Name = !string.IsNullOrWhiteSpace(FullPath) ? Path.GetFileName(FullPath) : Properties.Resources.UnavailableText;
            }
            if ((this.Name.Contains("dllhost") || this.Name.Contains("Dllhost")) && (this.FullPath.Contains("System32") || this.FullPath.Contains("system32")))
            {
                if (this.CommandLine.Contains("/Processid:"))
                {
                    int FirstParenthesisIndex = this.CommandLine.IndexOf("{", StringComparison.CurrentCulture);
                    string AppID = this.CommandLine.Substring(FirstParenthesisIndex);
                    ComInterfaceName = NativeHelpers.GetComInterfaceName(AppID);
                }
            }
            Description = NativeHelpers.GetProcessDescription(ProcessHandle);
            if (string.IsNullOrWhiteSpace(Description))
            {
                Description = Properties.Resources.UnavailableText;
            }
            PriorityValue = (ProcessPriority)NativeHelpers.GetProcessPriority(ProcessHandle);
            ulong PrivateMemoryValue = NativeHelpers.GetProcessPrivateBytes(ProcessHandle);
            double CalculatedValue;
            if (PrivateMemoryValue is >= 1048576 and < 1073741824)
            {
                CalculatedValue = (double)PrivateMemoryValue / 1024 / 1024;
                this.PrivateMemoryValue = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " MB";
            }
            else if (PrivateMemoryValue >= 1073741824)
            {
                CalculatedValue = (double)PrivateMemoryValue / 1024 / 1024 / 1024;
                this.PrivateMemoryValue = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " GB";
            }
            else if (PrivateMemoryValue is < 1048576 and > 1024)
            {
                CalculatedValue = (double)PrivateMemoryValue / 1024;
                this.PrivateMemoryValue = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " KB";
            }
            else
            {
                this.PrivateMemoryValue = PrivateMemoryValue.ToString("N2", CultureInfo.CurrentCulture) + " B";
            }
            PrivateMemoryBytes = PrivateMemoryValue;
            if (PID != 0)
            {
                DateTime? CreationTime = NativeHelpers.GetProcessStartTime(ProcessHandle);
                if (CreationTime.HasValue)
                {
                    this.CreationTime = CreationTime.Value;
                    StartTime = this.CreationTime.ToString(CultureInfo.CurrentCulture);
                }
                else
                {
                    StartTime = Properties.Resources.UnavailableText;
                }
                
            }
            else
            {
                StartTime = Properties.Resources.UnavailableText;
            }
            if (this.PID is 0)
            {
                decimal? CalculatedProcessorUsage = NativeHelpers.GetProcessorIdlePercentage(ref PreviousProcessorIdleTime, ref PreviousProcessorUserTime, ref PreviousProcessorTotalTime);
                ProcessorUsageValue = CalculatedProcessorUsage.HasValue ? decimal.Truncate(100 * CalculatedProcessorUsage.Value) / 100 : 0.00M;
            }
            else if (this.Name != Properties.Resources.UnavailableText)
            {
                decimal? CalculatedProcessorUsage = NativeHelpers.GetProcessProcessorUsage(ProcessHandle, ref FormerProcessTime);
                ProcessorUsageValue = !CalculatedProcessorUsage.HasValue ? 0.00M : decimal.Truncate(100 * CalculatedProcessorUsage.Value) / 100;
            }
            ulong? Affinity = NativeHelpers.GetProcessAffinity(ProcessHandle);
            AffinityValue = Affinity ?? 0;
            NumThreadsValue = ThreadCount != uint.MaxValue ? ThreadCount : NativeHelpers.GetProcessThreadCount(PID);
        }

        /// <summary>
        /// Aggiorna le informazioni su un processo.
        /// </summary>
        public void Refresh()
        {
            UpdateProcessorUsageValue();
            Priority = (ProcessPriority)NativeHelpers.GetProcessPriority(AssociatedProcessHandle);
            ulong PrivateMemoryValue = NativeHelpers.GetProcessPrivateBytes(AssociatedProcessHandle);
            double CalculatedValue;
            if (PrivateMemoryValue is >= 1048576 and < 1073741824)
            {
                CalculatedValue = (double)PrivateMemoryValue / 1024 / 1024;
                PrivateMemory = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " MB";
            }
            else if (PrivateMemoryValue >= 1073741824)
            {
                CalculatedValue = (double)PrivateMemoryValue / 1024 / 1024 / 1024;
                PrivateMemory = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " GB";
            }
            else if (PrivateMemoryValue < 1048576)
            {
                CalculatedValue = (double)PrivateMemoryValue / 1024;
                PrivateMemory = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " KB";
            }
            PrivateMemoryBytes = PrivateMemoryValue;
            ulong? Affinity = NativeHelpers.GetProcessAffinity(AssociatedProcessHandle);
            this.Affinity = Affinity ?? 0;
            IsProcessInJob = NativeHelpers.IsProcessInJob(AssociatedProcessHandle);
        }

        /// <summary>
        /// Aggiorna i valori relativi all'utilizzo del processore da parte del processo.
        /// </summary>
        private void UpdateProcessorUsageValue()
        {
            if (PID == 0)
            {
                decimal? CalculatedProcessorUsage = NativeHelpers.GetProcessorIdlePercentage(ref PreviousProcessorIdleTime, ref PreviousProcessorUserTime, ref PreviousProcessorTotalTime);
                ProcessorUsage = CalculatedProcessorUsage.HasValue ? decimal.Truncate(100 * CalculatedProcessorUsage.Value) / 100 : 0.00M;
            }
            else
            {
                decimal? ProcessorUsage = NativeHelpers.GetProcessProcessorUsage(AssociatedProcessHandle, ref FormerProcessTime);
                this.ProcessorUsage = !ProcessorUsage.HasValue ? 0.00M : decimal.Truncate(100 * ProcessorUsage.Value) / 100;
            }
        }

        /// <summary>
        /// Aggiorna il valore del numero di thread.
        /// </summary>
        /// <param name="UpdateAction">Azione di aggiornamento</param>
        public void UpdateThreadsValue(string UpdateAction)
        {
            switch (UpdateAction)
            {
                case "Add":
                    NumThreads += 1;
                    break;
                case "Remove":
                    NumThreads -= 1;
                    break;
            }
        }
        #region Set Information Methods
        /// <summary>
        /// Cambia la priorità del processo.
        /// </summary>
        /// <param name="NewPriority">Nuova priorità.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool SetProcessPriority(ProcessPriority NewPriority)
        {
            if (NativeHelpers.SetProcessPriority(AssociatedProcessHandle, NewPriority))
            {
                string ProcessPriorityString = Enum.GetName(typeof(ProcessPriority), NewPriority);
                Logger.WriteEntry(NativeHelpers.BuildLogEntryForInformation("Cambiata priorità di un processo, nuova priorità: " + ProcessPriorityString, EventAction.ProcessPropertiesManipulation, AssociatedProcessHandle));
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Cambia l'affinità del processo.
        /// </summary>
        /// <param name="NewAffinity">Nuova affinità del processo.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool SetProcessAffinity(BitArray NewAffinity)
        {
            if (NewAffinity is not null)
            {
                byte[] NewAffinityMask = new byte[8];
                NewAffinity.CopyTo(NewAffinityMask, 0);
                ulong NewAffinityValue = BitConverter.ToUInt64(NewAffinityMask, 0);
                if (NativeHelpers.SetProcessAffinity(AssociatedProcessHandle, NewAffinityValue))
                {
                    Logger.WriteEntry(NativeHelpers.BuildLogEntryForInformation("Cambiata affinità di un processo", EventAction.ProcessPropertiesManipulation, AssociatedProcessHandle));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion
        #region Termination Methods
        /// <summary>
        /// Termina il processo.<br></br>
        /// Se il processo è un processo di sistema o il processo corrente questo metodo restituirà false.
        /// </summary>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool TerminateProcess()
        {
            if (NativeHelpers.TerminateProcess(AssociatedProcessHandle))
            {
                Logger.WriteEntry(NativeHelpers.BuildLogEntryForInformation("Processo terminato, nome processo: " + Name, EventAction.ProcessTermination, null));
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Termina il processo e tutti i suoi figli.<br/>
        /// Se il processo è un processo di sistema o il processo corrente questo metodo restituirà false.
        /// </summary>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool TerminateProcessTree()
        {
            if (NativeHelpers.TerminateProcessTree(AssociatedProcessHandle, PID))
            {
                Logger.WriteEntry(NativeHelpers.BuildLogEntryForInformation("Processo e figli terminati, nome processo padre: " + Name, EventAction.ProcessTreeTermination, null));
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
        #region Debug Methods
        /// <summary>
        /// Aggancia un debugger al processo.
        /// </summary>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool DebugProcess()
        {
            if (NativeHelpers.AttachDebuggerToProcess(AssociatedProcessHandle))
            {
                Logger.WriteEntry(NativeHelpers.BuildLogEntryForInformation("Debugger agganciato a un processo", EventAction.DebugProcess, AssociatedProcessHandle));
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Interrompe il debugging del processo.
        /// </summary>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool StopDebuggingProcess()
        {
            if (NativeHelpers.DetachDebuggerFromProcess(AssociatedProcessHandle, PID))
            {
                Logger.WriteEntry(NativeHelpers.BuildLogEntryForInformation("Debugger scollegato da un processo", EventAction.StopDebugProcess, AssociatedProcessHandle));
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determina se il processo è in corso di debug.
        /// </summary>
        /// <returns>true se il processo è in corso di debug, false altrimenti.</returns>
        public bool IsProcessDebugged()
        {
            bool? Result = NativeHelpers.IsProcessBeingDebugged(AssociatedProcessHandle, PID);
            if (Result.HasValue)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
        #region Token Manipulation/Query Methods
        /// <summary>
        /// Abilita la virtualizzazione per il processo.
        /// </summary>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool EnableVirtualization()
        {
            if (NativeHelpers.EnableVirtualizationForProcess(AssociatedProcessHandle))
            {
                Logger.WriteEntry(NativeHelpers.BuildLogEntryForInformation("Virtualizzazione abilitata per un processo", EventAction.TokenInfoManipulation, AssociatedProcessHandle));
                return true;
            }
            else
            {
                Logger.WriteEntry(NativeHelpers.BuildLogEntryForWarning("Non è stato possibile abilitare la virtualizzazione per un processo", EventAction.TokenInfoManipulation, AssociatedProcessHandle));
                return false;
            }
        }

        /// <summary>
        /// Disabilita la virtualizzazione per il processo.
        /// </summary>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool DisableVirtualization()
        {
            if (NativeHelpers.DisableVirtualizationForProcess(AssociatedProcessHandle))
            {
                Logger.WriteEntry(NativeHelpers.BuildLogEntryForInformation("Virtualizzazione disabilitata per un processo", EventAction.TokenInfoManipulation, AssociatedProcessHandle));
                return true;
            }
            else
            {
                Logger.WriteEntry(NativeHelpers.BuildLogEntryForWarning("Non è stato possibile disabilitare la virtualizzazione per un processo", EventAction.TokenInfoManipulation, AssociatedProcessHandle));
                return false;
            }
        }

        /// <summary>
        /// Cambia il livello di integrità di un processo.
        /// </summary>
        /// <param name="NewLevel">Nuovo livello.</param>
        /// <returns>true se l'operazione è riuscita.</returns>
        public bool ChangeProcessIntegrityLevel(string NewLevel)
        {
            if (NativeHelpers.ChangeTokenIntegrityLevel(AssociatedProcessHandle, NewLevel))
            {
                Logger.WriteEntry(NativeHelpers.BuildLogEntryForInformation("Cambiato livello di integrità di un processo, nuovo livello: " + NewLevel, EventAction.TokenInfoManipulation, AssociatedProcessHandle));
                return true;
            }
            else
            {
                Logger.WriteEntry(NativeHelpers.BuildLogEntryForWarning("Non è stato possibile cambiare il livello di integrità di un processo", EventAction.TokenInfoManipulation, AssociatedProcessHandle));
                return false;
            }
        }

        /// <summary>
        /// Recupera informazioni sul token di accesso del processo.
        /// </summary>
        /// <returns>Un'istanza di <see cref="TokenInfo"/> con le informazioni.</returns>
        public TokenInfo GetProcessTokenInfo()
        {
            IntPtr TokenHandle = NativeHelpers.GetProcessTokenHandle(AssociatedProcessHandle);
            if (TokenHandle != IntPtr.Zero)
            {
                return NativeHelpers.GetProcessTokenInfo(TokenHandle, IsProcessElevated);
            }
            else
            {
                return null;
            }
            
        }
        #endregion
        #region Get Information Methods
        /// <summary>
        /// Recupera informazioni generali sul processo.
        /// </summary>
        /// <returns>Un'istanza di <see cref="ProcessGeneralInfo"/> con le informazioni.</returns>
        public ProcessGeneralInfo GetProcessGeneralInfo()
        {
            string ParentProcessName = WMIProcessInfoMethods.GetParentProcessName(CreationTime, PID);
            return new ProcessGeneralInfo(AssociatedProcessHandle, Description, CreationTime, PID, FullPath, Version, CommandLine, ParentProcessName);
        }

        /// <summary>
        /// Recupera le statistiche del processo.
        /// </summary>
        /// <returns>Un'istanza di <see cref="ProcessStatistics"/> con le informazioni.</returns>
        public ProcessStatistics GetProcessStatistics()
        {
            CPUStatistics CPU = GetCPUStatistics();
            MemoryStatistics Memory = GetMemoryStatistics();
            IOStatistics IO = GetIOStatistics();
            HandleStatistics Handle = GetHandleStatistics();
            return new ProcessStatistics(CPU, Memory, IO, Handle);
        }
        #region Statistics Info Getters
        #region CPU Statistics
        /// <summary>
        /// Recupera le statistiche CPU del processo.
        /// </summary>
        /// <returns>Un'istanza di <see cref="CPUStatistics"/> con le statistiche.</returns>
        private CPUStatistics GetCPUStatistics()
        {
            return new CPUStatistics(PID, AssociatedProcessHandle);
        }
        #endregion
        #region Memory Statistics
        /// <summary>
        /// Recupera le statistiche della memoria di un processo.
        /// </summary>
        /// <returns>Un'istanza di <see cref="MemoryStatistics"/> con le statistiche.</returns>
        private MemoryStatistics GetMemoryStatistics()
        {
            return new MemoryStatistics(AssociatedProcessHandle);
        }
        #endregion
        #region I/O Statistics
        /// <summary>
        /// Recupera le statistiche I/O del processo.
        /// </summary>
        /// <returns>Un'istanza di <see cref="IOStatistics"/> con le statistiche.</returns>
        private IOStatistics GetIOStatistics()
        {
            return new IOStatistics(AssociatedProcessHandle);
        }
        #endregion
        #region Handle Statistics
        /// <summary>
        /// Recupera le statistiche degli handle del processo.
        /// </summary>
        /// <returns>Un'istanza di <see cref="HandleStatistics"/> con le statistiche.</returns>
        private HandleStatistics GetHandleStatistics()
        {
            return new HandleStatistics(AssociatedProcessHandle);
        }

        #endregion
        #endregion
        /// <summary>
        /// Recupera informazioni sui tempi del processo e sui dati I/O.
        /// </summary>
        /// <returns>Una struttura <see cref="ProcessTimesAndIOData"/> con le informazioni.</returns>
        public ProcessTimesAndIOData GetProcessTimesAndIOData()
        {
            TimeSpan?[] ProcessTimes = NativeHelpers.GetProcessTimes(AssociatedProcessHandle);
            ulong[] ProcessIOData = NativeHelpers.GetProcessIOInfo(AssociatedProcessHandle);
            uint ProcessPageFaultCount = (uint)NativeHelpers.GetProcessMemorySizes(AssociatedProcessHandle)[4];
            return ProcessTimes[1].HasValue && ProcessTimes[0].HasValue
                ? (new(ProcessTimes[1].Value, ProcessTimes[0].Value, ProcessPageFaultCount, ProcessIOData[0], ProcessIOData[1], ProcessIOData[2], ProcessIOData[3], ProcessIOData[4], ProcessIOData[5]))
                : (new(TimeSpan.Zero, TimeSpan.Zero, ProcessPageFaultCount, ProcessIOData[0], ProcessIOData[1], ProcessIOData[2], ProcessIOData[3], ProcessIOData[4], ProcessIOData[5]));
        }
        #region Threads Info Getters
        /// <summary>
        /// Recupera informazioni sui thread del processo.
        /// </summary>
        /// <returns>Un dizionario contenente le informazioni.</returns>
        public ObservableCollection<ThreadInfo> GetThreadsInfo()
        {
            return NativeHelpers.GetProcessThreadsInfo(AssociatedProcessHandle, PID);
        }
        #endregion
        #region Windows Info Getters
        /// <summary>
        /// Recupera informazioni sulle finestre aperte dal processo.
        /// </summary>
        /// <returns>Un array di istanze di <see cref="WindowInfo"/> contenente le informazioni.</returns>
        public WindowInfo[] GetWindowsInfo()
        {
            return NativeHelpers.GetProcessWindowsInfo(AssociatedProcessHandle, PID);
        }
        #endregion
        #region Modules Info Getters
        /// <summary>
        /// Recupera informazioni sui moduli caricati dal processo.
        /// </summary>
        /// <returns>Una lista di istanze di <see cref="ModuleInfo"/> con le informazioni.</returns>
        public List<ModuleInfo> GetProcessModulesInfo()
        {
            return NativeHelpers.GetProcessModulesInfo(PID, AssociatedProcessHandle);
        }
        #endregion
        #region Memory Info Getters
        /// <summary>
        /// Recupera informazioni sulla memoria del processo.
        /// </summary>
        /// <returns>Una lista di istanze di <see cref="MemoryRegionInfo"/> con le informazioni.</returns>
        public List<MemoryRegionInfo> GetProcessMemoryInfo()
        {
            return NativeHelpers.GetProcessMemoryInfo(AssociatedProcessHandle);
        }
        #endregion
        #region Handle Information Getters
        /// <summary>
        /// Recupera informazioni sugli handle del processo.
        /// </summary>
        /// <returns>Un array di istanze di <see cref="HandleInfo"/> con le informazioni.</returns>
        public HandleInfo[] GetHandleInformation()
        {
            return NativeHelpers.GetProcessHandlesInfo(AssociatedProcessHandle, Name, PID);
        }
        #endregion
        #region .NET Performance Info Getters
        /// <summary>
        /// Recupera informazioni sulle prestazioni di un'applicazione .NET.
        /// </summary>
        /// <returns>Un istanza di <see cref="NetPerformanceInfo"/> con le informazioni.</returns>
        public NetPerformanceInfo GetNetPerformanceInfoForProcess()
        {
            Dictionary<string, List<PerformanceCounter>> Counters = NativeHelpers.GetCounters(AssociatedProcessHandle);
            if (Counters.Count > 0)
            {
                return new NetPerformanceInfo(Counters);
            }
            else
            {
                return null;
            }
        }
        #endregion
        #endregion
        #region Working Set Manipulation Methods
        /// <summary>
        /// Elimina dal working set del processo il maggior numero possibile di pagine.
        /// </summary>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool EmptyWorkingSet()
        {
            return NativeHelpers.EmptyProcessWorkingSet(AssociatedProcessHandle);
        }

        /// <summary>
        /// Cambia la dimensione massima dei working set del processo.
        /// </summary>
        /// <param name="NewSize">Nuova dimensione.</param>
        /// <param name="CurrentMinimumSize">Attuale limite minimo del working set.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool SetMaximumWorkingSetSize(ulong NewSize, IntPtr CurrentMinimumSize)
        {
            return NativeHelpers.SetProcessMaximumWorkingSetSize(AssociatedProcessHandle, NewSize, CurrentMinimumSize);
        }

        /// <summary>
        /// Cambia la dimensione minima dei working set del processo.
        /// </summary>
        /// <param name="NewSize">Nuova dimensione.</param>
        /// <param name="CurrentMaximumSize">Attuale limite massimo del working set.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool SetMinimumWorkingSetSize(ulong NewSize, IntPtr CurrentMaximumSize)
        {
            return NativeHelpers.SetProcessMinimumWorkingSetSize(AssociatedProcessHandle, NewSize, CurrentMaximumSize);
        }

        /// <summary>
        /// Recupera i limiti del working set di un processo.
        /// </summary>
        /// <returns>Una tupla con le informazioni, il primo elemento rappresenta la dimensione minima, il secondo la dimensione massima.</returns>
        public (IntPtr MinimumSize, IntPtr MaximumSize) GetWorkingSetLimits()
        {
            return NativeHelpers.GetProcessWorkingSetSize(AssociatedProcessHandle);
        }
        #endregion
        /// <summary>
        /// Aggiunge il processo a un job associato al limitatore CPU.
        /// </summary>
        /// <param name="JobHandle">Handle nativo al job.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool LimitProcessCpuUsage(IntPtr JobHandle)
        {
            if (IsProcessInJob.HasValue)
            {
                return !IsProcessInJob.Value && NativeHelpers.AddProcessToJob(JobHandle, AssociatedProcessHandle);
            }
            else
            {
                return false;
            }
        }

        private void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (MainWindow.ProgramTerminating)
                    {
                        Exit?.Invoke(this, EventArgs.Empty);
                        Exit = null;
                    }
                }
                AssociatedProcessHandle.Dispose();
                disposedValue = true;
            }
        }

        ~ProcessInfo()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}