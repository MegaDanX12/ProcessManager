using Microsoft.Win32.SafeHandles;
using ProcessManager.Commands.ProcessPropertiesWindowCommands;
using ProcessManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using System.Windows.Threading;
using ProcessManager.InfoClasses.ProcessStatistics;
using System.ComponentModel;
using ProcessManager.Commands.SemaphoreInfoWindowCommands;
using ProcessManager.ETW;
using System.Diagnostics;
using System.Globalization;

namespace ProcessManager.ViewModels
{
    public class ProcessPropertiesVM : IDisposable, INotifyPropertyChanged
    {
        /// <summary>
        /// Informazioni generali.
        /// </summary>
        public ProcessGeneralInfo GeneralInfo { get; }

        /// <summary>
        /// Statistiche.
        /// </summary>
        public ProcessStatistics Statistics { get; }

        /// <summary>
        /// Informazioni sui thread.
        /// </summary>
        public ObservableCollection<ThreadInfo> ThreadsInfo { get; }

        /// <summary>
        /// Informazioni sul token.
        /// </summary>
        public TokenInfo TokenInfo { get; }

        private ObservableCollection<ModuleInfo> ModulesInfoValue;

        /// <summary>
        /// Informazioni sui moduli.
        /// </summary>
        public ObservableCollection<ModuleInfo> ModulesInfo
        {
            get => ModulesInfoValue;
            private set
            {
                if (ModulesInfoValue != value)
                {
                    ModulesInfoValue = value;
                    NotifyPropertyChanged(nameof(ModulesInfo));
                }
            }
        }

        private ObservableCollection<MemoryRegionInfo> MemoryRegionsInfoValue;

        /// <summary>
        /// Informazioni sulla memoria utilizzata.
        /// </summary>
        public ObservableCollection<MemoryRegionInfo> MemoryRegionsInfo
        {
            get => MemoryRegionsInfoValue;
            private set
            {
                if (MemoryRegionsInfoValue != value)
                {
                    MemoryRegionsInfoValue = value;
                    NotifyPropertyChanged(nameof(MemoryRegionsInfo));
                }
            }
        }

        private ObservableCollection<HandleInfo> HandlesInfoValue;

        /// <summary>
        /// Informazioni sugli handle.
        /// </summary>
        public ObservableCollection<HandleInfo> HandlesInfo
        {
            get => HandlesInfoValue;
            private set
            {
                if (HandlesInfoValue != value)
                {
                    HandlesInfoValue = value;
                    NotifyPropertyChanged(nameof(HandlesInfo));
                }
            }
        }

        /// <summary>
        /// Informazioni sulle prestazioni (solo processi .NET).
        /// </summary>
        public NetPerformanceInfo NetPerformanceInfo { get; }

        /// <summary>
        /// Contatori handle.
        /// </summary>
        public ObservableCollection<HandleCountInfo> HandleCounters { get; }
        #region Commands
        /// <summary>
        /// Comando per visualizzare informazioni sull'immagine.
        /// </summary>
        public ICommand ImageInfoCommand { get; }

        /// <summary>
        /// Comando per aprire la cartella.
        /// </summary>
        public ICommand OpenFolderCommand { get; }

        /// <summary>
        /// Comando per visualizzare i dettagli sulle politiche di mitigazione.
        /// </summary>
        public ICommand MitigationPoliciesDetailCommand { get; }

        /// <summary>
        /// Comando per visualizzare informazioni dettagliate sul working set.
        /// </summary>
        public ICommand WorkingSetDetailedInfoCommand { get; }

        /// <summary>
        /// Comando per visualizzare dettagli sugli handle.
        /// </summary>
        public ICommand HandlesDetailedInfoCommand { get; }

        /// <summary>
        /// Comando per terminare un thread.
        /// </summary>
        public ICommand TerminateThreadCommand { get; }

        /// <summary>
        /// Comando per sospendere l'esecuzione di un thread.
        /// </summary>
        public ICommand SuspendThreadCommand { get; }

        /// <summary>
        /// Comando per riprendere l'esecuzione di un thread.
        /// </summary>
        public ICommand ResumeThreadCommand { get; }

