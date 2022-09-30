using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ProcessManager.Watchdog;

namespace ProcessManager
{
    /// <summary>
    /// Fonte dei dati dei processi.
    /// </summary>
    public enum ProcessDataSource
    {
        /// <summary>
        /// Windows Management Instrumentation.
        /// </summary>
        WMI,
        /// <summary>
        /// Event Tracing for Windows.
        /// </summary>
        ETW
    }

    /// <summary>
    /// Contiene le impostazioni del programma.
    /// </summary>
    public static class Settings
    {

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        private static void NotifyStaticPropertyChanged(string PropertyName)
        {
            StaticPropertyChanged?.Invoke(null, new(PropertyName));
        }
        #region General Settings
        private static uint ProcessDataUpdateRateValue = 500;

        /// <summary>
        /// Tempo di aggiornamento dei dati dei processi (in millisecondi).
        /// </summary>
        public static uint ProcessDataUpdateRate
        {
            get => ProcessDataUpdateRateValue;
            set
            {
                if (value != ProcessDataUpdateRateValue)
                {
                    ProcessDataUpdateRateValue = value;
                    NotifyStaticPropertyChanged(nameof(ProcessDataUpdateRate));
                }
            }
        }

        private static bool SafeModeValue = true;

        /// <summary>
        /// Indica se il programma deve essere eseguito in modalità sicura.
        /// </summary>
        /// <remarks>La modalità sicura impedisce l'esecuzione di azioni su processi di sistema, se è disattivata non verrà eseguito alcun controllo.<br/>
        /// Anche se la modalità sicura è disattivata il sistema operativo potrebbe comunque impedire l'operazione.</remarks>
        public static bool SafeMode
        {
            get => SafeModeValue;
            set
            {
                if (value != SafeModeValue)
                {
                    SafeModeValue = value;
                    NotifyStaticPropertyChanged(nameof(SafeMode));
                }
            }
        }

        private static bool AllowProcessMemoryManipulationValue;

        /// <summary>
        /// Indica se è permesso manipolare la memoria di un processo.
        /// </summary>
        public static bool AllowProcessMemoryManipulation
        {
            get => AllowProcessMemoryManipulationValue;
            set
            {
                if (AllowProcessMemoryManipulationValue != value)
                {
                    AllowProcessMemoryManipulationValue = value;
                    NotifyStaticPropertyChanged(nameof(AllowProcessMemoryManipulation));
                }
            }
        }

        private static bool ServiceMonitoringEnabledValue = true;

        /// <summary>
        /// Indica se il monitoraggio dei servizi è attivo.
        /// </summary>
        public static bool ServiceMonitoringEnabled
        {
            get => ServiceMonitoringEnabledValue;
            set
            {
                if (value != ServiceMonitoringEnabledValue)
                {
                    ServiceMonitoringEnabledValue = value;
                    NotifyStaticPropertyChanged(nameof(ServiceMonitoringEnabled));
                }
            }
        }

        private static bool DarkModeEnabledValue = true;

        /// <summary>
        /// Indica se la modalità scura dell'interfaccia utente è attiva.
        /// </summary>
        public static bool DarkModeEnabled
        {
            get => DarkModeEnabledValue;
            set
            {
                if (value != DarkModeEnabledValue)
                {
                    DarkModeEnabledValue = value;
                    NotifyStaticPropertyChanged(nameof(DarkModeEnabled));
                }
            }
        }

        private static ProcessDataSource DataSourceValue = ProcessDataSource.WMI;

        /// <summary>
        /// Fonte dei dati sui processi.
        /// </summary>
        public static ProcessDataSource DataSource
        {
            get => DataSourceValue;
            set
            {
                if (value != DataSourceValue)
                {
                    DataSourceValue = value;
                    NotifyStaticPropertyChanged(nameof(DataSource));
                }
            }
        }
        #endregion
        #region Logging Settings
        private static bool LogProgramActivityValue;

        /// <summary>
        /// Indica se eseguire il logging dell'attività del programma.
        /// </summary>
        public static bool LogProgramActivity
        {
            get => LogProgramActivityValue;
            set
            {
                if (value != LogProgramActivityValue)
                {
                    LogProgramActivityValue = value;
                    NotifyStaticPropertyChanged(nameof(LogProgramActivity));
                }
            }
        }

