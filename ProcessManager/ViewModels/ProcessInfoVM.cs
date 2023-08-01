using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management;
using Microsoft.Win32.SafeHandles;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ProcessManager.Models;
using System.Windows.Input;
using ProcessManager.Commands.MainWindowCommands;
using Microsoft.Win32;
using System.Diagnostics;
using System.Security;
using System.Collections.Generic;
using static ProcessManager.NativeHelpers;
using ProcessManager.InfoClasses.ServicesInfo;
using System.ComponentModel;
using ProcessManager.ETW;
using ProcessManager.Watchdog;
using Microsoft.Toolkit.Uwp.Notifications;

namespace ProcessManager.ViewModels
{
    public class ProcessInfoVM : IDisposable, INotifyPropertyChanged
    {
        /// <summary>
        /// Oggetto di sincronizzazione.
        /// </summary>
        private readonly object Locker = new();
        #region Data Fields
        /// <summary>
        /// Informazioni sui processi.
        /// </summary>
        public ObservableCollection<ProcessInfo> ActiveProcessesInfo { get; private set; }

        /// <summary>
        /// Informazioni sui processi (non filtrata).
        /// </summary>
        private ObservableCollection<ProcessInfo> UnfilteredActiveProcessesInfo { get; }

        /// <summary>
        /// Numero di processi attualmente in esecuzione.
        /// </summary>
        public int ProcessesCount => UnfilteredActiveProcessesInfo.Count;


        private uint MemoryUsagePercentageValue;

        /// <summary>
        /// Utilizzo, in percentuale, della memoria.
        /// </summary>
        public uint MemoryUsagePercentage
        {
            get => MemoryUsagePercentageValue;
            private set
            {
                if (value != MemoryUsagePercentageValue)
                {
                    MemoryUsagePercentageValue = value;
                    NotifyPropertyChanged(nameof(MemoryUsagePercentage));
                }
            }
        }

        private string MemoryUsageValue;

        /// <summary>
        /// Utilizzo memoria.
        /// </summary>
        public string MemoryUsage
        {
            get => MemoryUsageValue;
            private set
            {
                if (value != MemoryUsageValue)
                {
                    MemoryUsageValue = value;
                    NotifyPropertyChanged(nameof(MemoryUsage));
                }
            }
        }

        private decimal CpuUsageValue;

        /// <summary>
        /// Utilizzo CPU, in percentuale.
        /// </summary>
        public decimal CpuUsagePercentage
        {
            get => CpuUsageValue;
            private set
            {
                if (value != CpuUsageValue)
                {
                    CpuUsageValue = value;
                    NotifyPropertyChanged(nameof(CpuUsagePercentage));
                }
            }
        }
        #endregion
        #region Commands
        /// <summary>
        /// Comando per terminare un processo.
        /// </summary>
        public ICommand TerminateProcessCommand { get; }

        /// <summary>
        /// Comando per terminare un processo e tutti i suoi figli.
        /// </summary>
        public ICommand TerminateProcessTreeCommand { get; }

        /// <summary>
        /// Avvia un processo.
        /// </summary>
        public ICommand StartProcessCommand { get; }

        /// <summary>
        /// Avvia un processo come l'utente specificato.
        /// </summary>
        public ICommand StartProcessAsUserCommand { get; }

        /// <summary>
        /// Avvia un processo con limiti applicati all'utilizzo CPU.
        /// </summary>
        public ICommand StartLimitedProcessCommand { get; }

        /// <summary>
        /// Recupera informazioni sui processi figlio del processo specificato.
        /// </summary>
        public ICommand ShowChildrenProcessesCommand { get; }

        /// <summary>
        /// Mostra le proprietà del processo.
        /// </summary>
        public ICommand PropertiesCommand { get; }

        /// <summary>
        /// Connette un debugger al processo.
        /// </summary>
        public ICommand DebugProcessCommand { get; }

        /// <summary>
        /// Abilita virtualizzazione per il processo.
        /// </summary>
        public ICommand EnableVirtualizationCommand { get; }

        /// <summary>
        /// Disabilita virtualizzazione per il processo.
        /// </summary>
        public ICommand DisableVirtualizationCommand { get; }

        /// <summary>
        /// Mostra informazioni sulle finestre aperte da un processo.
        /// </summary>
        public ICommand ShowProcessWindowsInfoCommand { get; }

        /// <summary>
        /// Mostra i servizi ospitati da un processo.
        /// </summary>
        public ICommand ShowHostedServicesCommand { get; }

        /// <summary>
        /// Visualizza il log.
        /// </summary>
        public ICommand ShowLogCommand { get; }

        /// <summary>
        /// Abilita il logging delle attività.
        /// </summary>
        public ICommand EnableLoggingCommand { get; }

        /// <summary>
        /// Disabilita il loggig delle attività.
        /// </summary>
        public ICommand DisableLoggingCommand { get; }

        /// <summary>
        /// Inizializza il monitoraggio dei servizi.
        /// </summary>
        public ICommand InitializeServiceMonitoringCommand { get; }

        /// <summary>
        /// Interrompe il monitoraggio dei servizi.
        /// </summary>
        public ICommand ShutdownServiceMonitoringCommand { get; }

        /// <summary>
        /// Visualizza la lista dei servizi esistenti nel computer.
        /// </summary>
        public ICommand ShowServicesListCommand { get; }

        /// <summary>
        /// Modifica delle impostazioni.
        /// </summary>
        public ICommand EditSettingsCommand { get; }

        /// <summary>
        /// Blocca il computer.
        /// </summary>
        public ICommand LockComputerCommand { get; }

        /// <summary>
        /// Disconnette l'utente.
        /// </summary>
        public ICommand LogoffUserCommand { get; }

        /// <summary>
        /// Inizia il cambio di stato del computer allo stato in standby.
        /// </summary>
        public ICommand SleepMachineCommand { get; }

        /// <summary>
        /// Inizia l'ibernazione del computer.
        /// </summary>
        public ICommand HibernateMachineCommand { get; }

        /// <summary>
        /// Inizia il riavvio del sistema.
        /// </summary>
        public ICommand RestartMachineCommand { get; }

        /// <summary>
        /// Inizia l'arresto del sistema.
        /// </summary>
        public ICommand ShutdownMachineCommand { get; }

        /// <summary>
        /// Mostra informazioni sul computer.
        /// </summary>
        public ICommand ShowComputerInfoCommand { get; }

        /// <summary>
        /// Elimina il maggior numero possibile di pagine dal working set di un processo.
        /// </summary>
        public ICommand EmptyProcessWorkingSetCommand { get; }

        /// <summary>
        /// Imposta la dimensione massima del working set di un processo.
        /// </summary>
        public ICommand SetMaximumWorkingSetCommand { get; }

        /// <summary>
        /// Imposta la dimensione minima del working set di un processo.
        /// </summary>
        public ICommand SetMinimumWorkingSetCommand { get; }

        /// <summary>
        /// Aggiorna l'applicazione.
        /// </summary>
        public ICommand UpdateCommand { get; }

        /// <summary>
        /// Abilita il watchdog processi.
        /// </summary>
        public ICommand EnableWatchdogCommand { get; }

        /// <summary>
        /// Disabilita il watchdog processi.
        /// </summary>
        public ICommand DisableWatchdogCommand { get; }