        /// <summary>
        /// Comando per visualizzare le finestre associate a un thread.
        /// </summary>
        public ICommand ShowAssociatedWindowsCommand { get; }

        /// <summary>
        /// Comando per visualizzare le proprietà del token.
        /// </summary>
        public ICommand ShowTokenPropertiesCommand { get; }

        /// <summary>
        /// Comando per abilitare un privilegio.
        /// </summary>
        public ICommand EnablePrivilegeCommand { get; }

        /// <summary>
        /// Comando per disabilitare un privilegio.
        /// </summary>
        public ICommand DisablePrivilegeCommand { get; }

        /// <summary>
        /// Comando per rimuovere un privilegio.
        /// </summary>
        public ICommand RemovePrivilegeCommand { get; }

        /// <summary>
        /// Comando per visualizzare le proprietà di un file.
        /// </summary>
        public ICommand ShowFilePropertiesCommand { get; }

        /// <summary>
        /// Comando per aggiornare i dati.
        /// </summary>
        public ICommand RefreshDataCommand { get; }

        /// <summary>
        /// Comando per liberare una regione di memoria.
        /// </summary>
        public ICommand FreeMemoryRegionCommand { get; }

        /// <summary>
        /// Comando per annullare la mappatura di una regione di memoria.
        /// </summary>
        public ICommand DecommitMemoryRegionCommand { get; }

        /// <summary>
        /// Comando per chiudere un handle.
        /// </summary>
        public ICommand CloseHandleCommand { get; }

        /// <summary>
        /// Comando per visualizzare le proprietà di un handle.
        /// </summary>
        public ICommand ShowHandlePropertiesCommand { get; }

        /// <summary>
        /// Comando per visualizzare proprietà addizionali di un handle.
        /// </summary>
        public ICommand ShowHandleOtherPropertiesCommand { get; }

        /// <summary>
        /// Comando per acquisire un semaforo.
        /// </summary>
        public ICommand AcquireSemaphoreCommand { get; }

        /// <summary>
        /// Comando per rilasciare un semaforo.
        /// </summary>
        public ICommand ReleaseSemaphoreCommand { get; }
        #endregion
        /// <summary>
        /// Timer per l'aggiornamento dei dati.
        /// </summary>
        private readonly Timer UpdateTimer;

        /// <summary>
        /// Timer per l'aggiornamento dei dati sui thread.
        /// </summary>
        private readonly Timer UpdateThreadDataTimer;
        private bool disposedValue;

        /// <summary>
        /// Handle al processo.
        /// </summary>
        public SafeProcessHandle Handle { get; }

        private readonly object Lock = new();

        /// <summary>
        /// Istanza di <see cref="ManagementEventWatcher"/> che monitora l'avvio di thread.
        /// </summary>
        private readonly ManagementEventWatcher ThreadStartEventWatcher;

        /// <summary>
        /// Istanza di <see cref="ManagementEventWatcher"/> che monitora la terminazione di thread.
        /// </summary>
        private readonly ManagementEventWatcher ThreadStopEventWatcher;

        /// <summary>
        /// PID del processo associato.
        /// </summary>
        private readonly uint PID;

        /// <summary>
        /// Oggetto <see cref="System.Windows.Threading.Dispatcher"/> necessario per aggiornare la lista di thread.
        /// </summary>
        public Dispatcher Dispatcher { get; set; }

        /// <summary>
        /// Indica se i timer sono stati eliminati.
        /// </summary>
        private bool IsTimerDisposed;

        /// <summary>
        /// Opzioni del ciclo parallelo.
        /// </summary>
        private readonly ParallelOptions Options;