        private static string LogsPathValue = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Percorso di salvataggio dei log.
        /// </summary>
        public static string LogsPath
        {
            get => LogsPathValue;
            set
            {
                if (value != LogsPathValue)
                {
                    LogsPathValue = value;
                    NotifyStaticPropertyChanged(nameof(LogsPath));
                }
            }
        }

        private static bool KeepOldLogsValue = true;

        /// <summary>
        /// Indica se mantenere i vecchi log.
        /// </summary>
        public static bool KeepOldLogs
        {
            get => KeepOldLogsValue;
            set
            {
                if (value != KeepOldLogsValue)
                {
                    KeepOldLogsValue = value;
                    NotifyStaticPropertyChanged(nameof(KeepOldLogs));
                }
            }
        }

        private static uint MaxLogSizeValue = 50;

        /// <summary>
        /// Dimensione massima, in MB, di un file di log.
        /// </summary>
        public static uint MaxLogSize
        {
            get => MaxLogSizeValue;
            set
            {
                if (value != MaxLogSizeValue)
                {
                    MaxLogSizeValue = value;
                    NotifyStaticPropertyChanged(nameof(MaxLogSize));
                }
            }
        }

        private static LogLevel LoggingLevelValue = LogLevel.None;

        /// <summary>
        /// Livello di logging.
        /// </summary>
        public static LogLevel LoggingLevel
        {
            get => LoggingLevelValue;
            set
            {
                if (value != LoggingLevelValue)
                {
                    LoggingLevelValue = value;
                    NotifyStaticPropertyChanged(nameof(LoggingLevelValue));
                }
            }
        }
        #endregion
        #region Shutdown Settings
        private static bool ForceLogOffIfHungValue = true;

        /// <summary>
        /// Indice se le applicazioni che non rispondono vengono forzatamente chiuse durante la disconnessione.
        /// </summary>
        public static bool ForceLogOffIfHung
        {
            get => ForceLogOffIfHungValue;
            set
            {
                if (ForceLogOffIfHungValue != value)
                {
                    ForceLogOffIfHungValue = value;
                    NotifyStaticPropertyChanged(nameof(ForceLogOffIfHung));
                }
            }
        }

        private static bool ForceOtherSessionsLogOffOnShutdownValue;

        /// <summary>
        /// Indice se forzare la disconnessione degli altri utenti connessi durante lo spegnimento.
        /// </summary>
        public static bool ForceOtherSessionsLogOffOnShutdown
        {
            get => ForceOtherSessionsLogOffOnShutdownValue;
            set
            {
                if (ForceOtherSessionsLogOffOnShutdownValue != value)
                {
                    ForceOtherSessionsLogOffOnShutdownValue = value;
                    NotifyStaticPropertyChanged(nameof(ForceOtherSessionsLogOffOnShutdown));
                }
            }
        }

        private static bool ForceCurrentSessionLogOffOnShutdownValue;

        /// <summary>
        /// Indice se forzare la disconnessione durante lo spegnimento.
        /// </summary>
        public static bool ForceCurrentSessionLogOffOnShutdown
        {
            get => ForceCurrentSessionLogOffOnShutdownValue;
            set
            {
                if (ForceCurrentSessionLogOffOnShutdownValue != value)
                {
                    ForceCurrentSessionLogOffOnShutdownValue = value;
                    NotifyStaticPropertyChanged(nameof(ForceCurrentSessionLogOffOnShutdown));
                }
            }
        }

        private static bool InstallUpdatesBeforeShutdownValue = true;

        /// <summary>
        /// Indica se installare gli aggiornamenti prima dello spegnimento.
        /// </summary>
        public static bool InstallUpdatesBeforeShutdown
        {
            get => InstallUpdatesBeforeShutdownValue;
            set
            {
                if (InstallUpdatesBeforeShutdownValue != value)
                {
                    InstallUpdatesBeforeShutdownValue = value;
                    NotifyStaticPropertyChanged(nameof(InstallUpdatesBeforeShutdown));
                }
            }
        }

        private static bool ManualPowerDownValue;

        /// <summary>
        /// Indica se il computer deve essere spento manualmente.
        /// </summary>
        public static bool ManualPowerDown
        {
            get => ManualPowerDownValue;
            set
            {
                if (ManualPowerDownValue != value)
                {
                    ManualPowerDownValue = value;
                    NotifyStaticPropertyChanged(nameof(ManualPowerDown));
                }
            }
        }
        #endregion
        #region Update Settings