        /// <summary>
        /// Visualizza le regole del watchdog per i processi.
        /// </summary>
        public ICommand ShowProcessWatchdogRulesSettingsCommand { get; }

        /// <summary>
        /// Visualizza le impostazione del watchdog memoria.
        /// </summary>
        public ICommand ShowMemoryWatchdogSettingsCommand { get; }

        /// <summary>
        /// Visualizza le impostazioni predefinite della CPU per i processi.
        /// </summary>
        public ICommand ShowProcessCPUDefaultSettingsCommand { get; }

        /// <summary>
        /// Visualizza le impostazioni dell'utilizzo enegertico dei processi.
        /// </summary>
        public ICommand ShowProcessEnergyUsageSettingsCommand { get; }

        /// <summary>
        /// Visualizza le impostazioni del limite delle istanze dei processi.
        /// </summary>
        public ICommand ShowProcessInstanceLimitsSettingsCommand { get; }

        /// <summary>
        /// Visualizza le impostazioni relative ai processi non permessi.
        /// </summary>
        public ICommand ShowDisallowedProcessesSettingsCommand { get; }

        /// <summary>
        /// Visualizza le impostazionie relative ai processi da mantenere in esecuzione.
        /// </summary>
        public ICommand ShowPermanentProcessesSettingsCommand { get; }

        /// <summary>
        /// Ordina la lista processi.
        /// </summary>
        public ICommand SortListCommand { get; }

        /// <summary>
        /// Visualizza le impostazioni del limitatore processi.
        /// </summary>
        public ICommand ShowProcessLimiterSettingsCommand { get; }

        /// <summary>
        /// Visualizza i processi attualmente limitati.
        /// </summary>
        public ICommand ShowActiveLimitedProcessesCommand { get; }

        /// <summary>
        /// Visualizza informazioni sui file di paging esistenti.
        /// </summary>
        public ICommand ShowPagefilesInfoCommand { get; }

        /// <summary>
        /// Pulisci la memoria di tutti i processi.
        /// </summary>
        public ICommand CleanAllProcessesMemoryCommand { get; }

        /// <summary>
        /// Trova il processo proprietario di una finestra.
        /// </summary>
        public ICommand FindWindowCommand { get; }

        /// <summary>
        /// Trova il processo proprietario di una finestra e lo termina.
        /// </summary>
        public ICommand FindWindowAndTerminateOwnerCommand { get; }
        #endregion
        #region Utility Fields
        /// <summary>
        /// Timer per l'aggiornamento dei dati.
        /// </summary>
        private readonly Timer UpdateTimer;

        /// <summary>
        /// Timer per l'aggiornamento dei dati riguardanti utilizzo CPU e memoria.
        /// </summary>
        private readonly Timer UpdateSystemDataTimer;

        /// <summary>
        /// Istanza di <see cref="ManagementEventWatcher"/> che monitora l'avvio di thread.
        /// </summary>
        private readonly ManagementEventWatcher ThreadStartEventWatcher;

        /// <summary>
        /// Istanza di <see cref="ManagementEventWatcher"/> che monitora la terminazione di thread.
        /// </summary>
        private readonly ManagementEventWatcher ThreadStopEventWatcher;

        /// <summary>
        /// Istanza di <see cref="ManagementEventWatcher"/> che monitora l'avvio di processi.
        /// </summary>
        private readonly ManagementEventWatcher ProcessStartEventWatcher;

        /// <summary>
        /// Istanza di <see cref="ManagementEventWatcher"/> che monitora la terminazione di processi.
        /// </summary>
        private readonly ManagementEventWatcher ProcessStopEventWatcher;

        private bool disposedValue;

        /// <summary>
        /// Opzioni del ciclo parallelo.
        /// </summary>
        private readonly ParallelOptions Options;

        /// <summary>
        /// Attività del ciclo parallelo per aggiornare i dati di un processo.
        /// </summary>
        private readonly Action<ProcessInfo> CycleActivity;

        /// <summary>
        /// Dati sui servizi installati nel sistema.
        /// </summary>
        public Services ServicesData { get; private set; }


        private bool IsTimerDisposed;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Valore del filtro della lista dei processi.
        /// </summary>
        public string FilterValue { get; set; }

        /// <summary>
        /// Indica se la lista processi è ordinata.
        /// </summary>
        public bool IsDatagridSorted { get; set; }

        /// <summary>
        /// Direzione dell'ordinamento della lista processi.
        /// </summary>
        public SortOrder DatagridSortOrder { get; set; }

        /// <summary>
        /// Colonna secondo cui la lista processi è stata ordinata.
        /// </summary>
        public string DatagridSortColumn { get; set; }

        /// <summary>
        /// Evento che indica se la lista processi è disponibile per la manipolazione.
        /// </summary>
        private readonly ManualResetEvent ListAvailable = new(true);

        /// <summary>
        /// Informazioni sul processo di inattività del sistema.
        /// </summary>
        private readonly ProcessInfo SystemIdleProcessData;

        private ProcessInfo WindowOwnerValue;

        /// <summary>
        /// Istanza di <see cref="ProcessInfo"/> che rappresenta il processo proprietario della finestra selezionata tramite il comando "Trova finestra".
        /// </summary>
        public ProcessInfo WindowOwner
        {
            get => WindowOwnerValue;
            set
            {
                if (WindowOwnerValue != value)
                {
                    WindowOwnerValue = value;
                    NotifyPropertyChanged(nameof(WindowOwner));
                }
            }
        }

        /// <summary>
        /// Indica se la fonte dei dati è cambiata durante l'uso.
        /// </summary>
        public static bool HasDataSourceChanged { get; set; }

        /// <summary>
        /// Handle nativo all'hook eventi.
        /// </summary>
        private IntPtr ForegroundWindowChangeEventHookHandle;
        #endregion