        /// <summary>
        /// Attività da eseguire per il ciclo parallelo.
        /// </summary>
        private readonly Action<ThreadInfo> CycleActivity;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Istanza di <see cref="ProcessInfo"/> associata al processo.
        /// </summary>
        public ProcessInfo Info { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="ProcessPropertiesVM"/>.
        /// </summary>
        /// <param name="GeneralInfo">Informazioni generali.</param>
        /// <param name="Statistics">Statistiche.</param>
        /// <param name="ThreadsInfo">Informazioni sui thread.</param>
        /// <param name="TokenInfo">Informazioni sul token.</param>
        /// <param name="ModulesInfo">Informazioni sui moduli.</param>
        /// <param name="MemoryRegionsInfo">Informazioni sulla memoria utilizzata.</param>
        /// <param name="HandlesInfo">Informazioni sugli handle.</param>
        /// <param name="NetPerformanceInfo">Informazioni sulle prestazioni .NET.</param>
        /// <param name="Handle">Handle al processo.</param>
        /// <param name="HandleCounters">Contatori per tipo degli handle.</param>
        /// <param name="Info">Istanza di <see cref="ProcessInfo"/> associata al processo.</param>
        public ProcessPropertiesVM(ProcessGeneralInfo GeneralInfo, ProcessStatistics Statistics, ObservableCollection<ThreadInfo> ThreadsInfo, TokenInfo TokenInfo, List<ModuleInfo> ModulesInfo, List<MemoryRegionInfo> MemoryRegionsInfo, List<HandleInfo> HandlesInfo, NetPerformanceInfo NetPerformanceInfo, SafeProcessHandle Handle, ObservableCollection<HandleCountInfo> HandleCounters, ProcessInfo Info)
        {
            this.GeneralInfo = GeneralInfo;
            this.Statistics = Statistics;
            this.ThreadsInfo = ThreadsInfo;
            this.TokenInfo = TokenInfo;
            ModulesInfoValue = new(ModulesInfo);
            MemoryRegionsInfoValue = new(MemoryRegionsInfo);
            HandlesInfoValue = new(HandlesInfo);
            this.NetPerformanceInfo = NetPerformanceInfo;
            this.HandleCounters = HandleCounters;
            this.Info = Info;
            if (Settings.DataSource is ProcessDataSource.WMI)
            {
                Options = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount / 2
                };
                CycleActivity = new(info => info.Update());
            }
            this.Handle = Handle;
            #region Commands Initialization
            ImageInfoCommand = new ImageInfoCommand(this);
            OpenFolderCommand = new OpenFolderCommand(this);
            MitigationPoliciesDetailCommand = new MitigationPoliciesDetailsCommand(this);
            WorkingSetDetailedInfoCommand = new WorkingSetDetailedInfoCommand(this);
            HandlesDetailedInfoCommand = new HandlesDetailedInfoCommand(this);
            TerminateThreadCommand = new TerminateThreadCommand(this);
            SuspendThreadCommand = new SuspendThreadCommand(this);
            ResumeThreadCommand = new ResumeThreadCommand(this);
            ShowAssociatedWindowsCommand = new ShowThreadWindowsInfoCommand();
            ShowTokenPropertiesCommand = new ShowTokenPropertiesCommand(this);
            EnablePrivilegeCommand = new EnableTokenPrivilegeCommand(this);
            DisablePrivilegeCommand = new DisableTokenPrivilegeCommand(this);
            RemovePrivilegeCommand = new RemovePrivilegeFromTokenCommand(this);
            ShowFilePropertiesCommand = new ShowFilePropertiesCommand(this);
            RefreshDataCommand = new RefreshDataCommand(this);
            FreeMemoryRegionCommand = new FreeMemoryRegionCommand(this);
            DecommitMemoryRegionCommand = new DecommitMemoryRegionCommand(this);
            CloseHandleCommand = new CloseHandleCommand(this);
            ShowHandlePropertiesCommand = new ShowHandlePropertiesCommand(this);
            ShowHandleOtherPropertiesCommand = new ShowHandleOtherPropertiesCommand(this);
            AcquireSemaphoreCommand = new AcquireSemaphoreCommand(this);
            ReleaseSemaphoreCommand = new ReleaseSemaphoreCommand(this);
            #endregion
            PID = NativeHelpers.GetProcessPID(Handle);
            UpdateTimer = new(new(UpdateData), null, 500, Timeout.Infinite);
            if (Settings.DataSource is ProcessDataSource.WMI)
            {
                UpdateThreadDataTimer = new(new(UpdateThreadData), null, 500, Timeout.Infinite);
                ThreadStartEventWatcher = new("SELECT * FROM Win32_ThreadStartTrace WHERE ProcessID = " + PID);
                ThreadStartEventWatcher.EventArrived += ThreadStartEventWatcher_EventArrived;
                ThreadStopEventWatcher = new("SELECT * FROM Win32_ThreadStopTrace WHERE ProcessID = " + PID);
                ThreadStopEventWatcher.EventArrived += ThreadStopEventWatcher_EventArrived;
                ThreadStartEventWatcher.Start();
                ThreadStopEventWatcher.Start();
            }
            else if (Settings.DataSource is ProcessDataSource.ETW)
            {
                ETWEventParser.InitializeProcessPropertiesEventsParser(this);
            }
        }