        private static bool AutomaticUpdatesCheckValue = true;

        /// <summary>
        /// Indica se aggiornare automaticamente il programma.
        /// </summary>
        public static bool AutomaticUpdatesCheck
        {
            get => AutomaticUpdatesCheckValue;
            set
            {
                if (AutomaticUpdatesCheckValue != value)
                {
                    AutomaticUpdatesCheckValue = value;
                    NotifyStaticPropertyChanged(nameof(AutomaticUpdatesCheck));
                }
            }
        }

        /// <summary>
        /// Frequenze di controllo aggiornamenti.
        /// </summary>
        public enum UpdateCheckRateValues
        {
            Day,
            Week,
            Month,
        }

        private static UpdateCheckRateValues UpdateCheckRateValue = UpdateCheckRateValues.Week;

        /// <summary>
        /// Frequenza di controllo aggiornamenti.
        /// </summary>
        public static UpdateCheckRateValues UpdateCheckRate
        {
            get => UpdateCheckRateValue;
            set
            {
                if (UpdateCheckRateValue != value)
                {
                    UpdateCheckRateValue = value;
                    NotifyStaticPropertyChanged(nameof(UpdateCheckRate));
                }
            }
        }

        private static DateTime UpdateHourValue = new(1, 1, 1, 15, 0, 0);

        /// <summary>
        /// Orario in cui eseguire l'aggiornamento.
        /// </summary>
        public static DateTime UpdateHour
        {
            get => UpdateHourValue;
            set
            {
                if (UpdateHourValue != value)
                {
                    UpdateHourValue = value;
                    NotifyStaticPropertyChanged(nameof(UpdateHour));
                }
            }
        }

        /// <summary>
        /// Giorni della settimana.
        /// </summary>
        public enum WeekDay
        {
            Monday,
            Tuesday,
            Wednesday,
            Thursday,
            Friday,
            Saturday,
            Sunday
        }

        private static WeekDay UpdateDayOfWeekValue = WeekDay.Monday;

        /// <summary>
        /// Giorno della settimana in cui controllare gli aggiornamenti.
        /// </summary>
        public static WeekDay UpdateDayOfWeek
        {
            get => UpdateDayOfWeekValue;
            set
            {
                if (UpdateDayOfWeekValue != value)
                {
                    UpdateDayOfWeekValue = value;
                    NotifyStaticPropertyChanged(nameof(UpdateDayOfWeek));
                }
            }
        }

        private static bool UpdateDownloadOnlyValue;

        /// <summary>
        /// Indica se scaricare gli aggiornamenti senza installarli.
        /// </summary>
        public static bool UpdateDownloadOnly
        {
            get => UpdateDownloadOnlyValue;
            set
            {
                if (UpdateDownloadOnlyValue != value)
                {
                    UpdateDownloadOnlyValue = value;
                    NotifyStaticPropertyChanged(nameof(UpdateDownloadOnly));
                }
            }
        }

        private static bool UpdateDownloadCompletedNotificationsValue;

        /// <summary>
        /// Indica se visualizzare una notifica quando il download degli aggiornamenti è completato.
        /// </summary>
        public static bool UpdateDownloadCompletedNotifications
        {
            get => UpdateDownloadCompletedNotificationsValue;
            set
            {
                if (UpdateDownloadCompletedNotificationsValue != value)
                {
                    UpdateDownloadCompletedNotificationsValue = value;
                    NotifyStaticPropertyChanged(nameof(UpdateDownloadCompletedNotifications));
                }
            }
        }

        private static bool UpdateDownloadAfterConfirmationValue;

        /// <summary>
        /// Indica se chiedere conferma all'utente prima di iniziare il download degli aggiornamenti.
        /// </summary>
        public static bool UpdateDownloadAfterConfirmation
        {
            get => UpdateDownloadAfterConfirmationValue;
            set
            {
                if (UpdateDownloadAfterConfirmationValue != value)
                {
                    UpdateDownloadAfterConfirmationValue = value;
                    NotifyStaticPropertyChanged(nameof(UpdateDownloadAfterConfirmation));
                }
            }
        }

        private static bool UpdateInstallStartedNotificationsValue = true;