        public ProcessInfoVM()
        {
            ActiveProcessesInfo = new();
            #region Commands Initialization
            TerminateProcessCommand = new TerminateProcessCommand(this);
            TerminateProcessTreeCommand = new TerminateProcessTreeCommand(this);
            StartProcessCommand = new StartProcessCommand(this);
            StartProcessAsUserCommand = new StartProcessAsUserCommand(this);
            StartLimitedProcessCommand = new StartLimitedProcessCommand(this);
            ShowChildrenProcessesCommand = new ShowChildrenProcessesCommand(this);
            PropertiesCommand = new PropertiesCommand(this);
            DebugProcessCommand = new DebugProcessCommand(this);
            EnableVirtualizationCommand = new EnableVirtualizationCommand(this);
            DisableVirtualizationCommand = new DisableVirtualizationCommand(this);
            ShowProcessWindowsInfoCommand = new ShowProcessWindowsInfoCommand();
            ShowHostedServicesCommand = new ShowHostedServicesCommand(this);
            ShowLogCommand = new ShowLogCommand(this);
            EnableLoggingCommand = new EnableLoggingCommand(this);
            DisableLoggingCommand = new DisableLoggingCommand(this);
            InitializeServiceMonitoringCommand = new InitializeServiceMonitoringCommand(this);
            ShutdownServiceMonitoringCommand = new ShutdownServiceMonitoringCommand(this);
            ShowServicesListCommand = new ShowServicesListCommand(this);
            EditSettingsCommand = new EditSettingsCommand(this);
            LockComputerCommand = new LockComputerCommand(this);
            LogoffUserCommand = new LogoffUserCommand(this);
            SleepMachineCommand = new SleepMachineCommand(this);
            HibernateMachineCommand = new HibernateMachineCommand(this);
            RestartMachineCommand = new RestartSystemCommand(this);
            ShutdownMachineCommand = new ShutdownSystemCommand(this);
            ShowComputerInfoCommand = new ShowComputerInfoCommand(this);
            EmptyProcessWorkingSetCommand = new EmptyProcessWorkingSetCommand(this);
            SetMinimumWorkingSetCommand = new SetProcessMinimumWorkingSetSizeCommand(this);
            SetMaximumWorkingSetCommand = new SetProcessMaximumWorkingSetCommand(this);
            UpdateCommand = new UpdateCommand();
            EnableWatchdogCommand = new EnableWatchdogCommand();
            DisableWatchdogCommand = new DisableWatchdogCommand();
            ShowProcessWatchdogRulesSettingsCommand = new ShowProcessWatchdogRulesSettingsCommand();
            ShowMemoryWatchdogSettingsCommand = new ShowMemoryWatchdogSettingsCommand();
            ShowProcessCPUDefaultSettingsCommand = new ShowProcessCPUDefaultsSettingsCommand();
            ShowProcessEnergyUsageSettingsCommand = new ShowProcessEnergyUsageCommand();
            ShowProcessInstanceLimitsSettingsCommand = new ShowProcessInstanceLimitsSettingsCommand();
            ShowDisallowedProcessesSettingsCommand = new ShowDisallowedProcessesSettingsCommand();
            ShowPermanentProcessesSettingsCommand = new ShowPermanentProcessesSettingsCommand();
            SortListCommand = new SortListCommand(this);
            ShowProcessLimiterSettingsCommand = new ShowProcessLimiterSettingsCommand(this);
            ShowActiveLimitedProcessesCommand = new ShowActiveLimitedProcessesCommand();
            ShowPagefilesInfoCommand = new ShowPagefilesInfoCommand();
            CleanAllProcessesMemoryCommand = new CleanAllProcessesMemoryCommand(this);
            FindWindowCommand = new FindWindowCommand(this);
            FindWindowAndTerminateOwnerCommand = new FindWindowAndTerminateOwnerCommand(this);
            #endregion
            Logger.WriteEntry(BuildLogEntryForInformation("Inizio raccolta dati sui servizi del sistema", EventAction.ProgramStartup));
            ServicesData = new();
            WatchdogManager.LoadWatchdogSettings();
            Logger.WriteEntry(BuildLogEntryForInformation("Caricate impostazioni del watchdog processi", EventAction.ProgramStartup));
            ProcessLimiter.LoadProcessLimiterSettings();
            Logger.WriteEntry(BuildLogEntryForInformation("Caricate impostazioni del limitatore processi", EventAction.ProgramStartup));
            PopulateCollection();
            Logger.WriteEntry(BuildLogEntryForInformation("Recuperate informazioni sui processi in esecuzione", EventAction.ProgramStartup));
            UnfilteredActiveProcessesInfo = new(ActiveProcessesInfo);
            SystemIdleProcessData = UnfilteredActiveProcessesInfo.First(info => info.PID is 0);
            Options = new()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount / 2
            };
            CycleActivity = new(info => info.Refresh());
            UpdateTimer = new((state) => UpdateProcessData("UpdateData"), null, Settings.ProcessDataUpdateRate, Timeout.Infinite);
            if (Settings.DataSource is ProcessDataSource.WMI)
            {
                ProcessStartEventWatcher = new("SELECT * FROM Win32_ProcessStartTrace");
                ProcessStartEventWatcher.EventArrived += ProcessStartEventWatcher_EventArrived;
                ProcessStopEventWatcher = new("SELECT * FROM Win32_ProcessStopTrace");
                ProcessStopEventWatcher.EventArrived += ProcessStopEventWatcher_EventArrived;
                ThreadStartEventWatcher = new("SELECT * FROM Win32_ThreadStartTrace");
                ThreadStartEventWatcher.EventArrived += ThreadStartEventWatcher_EventArrived;
                ThreadStopEventWatcher = new("SELECT * FROM Win32_ThreadStopTrace");
                ThreadStopEventWatcher.EventArrived += ThreadStopEventWatcher_EventArrived;
                ProcessStartEventWatcher.Start();
                ProcessStopEventWatcher.Start();
                ThreadStartEventWatcher.Start();
                ThreadStopEventWatcher.Start();
                Logger.WriteEntry(BuildLogEntryForInformation("Iniziato monitoraggio processi e thread", EventAction.ProgramStartup));
            }
            else if (Settings.DataSource is ProcessDataSource.ETW)
            {
                ETWSessionControl.StartSession(this);
                _ = Task.Run(() => ETWSessionControl.StartSessionEventsProcessing());
                Logger.WriteEntry(BuildLogEntryForInformation("Iniziato monitoraggio processi e thread", EventAction.ProgramStartup));
            }
            GetMemoryAndCpuUsage(true);
            UpdateSystemDataTimer = new((state) => GetMemoryAndCpuUsage(false), null, Settings.ProcessDataUpdateRate, Timeout.Infinite);
            if (Settings.WatchdogEnabled)
            {
                Logger.WriteEntry(BuildLogEntryForInformation("Inizializzazione del watchdog iniziata", EventAction.ProgramStartup));
                WatchdogManager.InitializeWatchdog(this);
                Logger.WriteEntry(BuildLogEntryForInformation("Watchdog inizializzato", EventAction.ProgramStartup));
            }
            ProcessLimiter.LimitRunningProcesses(UnfilteredActiveProcessesInfo.ToList());
        }