        private void ThreadStopEventWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            if (Dispatcher != null)
            {
                UpdateThreadData("RemoveThread", (uint)e.NewEvent.Properties["ThreadID"].Value);
            }
        }

        private void ThreadStartEventWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            if (Dispatcher != null)
            {
                UpdateThreadData("AddThread", (uint)e.NewEvent.Properties["ThreadID"].Value);
            }
        }
        #region Threads
        /// <summary>
        /// Termina un thread.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="ThreadInfo"/> associata al thread.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Contrassegnare i membri come static", Justification = "<In sospeso>")]
        public bool TerminateThread(ThreadInfo Info)
        {
            return Info.TerminateThread();
        }

        /// <summary>
        /// Sospende l'esecuzione di un thread.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="ThreadInfo"/> associata al thread.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Contrassegnare i membri come static", Justification = "<In sospeso>")]
        public bool SuspendThread(ThreadInfo Info)
        {
            return Info.SuspendThread();
        }

        /// <summary>
        /// Riprende l'esecuzione di un thread.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="ThreadInfo"/> associata al thread.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Contrassegnare i membri come static", Justification = "<In sospeso>")]
        public bool ResumeThread(ThreadInfo Info)
        {
            return Info.ResumeThread();
        }
        #endregion
        #region Privileges
        /// <summary>
        /// Abilita un privilegio nel token di un processo.
        /// </summary>
        /// <param name="PrivilegeName">Nome del privilegio.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool EnablePrivilege(string PrivilegeName)
        {
            return TokenInfo.EnablePrivilege(Handle, PrivilegeName);
        }

        /// <summary>
        /// Disabilita un privilegio nel token di un processo.
        /// </summary>
        /// <param name="PrivilegeName">Nome del privilegio.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool DisablePrivilege(string PrivilegeName)
        {
            return TokenInfo.DisablePrivilege(Handle, PrivilegeName);
        }

        /// <summary>
        /// Rimuove un privilegio dal token di un processo.
        /// </summary>
        /// <param name="PrivilegeName">Nome del privilegio.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool RemovePrivilege(string PrivilegeName)
        {
            return TokenInfo.RemovePrivilege(Handle, PrivilegeName);
        }
        #endregion
        /// <summary>
        /// Visualizza la finestra delle proprietà di un modulo.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="ModuleInfo"/> associata al modulo.</param>
        /// <returns>treu se l'operazione è riuscita, false altrimenti.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Contrassegnare i membri come static", Justification = "<In sospeso>")]
        public bool ShowModuleProperties(ModuleInfo Info)
        {
            return Info.ShowModulePropertiesWindow();
        }
        #region Memory
        /// <summary>
        /// Cambia la protezione di una regione di memoria.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="MemoryRegionInfo"/> associata con la regione.</param>
        /// <param name="NewProtection">Nuova protezione da applicare.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool ChangeMemoryRegionProtection(MemoryRegionInfo Info, string NewProtection)
        {
            if (Info.ChangeRegionProtection(Handle, NewProtection))
            {
                UpdateMemoryRegionsInfo();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Libera una regione di memoria.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="MemoryRegionInfo"/> associata con la regione.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool FreeMemoryRegion(MemoryRegionInfo Info)
        {
            if (Info.FreeRegion(Handle))
            {
                UpdateMemoryRegionsInfo();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Annulla la mappatura di una regione di memoria.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="MemoryRegionInfo"/> associata con la regione.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool DecommitMemoryRegion(MemoryRegionInfo Info)
        {
            if (Info.DecommitRegion(Handle))
            {
                UpdateMemoryRegionsInfo();
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
        /// <summary>
        /// Chiude un handle.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="HandleInfo"/> associata all'handle.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool CloseHandle(HandleInfo Info)
        {
            return Info.CloseHandle(Handle);
        }
        #region Update Methods
        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        /// <param name="state">Dati necessari all'operazione.</param>
        private void UpdateData(object state)
        {
            Statistics.CPU.Update(Handle);
            Statistics.Memory.Update(Handle);
            Statistics.IO.Update(Handle);
            Statistics.Handle.Update(Handle);
            if (!IsTimerDisposed)
            {
                _ = UpdateTimer.Change(500, Timeout.Infinite);
            }
        }

        /// <summary>
        /// Aggiorna i dati dei thread.
        /// </summary>
        /// <param name="state">Dati necessari all'operazione.</param>
        private void UpdateThreadData(object state)
        {
            UpdateThreadData("UpdateData");
            if (!IsTimerDisposed)
            {
                _ = UpdateThreadDataTimer.Change(500, Timeout.Infinite);
            }
        }

        /// <summary>
        /// Aggiorna i dati relativi ai thread.
        /// </summary>
        /// <param name="Operation">Operazione da eseguire.</param>
        /// <param name="ThreadID">>ID del thread.</param>
        public void UpdateThreadData(string Operation, uint ThreadID = 0)
        {
            ThreadInfo Info;
            lock (Lock)
            {
                switch (Operation)
                {
                    case "UpdateData":
                        _ = Parallel.ForEach(ThreadsInfo, Options, CycleActivity);
                        break;
                    case "AddThread":
                        Info = NativeHelpers.GetThreadInfo(ThreadID, PID);
                        if (Dispatcher != null)
                        {
                            Dispatcher.Invoke(() => ThreadsInfo.Add(Info));
                        }
                        break;
                    case "RemoveThread":
                        Info = ThreadsInfo.FirstOrDefault(thread => thread.TID == ThreadID);
                        if (Info != null)
                        {
                            if (Dispatcher != null)
                            {
                                _ = Dispatcher.Invoke(() => ThreadsInfo.Remove(Info));
                            }
                            Info.Dispose();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Aggiorna la lista di moduli.
        /// </summary>
        /// <param name="Operation">Operazione da eseguire.</param>
        /// <param name="ModuleFullPath">Percorso completo del modulo.</param>
        /// <param name="ModuleSize">Dimensione, in bytes, del modulo.</param>
        /// <param name="ModuleBaseAddress">Indirizzo di base del modulo in memoria.</param>
        /// <param name="ModuleDescription">Descrizione del modulo.</param>
        /// <param name="ModuleName">Nome del modulo.</param>
        /// <remarks>Tutti i parametri di questo metodo sono utilizzati solamente quanto la fonte dei dati è ETW.<br/><br/>
        /// <paramref name="ModuleFullPath"/>, <paramref name="ModuleSize"/>, <paramref name="ModuleBaseAddress"/> e <paramref name="ModuleDescription"/> sonoo utilizzati quando <paramref name="Operation"/> ha valore AddModule.<br/>
        /// <paramref name="ModuleName"/> è utilizzato quando <paramref name="Operation"/> ha valore RemoveModule.</remarks>
        public void UpdateModulesList(string Operation = null, string ModuleFullPath = null, uint? ModuleSize = null, uint? ModuleBaseAddress = null, string ModuleDescription = null, string ModuleName = null)
        {
            if (Settings.DataSource is ProcessDataSource.WMI)
            {
                ModulesInfo = new(NativeHelpers.GetProcessModulesInfo(PID, Handle));
            }
            else if (Settings.DataSource is ProcessDataSource.ETW)
            {
                ModuleInfo Info;
                switch (Operation)
                {
                    case "AddModule":
                        Info = new(ModuleFullPath, ModuleBaseAddress.Value.ToString("X", CultureInfo.CurrentCulture), ModuleSize.Value, ModuleDescription);
                        Dispatcher.Invoke(() => ModulesInfo.Add(Info));
                        break;
                    case "RemoveModule":
                        Info = ModulesInfo.FirstOrDefault(info => info.Name == ModuleName);
                        if (Info is not null)
                        {
                            _ = Dispatcher.Invoke(() => ModulesInfo.Remove(Info));
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Aggiorna i dati sulla memoria utilizzata.
        /// </summary>
        /// <param name="Operation"></param>
        /// <param name="Info"></param>
        /// <param name="BaseAddress"></param>
        public void UpdateMemoryRegionsInfo(string Operation = null, MemoryRegionInfo Info = null, IntPtr? BaseAddress = null)
        {
            if (Settings.DataSource is ProcessDataSource.WMI)
            {
                MemoryRegionsInfo = new(NativeHelpers.GetProcessMemoryInfo(Handle));
            }
            else if (Settings.DataSource is ProcessDataSource.ETW)
            {
                MemoryRegionInfo RegionInfo;
                switch (Operation)
                {
                    case "AddRegion":
                        Dispatcher.Invoke(() => MemoryRegionsInfo.Add(Info));
                        break;
                    case "RemoveRegion":
                        RegionInfo = MemoryRegionsInfo.FirstOrDefault(info => info.BaseAddress == BaseAddress.Value.ToString("X"));
                        _ = Dispatcher.Invoke(() => MemoryRegionsInfo.Remove(RegionInfo));
                        break;
                }
            }
        }

        /// <summary>
        /// Aggiorna i dati sugli handle aperti dal processo.
        /// </summary>
        /// <param name="Operation">Oprazione da eseguire.</param>
        /// <param name="Info">Istanza di <see cref="HandleInfo"/> che rappresenta un handle.</param>
        /// <param name="HandleValue">Valore numerico di un handle.</param>
        /// <remarks>Tutti i parametri di questo metodo sono utilizzati quando la fonte dei dati è ETW.<br/><br/>
        /// <paramref name="Info"/> è utilizzato quando <paramref name="Operation"/> ha valore AddHandle, <paramref name="HandleValue"/> è utilizzato quando <paramref name="Operation"/> ha valore RemoveHandle.</remarks>
        public void UpdateHandlesList(string Operation = null, HandleInfo Info = null, string HandleValue = null)
        {
            if (Settings.DataSource is ProcessDataSource.WMI)
            {
                HandlesInfo = new(NativeHelpers.GetProcessHandlesInfo(Handle));
            }
            else if (Settings.DataSource is ProcessDataSource.ETW)
            {
                HandleInfo HandleInfo;
                switch (Operation)
                {
                    case "AddHandle":
                        Dispatcher.Invoke(() => HandlesInfo.Add(Info));
                        break;
                    case "RemoveHandle":
                        HandleInfo = HandlesInfo.FirstOrDefault(info => info.Value == HandleValue);
                        if (HandleInfo is not null)
                        {
                            _ = Dispatcher.Invoke(() => HandlesInfo.Remove(HandleInfo));
                        }
                        break;
                }
            }
        }
        #endregion
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
                    Dispatcher = null;
                    if (Settings.DataSource is ProcessDataSource.WMI)
                    {
                        ThreadStartEventWatcher.Stop();
                        ThreadStartEventWatcher.Dispose();
                        ThreadStopEventWatcher.Stop();
                        ThreadStopEventWatcher.Dispose();
                        UpdateThreadDataTimer.Dispose();
                    }
                    else if (Settings.DataSource is ProcessDataSource.ETW)
                    {
                        ETWEventParser.ShutdownProcessPropertiesEventsParser();
                    }
                    UpdateTimer.Dispose();
                    IsTimerDisposed = true;
                    foreach (ThreadInfo info in ThreadsInfo)
                    {
                        info.Dispose();
                    }
                    if (NetPerformanceInfo != null)
                    {
                        NetPerformanceInfo.Dispose();
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