        /// <summary>
        /// Indica se notificare l'utente prima di iniziare l'installazione degli aggiornamenti.
        /// </summary>
        public static bool UpdateInstallStartedNotifications
        {
            get => UpdateInstallStartedNotificationsValue;
            set
            {
                if (UpdateInstallStartedNotificationsValue != value)
                {
                    UpdateInstallStartedNotificationsValue = value;
                    NotifyStaticPropertyChanged(nameof(UpdateInstallStartedNotifications));
                }
            }
        }

        private static bool UpdateInstallAfterConfirmationValue;

        /// <summary>
        /// Indica se chiedere conferma prima di installare gli aggiornamenti.
        /// </summary>
        public static bool UpdateInstallAfterConfirmation
        {
            get => UpdateInstallAfterConfirmationValue;
            set
            {
                if (UpdateInstallAfterConfirmationValue != value)
                {
                    UpdateInstallAfterConfirmationValue = value;
                    NotifyStaticPropertyChanged(nameof(UpdateInstallAfterConfirmation));
                }
            }
        }

        private static bool RestartAfterUpdateValue = true;

        /// <summary>
        /// Indica se riavviare l'applicazione dopo l'aggiornamento.
        /// </summary>
        public static bool RestartAfterUpdate
        {
            get => RestartAfterUpdateValue;
            set
            {
                if (RestartAfterUpdateValue != value)
                {
                    RestartAfterUpdateValue = value;
                    NotifyStaticPropertyChanged(nameof(RestartAfterUpdate));
                }
            }
        }
        #endregion
        #region Watchdog Settings
        private static bool WatchdogEnabledValue = true;

        /// <summary>
        /// Indica se il watchdog è abilitato.
        /// </summary>
        public static bool WatchdogEnabled
        {
            get => WatchdogEnabledValue;
            set
            {
                if (WatchdogEnabledValue != value)
                {
                    WatchdogEnabledValue = value;
                    NotifyStaticPropertyChanged(nameof(WatchdogEnabled));
                }
            }
        }

        private static bool ProcessWatchdogEnabledValue = true;

        /// <summary>
        /// Indica se il watchdog processi è abilitato.
        /// </summary>
        public static bool ProcessWatchdogEnabled
        {
            get => ProcessWatchdogEnabledValue;
            set
            {
                if (ProcessWatchdogEnabledValue != value)
                {
                    ProcessWatchdogEnabledValue = value;
                    NotifyStaticPropertyChanged(nameof(ProcessWatchdogEnabled));
                }
            }
        }

        private static bool SystemMemoryWatchdogEnabledValue = true;

        /// <summary>
        /// Indica se il watchdog memoria è abilitato.
        /// </summary>
        public static bool SystemMemoryWatchdogEnabled
        {
            get => SystemMemoryWatchdogEnabledValue;
            set
            {
                if (SystemMemoryWatchdogEnabledValue != value)
                {
                    SystemMemoryWatchdogEnabledValue = value;
                    NotifyStaticPropertyChanged(nameof(SystemMemoryWatchdogEnabled));
                }
            }
        }

        private static uint MaxMemoryUsagePercentageValue = 90;

        /// <summary>
        /// Percentuale massima di utilizzo della memoria.
        /// </summary>
        public static uint MaxMemoryUsagePercentage
        {
            get => MaxMemoryUsagePercentageValue;
            set
            {
                if (MaxMemoryUsagePercentageValue != value)
                {
                    MaxMemoryUsagePercentageValue = value;
                    NotifyStaticPropertyChanged(nameof(MaxMemoryUsagePercentage));
                }
            }
        }

        private static bool EmptyRunningProcessesWorkingSetValue = true;

        /// <summary>
        /// Indica se pulire il working set dei processi quando la memoria supera il massimo indicato da <see cref="MaxMemoryUsagePercentage"/>.
        /// </summary>
        public static bool EmptyRunningProcessesWorkingSet
        {
            get => EmptyRunningProcessesWorkingSetValue;
            set
            {
                if (EmptyRunningProcessesWorkingSetValue != value)
                {
                    EmptyRunningProcessesWorkingSetValue = value;
                    NotifyStaticPropertyChanged(nameof(EmptyRunningProcessesWorkingSet));
                }
            }
        }

        private static bool TerminateProcessesHighMemoryUsageValue;