        private void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        private void ThreadStopEventWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            UpdateProcessData("UpdateThreadsEnd", (uint)e.NewEvent.Properties["ProcessID"].Value);
        }

        private void ThreadStartEventWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            UpdateProcessData("UpdateThreadsNew", (uint)e.NewEvent.Properties["ProcessID"].Value);
        }

        private void ProcessStopEventWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            UpdateProcessData("RemoveProcess", (uint)e.NewEvent.Properties["ProcessID"].Value);
            NotifyPropertyChanged(nameof(ProcessesCount));
        }

        private void ProcessStartEventWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            UpdateProcessData("AddProcess", (uint)e.NewEvent.Properties["ProcessID"].Value);
            NotifyPropertyChanged(nameof(ProcessesCount));
        }
        #region Collection Manipulation Methods
        /// <summary>
        /// Popola la lista di processi.
        /// </summary>
        public void PopulateCollection()
        {
            SafeProcessHandle[] ProcessHandles = NativeHelpers.GetRunningProcesses(out List<(uint PID, uint ThreadCount, string Name)> OtherInformations);
            if (ProcessHandles != null)
            {
                for (int i = 0; i < ProcessHandles.Length; i++)
                {
                    ActiveProcessesInfo.Add(new(ProcessHandles[i], OtherInformations[i].PID, OtherInformations[i].ThreadCount, OtherInformations[i].Name));
                }
            }
        }

        /// <summary>
        /// Recupera la lista dei processi attualmente in esecuzione.
        /// </summary>
        /// <returns>La lista di processi in esecuzione.</returns>
        public List<ProcessInfo> GetRunningProcesses()
        {
            return new(UnfilteredActiveProcessesInfo);
        }

        /// <summary>
        /// Recupera le istanze di <see cref="ProcessInfo"/> che rappresentano i processi il cui utilizzo della memoria supera il massimo.
        /// </summary>
        /// <returns>Un array con le istanze di <see cref="ProcessInfo"/>.</returns>
        public ProcessInfo[] FindProcessesWithHighMemoryUsage()
        {
            ulong MemoryLimitBytes = Settings.MaxProcessMemoryUsage * 1024 * 1024;
            lock (Locker)
            {
                return UnfilteredActiveProcessesInfo.Where(info => info.PrivateMemoryBytes >= Settings.MaxProcessMemoryUsage).ToArray();
            }
        }

        /// <summary>
        /// Recupera le istanze di <see cref="ProcessInfo"/> con il nome specificato.
        /// </summary>
        /// <param name="Name">Nome dei processi da recuperare.</param>
        /// <returns>Un array con le istanze di <see cref="ProcessInfo"/>.</returns>
        public ProcessInfo[] FindProcessesWithName(string Name)
        {
            lock (Locker)
            {
                return UnfilteredActiveProcessesInfo.Where(info => info.Name == Name).ToArray();
            }
        }
        #region Process List Filtering/Sorting Methods
        /// <summary>
        /// Filtra la lista processi.
        /// </summary>
        public void FilterProcessList()
        {
            _ = ListAvailable.WaitOne();
            if (!string.IsNullOrWhiteSpace(FilterValue))
            {
                List<ProcessInfo> FilteredProcessList = UnfilteredActiveProcessesInfo.ToList().FindAll(info => info.Name.IndexOf(FilterValue, StringComparison.CurrentCultureIgnoreCase) is not -1 and 0);
                if (IsDatagridSorted)
                {
                    FilteredProcessList = SortProcessList(FilteredProcessList);
                }
                ActiveProcessesInfo.Clear();
                foreach (ProcessInfo info in FilteredProcessList)
                {
                    ActiveProcessesInfo.Add(info);
                }
            }
            else
            {
                if (IsDatagridSorted)
                {
                    List<ProcessInfo> UnfilteredSortedProcessList = SortProcessList(UnfilteredActiveProcessesInfo.ToList());
                    ActiveProcessesInfo.Clear();
                    foreach (ProcessInfo info in UnfilteredSortedProcessList)
                    {
                        ActiveProcessesInfo.Add(info);
                    }
                }
                else
                {
                    ActiveProcessesInfo.Clear();
                    foreach (ProcessInfo info in UnfilteredActiveProcessesInfo)
                    {
                        ActiveProcessesInfo.Add(info);
                    }
                }
            }
        }

        /// <summary>
        /// Ordina la lista processi.
        /// </summary>
        public void SortProcessList()
        {
            _ = ListAvailable.Reset();
            if (DatagridSortOrder is SortOrder.Ascending)
            {
                if (DatagridSortColumn == Properties.Resources.ProcessNameHeader)
                {
                    IOrderedEnumerable<ProcessInfo> OrderedListEnumerable = ActiveProcessesInfo.OrderBy(info => info.Name);
                    List<ProcessInfo> OrderedList = new(OrderedListEnumerable);
                    ActiveProcessesInfo.Clear();
                    foreach (ProcessInfo info in OrderedList)
                    {
                        ActiveProcessesInfo.Add(info);
                    }
                }
                else if (DatagridSortColumn == "PID")
                {
                    IOrderedEnumerable<ProcessInfo> OrderedListEnumerable = ActiveProcessesInfo.OrderBy(info => info.PID);
                    List<ProcessInfo> OrderedList = new(OrderedListEnumerable);
                    ActiveProcessesInfo.Clear();
                    foreach (ProcessInfo info in OrderedList)
                    {
                        ActiveProcessesInfo.Add(info);
                    }
                }
                else if (DatagridSortColumn == Properties.Resources.ProcessDescriptionHeader)
                {
                    IOrderedEnumerable<ProcessInfo> OrderedListEnumerable = ActiveProcessesInfo.OrderBy(info => info.Description);
                    List<ProcessInfo> OrderedList = new(OrderedListEnumerable);
                    ActiveProcessesInfo.Clear();
                    foreach (ProcessInfo info in OrderedList)
                    {
                        ActiveProcessesInfo.Add(info);
                    }
                }
                else if (DatagridSortColumn == Properties.Resources.ProcessStartDateTimeHeader)
                {
                    IOrderedEnumerable<ProcessInfo> OrderedListEnumerable = ActiveProcessesInfo.OrderBy(info => info.StartTime);
                    List<ProcessInfo> OrderedList = new(OrderedListEnumerable);
                    ActiveProcessesInfo.Clear();
                    foreach (ProcessInfo info in OrderedList)
                    {
                        ActiveProcessesInfo.Add(info);
                    }
                }
            }
            else
            {
                if (DatagridSortColumn == Properties.Resources.ProcessNameHeader)
                {
                    IOrderedEnumerable<ProcessInfo> OrderedListEnumerable = ActiveProcessesInfo.OrderByDescending(info => info.Name);
                    List<ProcessInfo> OrderedList = new(OrderedListEnumerable);
                    ActiveProcessesInfo.Clear();
                    foreach (ProcessInfo info in OrderedList)
                    {
                        ActiveProcessesInfo.Add(info);
                    }
                }
                else if (DatagridSortColumn == "PID")
                {
                    IOrderedEnumerable<ProcessInfo> OrderedListEnumerable = ActiveProcessesInfo.OrderByDescending(info => info.PID);
                    List<ProcessInfo> OrderedList = new(OrderedListEnumerable);
                    ActiveProcessesInfo.Clear();
                    foreach (ProcessInfo info in OrderedList)
                    {
                        ActiveProcessesInfo.Add(info);
                    }
                }
                else if (DatagridSortColumn == Properties.Resources.ProcessDescriptionHeader)
                {
                    IOrderedEnumerable<ProcessInfo> OrderedListEnumerable = ActiveProcessesInfo.OrderByDescending(info => info.Description);
                    List<ProcessInfo> OrderedList = new(OrderedListEnumerable);
                    ActiveProcessesInfo.Clear();
                    foreach (ProcessInfo info in OrderedList)
                    {
                        ActiveProcessesInfo.Add(info);
                    }
                }
                else if (DatagridSortColumn == Properties.Resources.ProcessStartDateTimeHeader)
                {
                    IOrderedEnumerable<ProcessInfo> OrderedListEnumerable = ActiveProcessesInfo.OrderByDescending(info => info.StartTime);
                    List<ProcessInfo> OrderedList = new(OrderedListEnumerable);
                    ActiveProcessesInfo.Clear();
                    foreach (ProcessInfo info in OrderedList)
                    {
                        ActiveProcessesInfo.Add(info);
                    }
                }
            }
            _ = ListAvailable.Set();
        }

        /// <summary>
        /// Ordina la lista processi fornita.
        /// </summary>
        /// <param name="UnsortedList">Lista da ordinare.</param>
        /// <returns>La lista ordinata.</returns>
        private List<ProcessInfo> SortProcessList(List<ProcessInfo> UnsortedList)
        {
            List<ProcessInfo> SortedList = null;
            if (DatagridSortOrder is SortOrder.Ascending)
            {
                if (DatagridSortColumn == Properties.Resources.ProcessNameHeader)
                {
                    SortedList = (List<ProcessInfo>)UnsortedList.OrderBy(info => info.Name);
                }
                else if (DatagridSortColumn == "PID")
                {
                    SortedList = (List<ProcessInfo>)UnsortedList.OrderBy(info => info.PID);
                }
                else if (DatagridSortColumn == Properties.Resources.ProcessDescriptionHeader)
                {
                    SortedList = (List<ProcessInfo>)UnsortedList.OrderBy(info => info.Description);
                }
                else if (DatagridSortColumn == Properties.Resources.ProcessStartDateTimeHeader)
                {
                    SortedList = (List<ProcessInfo>)UnsortedList.OrderBy(info => info.StartTime);
                }
            }
            else
            {
                if (DatagridSortColumn == Properties.Resources.ProcessNameHeader)
                {
                    SortedList = (List<ProcessInfo>)UnsortedList.OrderByDescending(info => info.Name);
                }
                else if (DatagridSortColumn == "PID")
                {
                    SortedList = (List<ProcessInfo>)UnsortedList.OrderByDescending(info => info.PID);
                }
                else if (DatagridSortColumn == Properties.Resources.ProcessDescriptionHeader)
                {
                    SortedList = (List<ProcessInfo>)UnsortedList.OrderByDescending(info => info.Description);
                }
                else if (DatagridSortColumn == Properties.Resources.ProcessStartDateTimeHeader)
                {
                    SortedList = (List<ProcessInfo>)UnsortedList.OrderByDescending(info => info.StartTime);
                }
            }
            return SortedList;
        }
        #endregion
        #endregion
        /// <summary>
        /// Aggiorna i dati dei processi.
        /// </summary>
        /// <param name="UpdateType">Tipo di aggiornamento da eseguire.</param>
        /// <param name="ProcessID">Processo da aggiungere o da rimuovere, usato quando <paramref name="UpdateType"/> ha valore AddProcess, RemoveProcess, UpdateThreadsNew oppure UpdateThreadsEnd.</param>
        /// <param name="ProcessCommandLine">Linea di comando del processo, usato quando <paramref name="UpdateType"/> ha valore AddProcess e la fonte dei dati è ETW.</param>
        /// <param name="ProcessFullPath">Percorso completo del processo, usato quando <paramref name="UpdateType"/> ha valore AddProcess e la fonte dei dati è ETW.</param>
        /// <param name="ProcessName">Nome del processo, usato quando <paramref name="UpdateType"/> ha valore AddProcess e la fonte dei dati è ETW.</param>
        public void UpdateProcessData(string UpdateType, uint ProcessID = 0, string ProcessName = null, string ProcessFullPath = null, string ProcessCommandLine = null)
        {
            ProcessInfo Info;
            lock (Locker)
            {
                switch (UpdateType)
                {
                    case "AddProcess":
                        SafeProcessHandle ProcessHandle = GetProcessHandle(ProcessID);
                        if (!ProcessHandle.IsInvalid)
                        {
                            Info = new(ProcessHandle, ProcessID, Name: ProcessName, CommandLine: ProcessCommandLine, FullPath: ProcessFullPath);
                            if (Info.Name != Properties.Resources.UnavailableText && Application.Current != null)
                            {
                                Logger.WriteEntry(BuildLogEntryForInformation("Processo avviato", EventAction.ProcessStart, ProcessHandle));
                                UnfilteredActiveProcessesInfo.Add(Info);
                                if (Settings.WatchdogEnabled)
                                {
                                    if (!WatchdogManager.IsProcessDisallowed(Info.Name, out bool NotificationEnabled))
                                    {
                                        uint ProcessCount = (uint)UnfilteredActiveProcessesInfo.Count(info => info.Name == Info.Name);
                                        if (!WatchdogManager.IsInstanceLimitReached(Info.Name, ProcessCount))
                                        {
                                            WatchdogManager.StartProcessWatchdog(Info);
                                            WatchdogManager.UpdateProcessCPUSettings(Info);
                                            WatchdogManager.UpdateSystemWakeState(Info.Name, true);
                                        }
                                        else
                                        {
                                            _ = TerminateProcess(Info);
                                        }
                                    }
                                    else
                                    {
                                        if (TerminateProcess(Info) && NotificationEnabled)
                                        {
                                            new ToastContentBuilder().AddText(Properties.Resources.DisallowedProcessToastTitle, hintMaxLines: 1).AddText(Properties.Resources.DisallowedProcessTerminatedMessageText + Info.Name + " (" + Info.PID + ")").Show();
                                        }
                                    }
                                }
                                _ = ListAvailable.WaitOne();
                                if (!string.IsNullOrWhiteSpace(FilterValue))
                                {
                                    if (Info.Name.IndexOf(FilterValue, StringComparison.CurrentCultureIgnoreCase) is not -1 and 0)
                                    {
                                        Application.Current.Dispatcher.Invoke(() => ActiveProcessesInfo.Add(Info));
                                        if (IsDatagridSorted)
                                        {
                                            Application.Current.Dispatcher.Invoke(() => SortProcessList());
                                        }
                                    }
                                }
                                else
                                {
                                    Application.Current.Dispatcher.Invoke(() => ActiveProcessesInfo.Add(Info));
                                    if (IsDatagridSorted)
                                    {
                                        Application.Current.Dispatcher.Invoke(() => SortProcessList());
                                    }
                                }
                            }
                            else
                            {
                                Info.Dispose();
                            }
                        }
                        break;
                    case "RemoveProcess":
                        Info = UnfilteredActiveProcessesInfo.FirstOrDefault(process => process.PID == ProcessID);
                        if (Info != null && Application.Current != null)
                        {
                            Logger.WriteEntry(BuildLogEntryForInformation("Processo terminato", EventAction.ProcessTermination, new SafeProcessHandle(IntPtr.Zero, false)));
                            _ = UnfilteredActiveProcessesInfo.Remove(Info);
                            _ = ListAvailable.WaitOne();
                            if (Settings.WatchdogEnabled)
                            {
                                WatchdogManager.UpdateSystemWakeState(Info.Name, false);
                                if (WatchdogManager.ProcessMustRun(Info.Name, out bool NotificationEnabled))
                                {
                                    if (RestartProcess(Info.FullPath, Info.CommandLine) && NotificationEnabled)
                                    {
                                        new ToastContentBuilder().AddText(Properties.Resources.PermanentProcessToastTitle, hintMaxLines: 1).AddText(Properties.Resources.PermanentProcessRestartedMessageText + Info.Name).Show();
                                    }
                                }
                            }
                            Info.Dispose();
                            _ = Application.Current.Dispatcher.Invoke(() => ActiveProcessesInfo.Remove(Info));
                        }
                        break;
                    case "UpdateData":
                        _ = Parallel.ForEach(UnfilteredActiveProcessesInfo, Options, CycleActivity);
                        if (!IsTimerDisposed)
                        {
                            _ = UpdateTimer.Change(Settings.ProcessDataUpdateRate, Timeout.Infinite);
                        }
                        break;
                    case "UpdateThreadsNew":
                        Info = UnfilteredActiveProcessesInfo.FirstOrDefault(process => process.PID == ProcessID);
                        if (Info != null)
                        {
                            Info.UpdateThreadsValue("Add");
                        }
                        break;
                    case "UpdateThreadsEnd":
                        Info = UnfilteredActiveProcessesInfo.FirstOrDefault(process => process.PID == ProcessID);
                        if (Info != null)
                        {
                            Info.UpdateThreadsValue("Remove");
                        }
                        break;
                }
            }
        }
        #region Services
        /// <summary>
        /// Inizializza il monitoraggio dei servizi.
        /// </summary>
        public void InitializeServiceMonitoring()
        {
            ServicesData = new();
            LogEntry Entry = BuildLogEntryForInformation("Monitoraggio servizi avviato", EventAction.ServicesMonitoring);
            Logger.WriteEntry(Entry);
        }

        /// <summary>
        /// Interrompe il monitoraggio dei servizi.
        /// </summary>
        public void ShutdownServiceMonitoring()
        {
            ServicesData.ShutdownMonitor();
        }

        /// <summary>
        /// Abilita le notifiche quando viene creato un servizio.
        /// </summary>
        public void EnableServiceCreationNotifications()
        {
            ServicesData.ActivateServiceCreationNotifications();
            LogEntry Entry = BuildLogEntryForInformation("Abilitate notifiche per la creazione di servizi", EventAction.ServicesMonitoring);
            Logger.WriteEntry(Entry);
        }

        /// <summary>
        /// Disabilita le notifiche quando viene creato un servizio.
        /// </summary>
        public void DisableServiceCreationNotifications()
        {
            ServicesData.DeactivateServiceCreationNotifications();
            LogEntry Entry = BuildLogEntryForInformation("Disabilitate notifiche per la creazione di servizi", EventAction.ServicesMonitoring);
            Logger.WriteEntry(Entry);
        }

        /// <summary>
        /// Abilita le notifiche quando viene eliminato un servizio.
        /// </summary>
        public void EnableServiceDeletionNotifications()
        {
            ServicesData.ActivateServiceDeletionNotifications();
            LogEntry Entry = BuildLogEntryForInformation("Abilitate notifiche per l'eliminazione di servizi", EventAction.ServicesMonitoring);
            Logger.WriteEntry(Entry);
        }

        /// <summary>
        /// Disabilita le notifiche quando viene eliminato un servizio.
        /// </summary>
        public void DisableServiceDeletionNotifications()
        {
            ServicesData.DeactivateServiceDeletionNotifications();
            LogEntry Entry = BuildLogEntryForInformation("Disabilitate notifiche per l'eliminazione di servizi", EventAction.ServicesMonitoring);
            Logger.WriteEntry(Entry);
        }
        #endregion
        #region Process Debugging
        /// <summary>
        /// Aggancia un debugger a un processo.
        /// </summary>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool DebugProcess(ProcessInfo Info)
        {
            return Info is not null && Info.DebugProcess();
        }

        /// <summary>
        /// Interrompe il debug di un processo.
        /// </summary>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool StopProcessDebugging(ProcessInfo Info)
        {
            return Info is not null && Info.StopDebuggingProcess();
        }

        /// <summary>
        /// Determina se un processo è in corso di debug.
        /// </summary>
        /// <returns>true se il processo è in corso di debug, false altrimenti.</returns>
        public bool IsProcessDebugged(ProcessInfo Info)
        {
            return Info is not null && Info.IsProcessDebugged();
        }
        #endregion
        #region Process Termination
        /// <summary>
        /// Termina un processo.
        /// </summary>
        /// <param name="Info">Il processo da terminare.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool TerminateProcess(ProcessInfo Info)
        {
            return Info is not null && Info.TerminateProcess();
        }

        /// <summary>
        /// Termina un processo e tutti i suoi figli.
        /// </summary>
        /// <param name="Info">Il processo da terminare.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool TerminateProcessTree(ProcessInfo Info)
        {
            return Info is not null && Info.TerminateProcessTree();
        }
        #endregion
        #region Process Start
        /// <summary>
        /// Avvia un processo.
        /// </summary>
        public void StartProcess()
        {
            OpenFileDialog ProcessOpenFileDialog = new()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "exe",
                Multiselect = false,
                Title = Properties.Resources.ProcessOpenFileDialogTitle,
                Filter = Properties.Resources.OpenFileDialogExecutableFilter
            };
            if (ProcessOpenFileDialog.ShowDialog().Value)
            {
                if (NativeHelpers.StartProcess(ProcessOpenFileDialog.FileName))
                {
                    LogEntry Entry = BuildLogEntryForInformation("Avviato nuovo processo, nome processo: " + ProcessOpenFileDialog.SafeFileName, EventAction.OtherActions);
                    Logger.WriteEntry(Entry);
                }
                else
                {
                    _ = MessageBox.Show(Properties.Resources.StartProcessFailureErrorMessage, Properties.Resources.StartProcessFailureErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    LogEntry Entry = BuildLogEntryForWarning("Errore durante l'avvio di un nuovo processo", EventAction.OtherActions);
                    Logger.WriteEntry(Entry);
                }
            }
        }

        /// <summary>
        /// Avvia un processo come uno specifico utente.
        /// </summary>
        /// <param name="Username">Nome utente.</param>
        /// <param name="Password">Password dell'utente.</param>
        public void StartProcessAsUser(string Username, SecureString Password)
        {
            OpenFileDialog ProcessOpenFileDialog = new()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "exe",
                Multiselect = false,
                Title = Properties.Resources.ProcessOpenFileDialogTitle,
                Filter = Properties.Resources.OpenFileDialogExecutableFilter
            };
            if (ProcessOpenFileDialog.ShowDialog().Value)
            {
                ProcessStartInfo StartInfo = new(ProcessOpenFileDialog.FileName)
                {
                    UserName = Username,
                    Password = Password,
                    UseShellExecute = false
                };
                Process StartedProcess = Process.Start(StartInfo);
                if (StartedProcess is not null)
                {
                    LogEntry Entry = BuildLogEntryForInformation("Avviato nuovo processo, nome processo: " + StartedProcess.ProcessName + ", nome utente: " + Username, EventAction.OtherActions);
                    Logger.WriteEntry(Entry);
                }
                else
                {
                    _ = MessageBox.Show(Properties.Resources.StartProcessFailureErrorMessage, Properties.Resources.StartProcessFailureErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    LogEntry Entry = BuildLogEntryForWarning("Errore durante l'avvio di un nuovo processo", EventAction.OtherActions);
                    Logger.WriteEntry(Entry);
                }
                /* if (StartProcessAsUser(Username, Password, ProcessOpenFileDialog.FileName))
                 {
                     LogEntry Entry = BuildLogEntryForInformation("Avviato nuovo processo, nome processo: " + ProcessOpenFileDialog.SafeFileName + ", nome utente: " + Username, EventAction.OtherActions);
                     Logger.WriteEntry(Entry);
                 }
                 else
                 {

                 }*/

            }
        }

        /// <summary>
        /// Avvia un processo limitato.
        /// </summary>
        /// <param name="Limit">Limite di utilizzo della CPU per il processo.</param>
        public void StartLimitedProcess(CpuUsageLimitsData Limit)
        {
            OpenFileDialog ProcessOpenFileDialog = new()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "exe",
                Multiselect = false,
                Title = Properties.Resources.ProcessOpenFileDialogTitle,
                Filter = Properties.Resources.OpenFileDialogExecutableFilter
            };
            if (ProcessOpenFileDialog.ShowDialog().Value)
            {
                if (!NativeHelpers.StartLimitedProcess(Limit, ProcessOpenFileDialog.FileName, out bool? TerminationError, out bool? ThreadResumeError))
                {
                    if (TerminationError is null && ThreadResumeError is null)
                    {
                        _ = MessageBox.Show(Properties.Resources.StartProcessFailureErrorMessage, Properties.Resources.StartProcessFailureErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        if (TerminationError.Value && !ThreadResumeError.Value)
                        {
                            _ = MessageBox.Show(Properties.Resources.StartLimitedProcessJobAddNotTerminatedErrorMessage, Properties.Resources.StartProcessFailureErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else if (!TerminationError.Value && ThreadResumeError.Value)
                        {
                            _ = MessageBox.Show(Properties.Resources.StartLimitedProcessNotResumedTerminatedErrorMessage, Properties.Resources.StartProcessFailureErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else if (TerminationError.Value && ThreadResumeError.Value)
                        {
                            _ = MessageBox.Show(Properties.Resources.StartLimitedProcessNotResumedNotTerminatedErrorMessage, Properties.Resources.StartProcessFailureErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else if (!TerminationError.Value && !ThreadResumeError.Value)
                        {
                            _ = MessageBox.Show(Properties.Resources.StartLimitedProcessJobAddTerminatedErrorMessage, Properties.Resources.StartProcessFailureErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Riavvia un processo.
        /// </summary>
        /// <param name="ProcessPath">Percorso completo dell'eseguibile.</param>
        /// <param name="ProcessCommandLine">Linea di comando.</param>
        public bool RestartProcess(string ProcessPath, string ProcessCommandLine)
        {
            if (NativeHelpers.StartProcess(ProcessPath, ProcessCommandLine))
            {
                return true;
            }
            else
            {
                _ = MessageBox.Show(Properties.Resources.StartProcessFailureErrorMessage, Properties.Resources.StartProcessFailureErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        #endregion
        #region Children Enumeration
        /// <summary>
        /// Mostra i processi figlio del processo fornito.
        /// </summary>
        /// <param name="Info">Processo di cui trovare i figli.</param>
        public List<ProcessInfo> GetProcessChildren(ProcessInfo Info)
        {
            if (Info is not null)
            {
                List<uint> ChildrenPIDs = FindProcessChildren(Info.CreationTime, Info.PID, IntPtr.Zero);
                List<ProcessInfo> ChildrenInfo = new();
                if (ChildrenPIDs != null)
                {
                    foreach (uint PID in ChildrenPIDs)
                    {
                        ChildrenInfo.Add(UnfilteredActiveProcessesInfo.First(info => info.PID == PID));
                    }
                }
                return ChildrenInfo;
            }
            else
            {
                return null;
            }
        }
        #endregion
        #region Process Info
        /// <summary>
        /// Recupera informazioni generali su un processo.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="ProcessInfo"/> associata al processo di cui recuperare le informazioni.</param>
        /// <returns>Un'istanza di <see cref="ProcessGeneralInfo"/> con le informazioni richieste.</returns>
        public ProcessGeneralInfo GetProcessGeneralInformation(ProcessInfo Info)
        {
            return Info is not null ? Info.GetProcessGeneralInfo() : null;
        }

        /// <summary>
        /// Recupera le statistiche del processo.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="ProcessInfo"/> associata al processo di cui recuperare le statistiche.</param>
        /// <returns>Un'istanza di <see cref="ProcessStatistics"/> con le informazioni richieste.</returns>
        public ProcessStatistics GetProcessStatistics(ProcessInfo Info)
        {
            return Info is not null ? Info.GetProcessStatistics() : null;
        }

        /// <summary>
        /// Recupera informazioni sui thread di un processo.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="ProcessInfo"/> associata al processo di cui recuperare le informazioni sui thread.</param>
        /// <returns>Una lista con le informazioni sui thread.</returns>
        public ObservableCollection<ThreadInfo> GetProcessThreadsInfo(ProcessInfo Info)
        {
            return Info is not null ? Info.GetThreadsInfo() : new();
        }
        #endregion
        #region Process Virtualization
        /// <summary>
        /// Abilita la virtualizzazione per un processo.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="ProcessInfo"/> relativo al processo di cui abilitare la virtualizzazione.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool EnableVirtualization(ProcessInfo Info)
        {
            return Info is not null && Info.EnableVirtualization();
        }

        /// <summary>
        /// Disabilita la virtualizzazione per un processo.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="ProcessInfo"/> relativo al processo di cui disablitare la virtualizzazione.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool DisableVirtualization(ProcessInfo Info)
        {
            return Info is not null && Info.DisableVirtualization();
        }
        #endregion
        #region Process Windows
        /// <summary>
        /// Recupera informazioni sulle finestre aperte da un processo.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="ProcessInfo"/> da cui recuperare le informazioni sulle finestre aperte.</param>
        /// <returns>Un dizionario contenente le informazioni.</returns>
        public WindowInfo[] GetProcessWindowsInfo(ProcessInfo Info)
        {
            return Info is not null ? Info.GetWindowsInfo() : null;
        }
        #endregion
        #region Process Token
        /// <summary>
        /// Recupera informazioni sul token di accesso di un processo.
        /// </summary>
        /// <param name="Info">Processo dal cui token recuperare le informazioni.</param>
        /// <returns>Un istanza di <see cref="TokenInfo"/> con le informazioni.</returns>
        public TokenInfo GetProcessTokenInfo(ProcessInfo Info)
        {
            return Info is not null ? Info.GetProcessTokenInfo() : null;
        }

        /// <summary>
        /// Cambia il livello di integrità di un processo.
        /// </summary>
        /// <param name="Info">Processo su cui eseguire l'operazione.</param>
        /// <param name="NewLevel">Nuovo livello di integrità.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool ChangeProcessTokenIntegrityLevel(ProcessInfo Info, string NewLevel)
        {
            return Info is not null && Info.ChangeProcessIntegrityLevel(NewLevel);
        }
        #endregion
        #region Process Modules
        /// <summary>
        /// Recupera informazioni sui moduli caricati di un processo.
        /// </summary>
        /// <param name="Info">Processo da cui recuperare le informazioni.</param>
        /// <returns>Una lista di istanze di <see cref="ModuleInfo"/> con le informazioni sui moduli caricati.</returns>
        public List<ModuleInfo> GetProcessModulesInfo(ProcessInfo Info)
        {
            return Info?.GetProcessModulesInfo();
        }
        #endregion
        #region Process Memory
        /// <summary>
        /// Recupera informazioni sulla memoria di un processo.
        /// </summary>
        /// <param name="Info">Processo da cui recuperare le informazioni.</param>
        /// <returns>Una lista di istanze di <see cref="MemoryRegionInfo"/> con le informazioni.</returns>
        public List<MemoryRegionInfo> GetProcessMemoryInfo(ProcessInfo Info)
        {
            return Info is not null ? Info.GetProcessMemoryInfo() : new();
        }

        /// <summary>
        /// Elimina il maggior numero possibile di pagine dal working set di un processo.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="ProcessInfo"/> che rappresenta il processo.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool EmptyProcessWorkingSet(ProcessInfo Info)
        {
            return Info is not null && Info.EmptyWorkingSet();
        }

        /// <summary>
        /// Elimina il maggior numero possibile di pagine dal working set di tutti i processi.
        /// </summary>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool EmptyAllProcessesWorkingSet()
        {
            bool Result = true;
            lock (Locker)
            {
                _ = Parallel.ForEach(UnfilteredActiveProcessesInfo, info =>
                {
                    if (!info.EmptyWorkingSet())
                    {
                        Result = false;
                    }
                });
            }
            return Result;
        }
        #endregion
        #region Process Handles
        /// <summary>
        /// Recupera informazioni sugli handle di un processo.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="ProcessInfo"/> associata al processo.</param>
        /// <returns>Un array di istanze di <see cref="HandleInfo"/> con le informazioni.</returns>
        public HandleInfo[] GetProcessHandlesInfo(ProcessInfo Info)
        {
            return Info is not null ? Info.GetHandleInformation() : null;
        }
        #endregion
        #region Process .NET Performance Info
        /// <summary>
        /// Recupera informazioni sulla performance per un processo .NET.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="ProcessInfo"/> associata al processo.</param>
        /// <returns>Un istanza di <see cref="NetPerformanceInfo"/> con le informazioni.</returns>
        public NetPerformanceInfo GetProcessNetPerformanceInfo(ProcessInfo Info)
        {
            return Info is not null ? Info.GetNetPerformanceInfoForProcess() : null;
        }
        #endregion
        #region Computer Info And Power State Change
        /// <summary>
        /// Recupera l'utilizzo della CPU e della memoria.
        /// </summary>
        /// <param name="FirstCall">Indica se il metodo viene chiamato per la prima volta.</param>
        private void GetMemoryAndCpuUsage(bool FirstCall)
        {
            Dictionary<string, object> MemoryData = GetMemoryUsage();
            if (FirstCall)
            {
                ulong AvailableMemory = (ulong)MemoryData["OSAvailableMemory"] - (ulong)MemoryData["CurrentlyAvailableMemory"];
                MemoryUsageValue = UtilityMethods.ConvertSizeValueToString(AvailableMemory);
                MemoryUsagePercentageValue = (uint)MemoryData["MemoryLoadPercentage"];
                CpuUsageValue = 100 - SystemIdleProcessData.ProcessorUsage;
            }
            else
            {
                ulong AvailableMemory = (ulong)MemoryData["OSAvailableMemory"] - (ulong)MemoryData["CurrentlyAvailableMemory"];
                MemoryUsage = UtilityMethods.ConvertSizeValueToString(AvailableMemory);
                MemoryUsagePercentage = (uint)MemoryData["MemoryLoadPercentage"];
                CpuUsagePercentage = 100 - SystemIdleProcessData.ProcessorUsage;
            }
            if (!FirstCall)
            {
                if (!IsTimerDisposed)
                {
                    _ = UpdateSystemDataTimer.Change(Settings.ProcessDataUpdateRate, Timeout.Infinite);
                }
                if (Settings.WatchdogEnabled && Settings.SystemMemoryWatchdogEnabled && !MainWindow.ProgramTerminating && MemoryUsagePercentage >= Settings.MaxMemoryUsagePercentage)
                {
                    WatchdogManager.ProcessMemoryOverusageActions();
                }
            }
        }

        /// <summary>
        /// Blocca il computer.
        /// </summary>
        /// <returns>true se il blocco del computer è iniziato, false altrimenti.</returns>
        public bool LockComputer()
        {
            return NativeHelpers.LockComputer();
        }

        /// <summary>
        /// Disconnette l'utente.
        /// </summary>
        /// <returns>true se il processo di disconnessione è iniziato, false altrimenti.</returns>
        public bool LogOffUser()
        {
            return NativeHelpers.LogOffUser();
        }

        /// <summary>
        /// Causa il passaggio del computer allo stato in standby.
        /// </summary>
        /// <returns>true se il passaggio è iniziato, false altrimenti.</returns>
        public bool SuspendSystem()
        {
            return NativeHelpers.SuspendSystem();
        }

        /// <summary>
        /// Iberna il sistema.
        /// </summary>
        /// <returns>true se l'ibernazione è iniziata, false altrimenti.</returns>
        public bool HibernateSystem()
        {
            return NativeHelpers.HibernateSystem();
        }

        /// <summary>
        /// Riavvia il computer.
        /// </summary>
        /// <returns>true se il riavvio è iniziato, false altrimenti.</returns>
        public bool RestartSystem()
        {
            return NativeHelpers.RestartSystem(false);
        }

        /// <summary>
        /// Riavvia il computer e visualizza le opzioni di boot dopo il riavvio.
        /// </summary>
        /// <returns>true se il riavvio è iniziato, false altrimenti.</returns>
        public bool RestartSystemToBootOptions()
        {
            return NativeHelpers.RestartSystem(true);
        }

        /// <summary>
        /// Spegne il computer.
        /// </summary>
        /// <returns>true se lo spegnimento è iniziato, false altrimenti.</returns>
        public bool ShutdownSystem()
        {
            return NativeHelpers.ShutdownSystem(false);
        }

        /// <summary>
        /// Spegne il computer in modalità ibrida.
        /// </summary>
        /// <returns>true se lo spegnimento è iniziato, false altrimenti.</returns>
        public bool ShutdownSystemHybrid()
        {
            return NativeHelpers.ShutdownSystem(true);
        }
        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    UpdateTimer.Dispose();
                    UpdateSystemDataTimer.Dispose();
                    IsTimerDisposed = true;
                    if (!HasDataSourceChanged)
                    {
                        if (Settings.DataSource is ProcessDataSource.WMI)
                        {
                            ProcessStartEventWatcher.Stop();
                            ProcessStopEventWatcher.Stop();
                            ThreadStartEventWatcher.Stop();
                            ThreadStopEventWatcher.Stop();
                            ProcessStartEventWatcher.Dispose();
                            ProcessStopEventWatcher.Dispose();
                            ThreadStartEventWatcher.Dispose();
                            ThreadStopEventWatcher.Dispose();
                        }
                        else if (Settings.DataSource is ProcessDataSource.ETW)
                        {
                            ETWSessionControl.StopSession();
                        }
                    }
                    else
                    {
                        if (Settings.DataSource is ProcessDataSource.ETW)
                        {
                            ProcessStartEventWatcher.Stop();
                            ProcessStopEventWatcher.Stop();
                            ThreadStartEventWatcher.Stop();
                            ThreadStopEventWatcher.Stop();
                            ProcessStartEventWatcher.Dispose();
                            ProcessStopEventWatcher.Dispose();
                            ThreadStartEventWatcher.Dispose();
                            ThreadStopEventWatcher.Dispose();
                        }
                        else if (Settings.DataSource is ProcessDataSource.WMI)
                        {
                            ETWSessionControl.StopSession();
                        }
                    }
                    foreach (ProcessInfo Info in UnfilteredActiveProcessesInfo)
                    {
                        Info.Dispose();
                    }
                    ServicesData.Dispose();
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