        /// <summary>
        /// Indica se terminare i processi il cui utilizzo di memoria supera il limite indicato da <see cref="MaxProcessMemoryUsage"/>.
        /// </summary>
        public static bool TerminateProcessesHighMemoryUsage
        {
            get => TerminateProcessesHighMemoryUsageValue;
            set
            {
                if (TerminateProcessesHighMemoryUsageValue != value)
                {
                    TerminateProcessesHighMemoryUsageValue = value;
                    NotifyStaticPropertyChanged(nameof(TerminateProcessesHighMemoryUsage));
                }
            }
        }

        private static uint MaxProcessMemoryUsageValue = 3072;

        /// <summary>
        /// Utilizzo massimo di memoria, in MB, da parte di un processo.
        /// </summary>
        public static uint MaxProcessMemoryUsage
        {
            get => MaxProcessMemoryUsageValue;
            set
            {
                if (MaxProcessMemoryUsageValue != value)
                {
                    MaxProcessMemoryUsageValue = value;
                    NotifyStaticPropertyChanged(nameof(MaxProcessMemoryUsage));
                }
            }
        }

        private static bool TerminateNamedProcessesValue;

        /// <summary>
        /// Indica se terminare i processi indicati da <see cref="WatchdogManager.ProcessNamesList"/>.
        /// </summary>
        public static bool TerminateNamedProcesses
        {
            get => TerminateNamedProcessesValue;
            set
            {
                if (TerminateNamedProcessesValue != value)
                {
                    TerminateNamedProcessesValue = value;
                    NotifyStaticPropertyChanged(nameof(TerminateNamedProcesses));
                }
            }
        }

        private static bool EnableLowSystemMemoryConditionMonitoringValue = true;

        /// <summary>
        /// Indica se abilitare il monitoraggio della memoria per rilevare una condizione di bassa disponibilità.
        /// </summary>
        public static bool EnableLowSystemMemoryConditionMonitoring
        {
            get => EnableLowSystemMemoryConditionMonitoringValue;
            set
            {
                if (EnableLowSystemMemoryConditionMonitoringValue != value)
                {
                    EnableLowSystemMemoryConditionMonitoringValue = value;
                    NotifyStaticPropertyChanged(nameof(EnableLowSystemMemoryConditionMonitoring));
                }
            }
        }

        private static bool ShowNotificationForLowMemoryConditionValue = true;

        /// <summary>
        /// Indica se visualizzare una notifica quando la disponibilità di memoria è bassa.
        /// </summary>
        public static bool ShowNotificationForLowMemoryCondition
        {
            get => ShowNotificationForLowMemoryConditionValue;
            set
            {
                if (ShowNotificationForLowMemoryConditionValue != value)
                {
                    ShowNotificationForLowMemoryConditionValue = value;
                    NotifyStaticPropertyChanged(nameof(ShowNotificationForLowMemoryCondition));
                }
            }
        }

        private static bool CleanSystemMemoryIfLowValue;

        /// <summary>
        /// Indica se pulire la memoria quando la disponibilità è bassa.
        /// </summary>
        public static bool CleanSystemMemoryIfLow
        {
            get => CleanSystemMemoryIfLowValue;
            set
            {
                if (CleanSystemMemoryIfLowValue != value)
                {
                    CleanSystemMemoryIfLowValue = value;
                    NotifyStaticPropertyChanged(nameof(CleanSystemMemoryIfLow));
                }
            }
        }
        #endregion
        /// <summary>
        /// Aggiorna il file delle impostazioni.
        /// </summary>
        public static void UpdateSettingsFile()
        {
            XDocument Doc = XDocument.Load(AppDomain.CurrentDomain.BaseDirectory + "\\Settings.xml");
            XElement Node = Doc.Descendants("ProcessDataUpdateRate").SingleOrDefault();
            Node.Value = ProcessDataUpdateRate.ToString("D0", CultureInfo.InvariantCulture);
            Node = Doc.Descendants("SafeMode").SingleOrDefault();
            Node.Value = SafeMode.ToString();
            Node = Doc.Descendants("AllowProcessMemoryManipulation").SingleOrDefault();
            Node.Value = AllowProcessMemoryManipulation.ToString();
            Node = Doc.Descendants("ServiceMonitoringEnabled").SingleOrDefault();
            Node.Value = ServiceMonitoringEnabled.ToString();
            Node = Doc.Descendants("LogProgramActivity").SingleOrDefault();
            Node.Value = LogProgramActivity.ToString();
            Node = Doc.Descendants("LogsPath").SingleOrDefault();
            Node.Value = LogsPath;
            Node = Doc.Descendants("KeepOldLogs").SingleOrDefault();
            Node.Value = KeepOldLogs.ToString();
            Node = Doc.Descendants("MaxLogSize").SingleOrDefault();
            Node.Value = MaxLogSize.ToString("D0", CultureInfo.InvariantCulture);
            Node = Doc.Descendants("LogLevel").SingleOrDefault();
            Node.Value = Enum.GetName(typeof(LogLevel), LoggingLevel);
            Node = Doc.Descendants("ForceLogOffIfHung").SingleOrDefault();
            Node.Value = ForceLogOffIfHung.ToString();
            Node = Doc.Descendants("ForceOtherSessionsLogOffOnShutdown").SingleOrDefault();
            Node.Value = ForceOtherSessionsLogOffOnShutdown.ToString();
            Node = Doc.Descendants("ForceCurrentSessionLogOffOnShutdown").SingleOrDefault();
            Node.Value = ForceCurrentSessionLogOffOnShutdown.ToString();
            Node = Doc.Descendants("InstallUpdatesBeforeShutdown").SingleOrDefault();
            Node.Value = InstallUpdatesBeforeShutdown.ToString();
            Node = Doc.Descendants("ManualPowerDown").SingleOrDefault();
            Node.Value = ManualPowerDown.ToString();
            Node = Doc.Descendants("AutomaticUpdatesCheck").SingleOrDefault();
            Node.Value = AutomaticUpdatesCheck.ToString();
            Node = Doc.Descendants("CheckRate").SingleOrDefault();
            Node.Value = UpdateCheckRate.ToString();
            Node = Doc.Descendants("Hour").SingleOrDefault();
            Node.Value = UpdateHour.ToShortTimeString();
            Node = Doc.Descendants("DayOfWeek").SingleOrDefault();
            Node.Value = UpdateDayOfWeek.ToString();
            Node = Doc.Descendants("DownloadOnly").SingleOrDefault();
            Node.Value = UpdateDownloadOnly.ToString();
            Node = Doc.Descendants("DownloadCompletedNotifications").SingleOrDefault();
            Node.Value = UpdateDownloadCompletedNotifications.ToString();
            Node = Doc.Descendants("DownloadAfterConfirmation").SingleOrDefault();
            Node.Value = UpdateDownloadAfterConfirmation.ToString();
            Node = Doc.Descendants("InstallStartedNotifications").SingleOrDefault();
            Node.Value = UpdateInstallStartedNotifications.ToString();
            Node = Doc.Descendants("InstallAfterConfirmation").SingleOrDefault();
            Node.Value = UpdateInstallAfterConfirmation.ToString();
            Node = Doc.Descendants("RestartAfterUpdate").SingleOrDefault();
            Node.Value = RestartAfterUpdate.ToString();
            Node = Doc.Descendants("DataSource").SingleOrDefault();
            Node.Value = DataSource.ToString();
            Doc.Save(AppDomain.CurrentDomain.BaseDirectory + "\\Settings.xml");
        }

        /// <summary>
        /// Carica le impostazioni del programma.
        /// </summary>
        public static void LoadSettings()
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\Settings.xml"))
            {
                XDocument Doc = XDocument.Load(AppDomain.CurrentDomain.BaseDirectory + "\\Settings.xml");
                XElement Node = Doc.Descendants("ProcessDataUpdateRate").SingleOrDefault();
                ProcessDataUpdateRate = Convert.ToUInt32(Node.Value, CultureInfo.CurrentCulture);
                Node = Doc.Descendants("LogProgramActivity").SingleOrDefault();
                LogProgramActivity = Convert.ToBoolean(Node.Value, CultureInfo.CurrentCulture);
                Node = Doc.Descendants("SafeMode").SingleOrDefault();
                SafeMode = Convert.ToBoolean(Node.Value, CultureInfo.CurrentCulture);
                Node = Doc.Descendants("AllowProcessMemoryManipulation").SingleOrDefault();
                AllowProcessMemoryManipulation = Convert.ToBoolean(Node.Value, CultureInfo.CurrentCulture);
                Node = Doc.Descendants("ServiceMonitoringEnabled").SingleOrDefault();
                ServiceMonitoringEnabled = Convert.ToBoolean(Node.Value, CultureInfo.CurrentCulture);
                Node = Doc.Descendants("LogsPath").SingleOrDefault();
                LogsPath = Node.Value;
                Node = Doc.Descendants("KeepOldLogs").SingleOrDefault();
                KeepOldLogs = Convert.ToBoolean(Node.Value, CultureInfo.CurrentCulture);
                Node = Doc.Descendants("MaxLogSize").SingleOrDefault();
                MaxLogSize = Convert.ToUInt32(Node.Value, CultureInfo.CurrentCulture);
                Node = Doc.Descendants("LogLevel").SingleOrDefault();
                LoggingLevel = (LogLevel)Enum.Parse(typeof(LogLevel), Node.Value);
                Node = Doc.Descendants("ForceLogOffIfHung").SingleOrDefault();
                ForceLogOffIfHung = Convert.ToBoolean(Node.Value, CultureInfo.CurrentCulture);
                Node = Doc.Descendants("ForceOtherSessionsLogOffOnShutdown").SingleOrDefault();
                ForceOtherSessionsLogOffOnShutdown = Convert.ToBoolean(Node.Value, CultureInfo.CurrentCulture);
                Node = Doc.Descendants("ForceCurrentSessionLogOffOnShutdown").SingleOrDefault();
                ForceCurrentSessionLogOffOnShutdown = Convert.ToBoolean(Node.Value, CultureInfo.CurrentCulture);
                Node = Doc.Descendants("InstallUpdatesBeforeShutdown").SingleOrDefault();
                InstallUpdatesBeforeShutdown = Convert.ToBoolean(Node.Value, CultureInfo.CurrentCulture);
                Node = Doc.Descendants("ManualPowerDown").SingleOrDefault();
                ManualPowerDown = Convert.ToBoolean(Node.Value, CultureInfo.CurrentCulture);
                Node = Doc.Descendants("AutomaticUpdatesCheck").SingleOrDefault();
                AutomaticUpdatesCheck = Convert.ToBoolean(Node.Value, CultureInfo.CurrentCulture);
                Node = Doc.Descendants("CheckRate").SingleOrDefault();
                UpdateCheckRate = (Settings.UpdateCheckRateValues)Convert.ToInt32(Node.Value);
                Node = Doc.Descendants("Hour").SingleOrDefault();
                string[] HourComponents = Node.Value.Split(':');
                UpdateHour = new(1, 1, 1, Convert.ToInt32(HourComponents[0], CultureInfo.CurrentCulture), Convert.ToInt32(HourComponents[1], CultureInfo.CurrentCulture), 0);
                Node = Doc.Descendants("DayOfWeek").SingleOrDefault();
                UpdateDayOfWeek = (Settings.WeekDay)Convert.ToInt32(Node.Value, CultureInfo.CurrentCulture);
                Node = Doc.Descendants("DownloadOnly").SingleOrDefault();
                UpdateDownloadOnly = Convert.ToBoolean(Node.Value, CultureInfo.CurrentCulture);
                Node = Doc.Descendants("DownloadCompletedNotifications").SingleOrDefault();
                UpdateDownloadCompletedNotifications = Convert.ToBoolean(Node.Value, CultureInfo.CurrentCulture);
                Node = Doc.Descendants("DownloadAfterConfirmation").SingleOrDefault();
                UpdateDownloadAfterConfirmation = Convert.ToBoolean(Node.Value, CultureInfo.CurrentCulture);
                Node = Doc.Descendants("InstallStartedNotifications").SingleOrDefault();
                UpdateInstallStartedNotifications = Convert.ToBoolean(Node.Value, CultureInfo.CurrentCulture);
                Node = Doc.Descendants("InstallAfterConfirmation").SingleOrDefault();
                UpdateInstallAfterConfirmation = Convert.ToBoolean(Node.Value, CultureInfo.CurrentCulture);
                Node = Doc.Descendants("RestartAfterUpdate").SingleOrDefault();
                RestartAfterUpdate = Convert.ToBoolean(Node.Value, CultureInfo.CurrentCulture);
                Node = Doc.Descendants("DataSource").SingleOrDefault();
                DataSource = (ProcessDataSource)Convert.ToInt32(Node.Value, CultureInfo.CurrentCulture);
            }
        }
    }
}