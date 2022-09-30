using ProcessManager.ViewModels;
using ProcessManager.Models;
using static ProcessManager.NativeHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Collections.Concurrent;
using Microsoft.Toolkit.Uwp.Notifications;

namespace ProcessManager.Watchdog
{
    /// <summary>
    /// Gestore del watchdog.
    /// </summary>
    public static class WatchdogManager
    {
        #region Data Fields
        /// <summary>
        /// Regole del watchdog (CPU e memoria).
        /// </summary>
        public static ObservableCollection<ProcessWatchdogRule> Rules { get; } = new();

        /// <summary>
        /// Dizionario con le informazioni che descrivono la configurazione del watchdog per uno specifico processo.
        /// </summary>
        private static readonly ConcurrentDictionary<ProcessWatchdogConfiguration, CancellationTokenSource> CurrentMonitoredProcesses = new();

        /// <summary>
        /// Lista dei nomi di processi da terminare quando l'utilizzo della memoria di sistema supera il massimo.
        /// </summary>
        public static ObservableCollection<string> ProcessNamesList { get; } = new();

        /// <summary>
        /// Impostazioni CPU predefinite dei processi.
        /// </summary>
        public static ObservableCollection<ProcessDefaultCPUSettings> ProcessCPUDefaults { get; } = new();

        /// <summary>
        /// Impostazioni di utilizzo energetico per i processi.
        /// </summary>
        public static ObservableCollection<ProcessEnergyUsage> ProcessEnergyUsageData { get; } = new();

        /// <summary>
        /// Impostazioni di utilizzo energetico attualmente attivate.
        /// </summary>
        private static List<ProcessEnergyUsage> ActiveEnergyUsageSettings { get; } = new();

        /// <summary>
        /// Informazioni sui limiti massimi di istanze dei processi.
        /// </summary>
        public static ObservableCollection<ProcessInstanceLimit> ProcessInstanceLimits { get; } = new();

        /// <summary>
        /// Processi la cui esecuzione non è permessa.
        /// </summary>
        public static ObservableCollection<DisallowedProcess> DisallowedProcesses { get; } = new();

        /// <summary>
        /// Processi che devono rimanere in esecuzione.
        /// </summary>
        public static ObservableCollection<PermanentProcess> PermanentProcesses { get; } = new();
        #endregion
        #region Utility Fields
        /// <summary>
        /// Indica se il watchdog è abilitato.
        /// </summary>
        public static bool IsWatchdogEnabled { get; private set; }

        /// <summary>
        /// Istanza del viewmodel <see cref="ProcessInfoVM"/>.
        /// </summary>
        public static ProcessInfoVM ProcessesData { get; private set; }

        /// <summary>
        /// Handle nativo a un oggetto di notifica stato della memoria
        /// </summary>
        private static IntPtr MemoryResourceNotificationHandle;

        /// <summary>
        /// Evento per l'arresto del monitoraggio della memoria.
        /// </summary>
        private static ManualResetEvent MemoryWatchTerminationEvent;
        #endregion
        #region Synchronization Objects
        /// <summary>
        /// Oggetto di sincronizzazione per la lista delle regole del watchdog processi.
        /// </summary>
        private static readonly object RulesListLocker = new();

        /// <summary>
        /// Oggetto di sincronizzazione per la lista di nomi di processi da terminare in caso di utilizzo eccessivo della memoria.
        /// </summary>
        private static readonly object ProcessNamesListLocker = new();

        /// <summary>
        /// Oggetto di sincronizzazione per la lista di impostazioni predefinite della CPU.
        /// </summary>
        private static readonly object CPUDefaultSettingsListLocker = new();

        /// <summary>
        /// Oggetto di sincronizzazione per la lista relative alle impostazioni energetiche per i processi.
        /// </summary>
        private static readonly object ProcessEnergyUsageDataListLocker = new();

        /// <summary>
        /// Oggetto di sincronizzazione per la lista relativa ai limiti massimi di istanze dei processi.
        /// </summary>
        private static readonly object ProcessInstanceLimitsListLocker = new();

        /// <summary>
        /// Oggetto di sincronizzazione per la lista di processi la cui esecuzione non è permessa.
        /// </summary>
        private static readonly object DisallowedProcessesListLocker = new();

        /// <summary>
        /// Oggetto di sincronizzazione per la lista di processi che devono rimanere in esecuzione.
        /// </summary>
        private static readonly object PermanentProcessesListLocker = new();
        #endregion
        #region Watchdog Settings Management Methods
        /// <summary>
        /// Carica le impostazioni del watchdog.
        /// </summary>
        public static void LoadWatchdogSettings()
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\WatchdogSettings.xml"))
            {
                XDocument Doc = XDocument.Load(AppDomain.CurrentDomain.BaseDirectory + "\\WatchdogSettings.xml");
                XElement WatchdogEnabledElement = Doc.Descendants("WatchdogEnabled").SingleOrDefault();
                IsWatchdogEnabled = Settings.WatchdogEnabled = Convert.ToBoolean(WatchdogEnabledElement.Value, CultureInfo.CurrentCulture);
                XElement Element = Doc.Descendants("ProcessWatchdogEnabled").SingleOrDefault();
                Settings.ProcessWatchdogEnabled = Convert.ToBoolean(Element.Value, CultureInfo.CurrentCulture);
                IEnumerable<XElement> ProcessesWatchdogDataElements = Doc.Descendants("Process");
                string ProcessName;
                bool CPUWatchdogEnabled;
                uint CPUWatchdogValue;
                uint CPUWatchdogTime;
                bool MemoryWatchdogEnabled;
                uint MemoryWatchdogValue;
                uint MemoryWatchdogTime;
                WatchdogSettings ProcessWatchdogSettings;
                WatchdogAction CpuActionType;
                ProcessInfo.ProcessPriority CPUActionPriorityValue;
                ulong CPUActionAffinityValue;
                ProcessWatchdogAction CPUAction = null;
                WatchdogAction MemoryActionType;
                ProcessWatchdogAction MemoryAction;
                ProcessWatchdogRule Rule;
                ProcessInfo.ProcessPriority? Priority;
                ulong? Affinity;
                bool KeepDisplayOn;
                bool KeepSystemInWorkingState;
                uint MaximumInstances;
                bool NotificationEnabled;
                foreach (XElement element in ProcessesWatchdogDataElements)
                {
                    ProcessName = element.Element("Name").Value;
                    CPUWatchdogEnabled = Convert.ToBoolean(element.Element("CPUWatchdog").Value, CultureInfo.CurrentCulture);
                    CPUWatchdogValue = Convert.ToUInt32(element.Element("CPUWatchdogValue").Value, CultureInfo.CurrentCulture);
                    CPUWatchdogTime = Convert.ToUInt32(element.Element("CPUWatchdogTime").Value, CultureInfo.CurrentCulture);
                    MemoryWatchdogEnabled = Convert.ToBoolean(element.Element("MemoryWatchdog").Value, CultureInfo.CurrentCulture);
                    MemoryWatchdogValue = Convert.ToUInt32(element.Element("MemoryWatchdogValue").Value, CultureInfo.CurrentCulture);
                    MemoryWatchdogTime = Convert.ToUInt32(element.Element("MemoryWatchdogTime").Value, CultureInfo.CurrentCulture);
                    ProcessWatchdogSettings = new(CPUWatchdogEnabled, CPUWatchdogValue, CPUWatchdogTime, MemoryWatchdogEnabled, MemoryWatchdogValue, MemoryWatchdogTime);
                    CpuActionType = (WatchdogAction)Enum.Parse(typeof(WatchdogAction), element.Element("CPUAction").Value);
                    switch (CpuActionType)
                    {
                        case WatchdogAction.ChangePriority:
                            CPUActionPriorityValue = (ProcessInfo.ProcessPriority)Enum.Parse(typeof(ProcessInfo.ProcessPriority), element.Element("CPUActionValue").Value);
                            CPUAction = new(CpuActionType, CPUActionPriorityValue);
                            break;
                        case WatchdogAction.ChangeAffinity:
                            CPUActionAffinityValue = Convert.ToUInt64(element.Element("CPUActionValue").Value, CultureInfo.CurrentCulture);
                            CPUAction = new(CpuActionType, CPUActionAffinityValue);
                            break;
                    }
                    MemoryActionType = (WatchdogAction)Enum.Parse(typeof(WatchdogAction), element.Element("MemoryAction").Value);
                    MemoryAction = new(MemoryActionType, null);
                    Rule = new(ProcessName, ProcessWatchdogSettings, CPUAction, MemoryAction);
                    Rules.Add(Rule);
                }
                Element = Doc.Descendants("SystemMemoryWatchdogEnabled").SingleOrDefault();
                Settings.SystemMemoryWatchdogEnabled = Convert.ToBoolean(Element.Value, CultureInfo.CurrentCulture);
                Element = Doc.Descendants("MaxMemoryUsagePercentage").SingleOrDefault();
                Settings.MaxMemoryUsagePercentage = Convert.ToUInt32(Element.Value, CultureInfo.CurrentCulture);
                Element = Doc.Descendants("EmptyRunningProcessesWorkingSet").SingleOrDefault();
                Settings.EmptyRunningProcessesWorkingSet = Convert.ToBoolean(Element.Value, CultureInfo.CurrentCulture);
                Element = Doc.Descendants("TerminateProcessesHighMemoryUsage").SingleOrDefault();
                Settings.TerminateProcessesHighMemoryUsage = Convert.ToBoolean(Element.Value, CultureInfo.CurrentCulture);
                Element = Doc.Descendants("MaxProcessMemoryUsage").SingleOrDefault();
                Settings.MaxProcessMemoryUsage = Convert.ToUInt32(Element.Value, CultureInfo.CurrentCulture);
                Element = Doc.Descendants("TerminateNamedProcesses").SingleOrDefault();
                Settings.TerminateNamedProcesses = Convert.ToBoolean(Element.Value, CultureInfo.CurrentCulture);
                IEnumerable<XElement> ProcessNameElements = Doc.Descendants("ProcessName");
                foreach (XElement element in ProcessNameElements)
                {
                    ProcessNamesList.Add(element.Value);
                }
                Element = Doc.Descendants("EnableLowSystemMemoryConditionMonitoring").SingleOrDefault();
                Settings.EnableLowSystemMemoryConditionMonitoring = Convert.ToBoolean(Element.Value, CultureInfo.CurrentCulture);
                Element = Doc.Descendants("ShowNotificationForLowMemoryCondition").SingleOrDefault();
                Settings.ShowNotificationForLowMemoryCondition = Convert.ToBoolean(Element.Value, CultureInfo.CurrentCulture);
                Element = Doc.Descendants("CleanSystemMemoryIfLow").SingleOrDefault();
                Settings.CleanSystemMemoryIfLow = Convert.ToBoolean(Element.Value, CultureInfo.CurrentCulture);
                IEnumerable<XElement> CpuDefaultSettings = Doc.Descendants("CpuSetting");
                foreach (XElement element in CpuDefaultSettings)
                {
                    ProcessName = element.Element("Name").Value;
                    string PriorityString = element.Element("Priority").Value;
                    Priority = !string.IsNullOrWhiteSpace(PriorityString)
                        ? (ProcessInfo.ProcessPriority)Enum.Parse(typeof(ProcessInfo.ProcessPriority), PriorityString)
                        : null;
                    string AffinityString = element.Element("Affinity").Value;
                    Affinity = !string.IsNullOrWhiteSpace(AffinityString) ? Convert.ToUInt64(AffinityString, CultureInfo.CurrentCulture) : (ulong?)null;
                    ProcessDefaultCPUSettings Setting = new(ProcessName, Priority, Affinity);
                    ProcessCPUDefaults.Add(Setting);
                }
                IEnumerable<XElement> EnergyUsageSettings = Doc.Descendants("EnergyUsageSetting");
                foreach (XElement element in EnergyUsageSettings)
                {
                    ProcessName = element.Element("Name").Value;
                    KeepDisplayOn = Convert.ToBoolean(element.Element("KeepDisplayOn").Value, CultureInfo.CurrentCulture);
                    KeepSystemInWorkingState = Convert.ToBoolean(element.Element("KeepSystemInWorkingState").Value, CultureInfo.CurrentCulture);
                    ProcessEnergyUsage Setting = new(ProcessName, KeepDisplayOn, KeepSystemInWorkingState);
                    ProcessEnergyUsageData.Add(Setting);
                }
                IEnumerable<XElement> InstanceLimitsSettings = Doc.Descendants("ProcessInstanceLimit");
                foreach (XElement element in InstanceLimitsSettings) 
                {
                    ProcessName = element.Element("Name").Value;
                    MaximumInstances = Convert.ToUInt32(element.Element("Limit").Value, CultureInfo.CurrentCulture);
                    ProcessInstanceLimit Limit = new(ProcessName, MaximumInstances);
                    ProcessInstanceLimits.Add(Limit);
                }
                IEnumerable<XElement> DisallowedProcesses = Doc.Descendants("DisallowedProcess");
                foreach (XElement element in DisallowedProcesses)
                {
                    ProcessName = element.Element("Name").Value;
                    NotificationEnabled = Convert.ToBoolean(element.Element("NotificationEnabled").Value, CultureInfo.CurrentCulture);
                    DisallowedProcess Process = new(ProcessName, NotificationEnabled);
                    WatchdogManager.DisallowedProcesses.Add(Process);
                }
                IEnumerable<XElement> PermanentProcesses = Doc.Descendants("PermanentProcess");
                foreach (XElement element in PermanentProcesses)
                {
                    ProcessName = element.Element("Name").Value;
                    NotificationEnabled = Convert.ToBoolean(element.Element("NotificationEnabled").Value, CultureInfo.CurrentCulture);
                    PermanentProcess Process = new(ProcessName, NotificationEnabled);
                    WatchdogManager.PermanentProcesses.Add(Process);
                }
            }
        }

        /// <summary>
        /// Salva le impostazioni del watchdog.
        /// </summary>
        public static void SaveWatchdogSettings()
        {
            string DocumentTextStart =
                "<WatchdogSettings>" + Environment.NewLine +
                "   <WatchdogEnabled>" + Settings.WatchdogEnabled.ToString(CultureInfo.CurrentCulture) + "</WatchdogEnabled>" + Environment.NewLine +
                "   <ProcessWatchdog>" + Environment.NewLine +
                "       <ProcessWatchdogEnabled>" + Settings.ProcessWatchdogEnabled.ToString(CultureInfo.CurrentCulture) + "</ProcessWatchdogEnabled>" + Environment.NewLine +
                "       <ProcessRules>" + Environment.NewLine;
            string WatchdogInfoText = string.Empty;
            foreach (ProcessWatchdogRule rule in Rules)
            {
                WatchdogInfoText +=
                    "           <Process>" + Environment.NewLine +
                    "               <Name>" + rule.ProcessName + "</Name>" + Environment.NewLine +
                    "               <CPUWatchdog>" + rule.Settings.CpuWatchdogEnabled.ToString(CultureInfo.CurrentCulture) + "</CPUWatchdog>" + Environment.NewLine +
                    "               <CPUWatchdogValue>" + rule.Settings.CpuWatchdogValue.ToString("D0", CultureInfo.CurrentCulture) + "</CPUWatchdogValue>" + Environment.NewLine +
                    "               <CPUWatchdogTime>" + rule.Settings.CpuWatchdogTime.ToString("D0", CultureInfo.CurrentCulture) + "</CPUWatchdogTime>" + Environment.NewLine +
                    "               <MemoryWatchdog>" + rule.Settings.MemoryWatchdogEnabled.ToString(CultureInfo.CurrentCulture) + "</MemoryWatchdog>" + Environment.NewLine +
                    "               <MemoryWatchdogValue>" + rule.Settings.MemoryWatchdogValue.ToString("F2", CultureInfo.CurrentCulture) + "</MemoryWatchdogValue>" + Environment.NewLine +
                    "               <MemoryWatchdogTime>" + rule.Settings.MemoryWatchdogTime.ToString("F2", CultureInfo.CurrentCulture) + "</MemoryWatchddogTime>" + Environment.NewLine +
                    "               <CPUAction>" + rule.CPUAction.ActionType.ToString() + "</CPUAction>" + Environment.NewLine;
                switch (rule.CPUAction.ActionType)
                {
                    case WatchdogAction.ChangeAffinity:
                        WatchdogInfoText +=
                            "               <CPUActionValue>" + ((ulong)rule.CPUAction.ActionValue).ToString("D0", CultureInfo.CurrentCulture) + "</CPUActionValue>" + Environment.NewLine +
                            "               <MemoryAction>" + rule.MemoryAction.ActionType.ToString() + "</MemoryAction>" + Environment.NewLine +
                            "           </Process>" + Environment.NewLine;
                        break;
                    case WatchdogAction.ChangePriority:
                        WatchdogInfoText +=
                            "               <CPUActionValue>" + ((ProcessInfo.ProcessPriority)rule.CPUAction.ActionValue).ToString() + "</CPUActionValue>" + Environment.NewLine +
                            "               <MemoryAction>" + rule.MemoryAction.ActionType.ToString() + "</MemoryAction>" + Environment.NewLine +
                            "           </Process>" + Environment.NewLine;
                        break;
                }
                DocumentTextStart += WatchdogInfoText;
                WatchdogInfoText = string.Empty;
            }
            DocumentTextStart +=
                "       </ProcessRules>" + Environment.NewLine +
                "  </ProcessWatchdog>" + Environment.NewLine +
                "   <MemoryWatchdog>" + Environment.NewLine +
                "       <SystemMemoryWatchdogEnabled>" + Settings.SystemMemoryWatchdogEnabled.ToString(CultureInfo.CurrentCulture) + "</SystemMemoryWatchdogEnabled>" + Environment.NewLine +
                "       <MaxMemoryUsagePercentage>" + Settings.MaxMemoryUsagePercentage.ToString("D0", CultureInfo.CurrentCulture) + "</MaxMemoryUsagePercentage>" + Environment.NewLine +
                "       <ProcessMemoryOverUsageActions>" + Environment.NewLine +
                "           <EmptyRunningProcessesWorkingSet>" + Settings.EmptyRunningProcessesWorkingSet.ToString(CultureInfo.CurrentCulture) + "</EmptyRunningProcessesWorkingSet>" + Environment.NewLine +
                "           <TerminateProcessesHighMemoryUsage>" + Settings.TerminateProcessesHighMemoryUsage.ToString(CultureInfo.CurrentCulture) + "</TerminateProcessesHighMemoryUsage>" + Environment.NewLine +
                "           <MaxProcessMemoryUsage>" + Settings.MaxProcessMemoryUsage.ToString("D0", CultureInfo.CurrentCulture) + "</MaxProcessMemoryUsage>" + Environment.NewLine +
                "           <TerminateNamedProcesses>" + Settings.TerminateNamedProcesses.ToString(CultureInfo.CurrentCulture) + "</TerminateNamedProcesses>" + Environment.NewLine +
                "           <ProcessNames>" + Environment.NewLine;
            foreach (string name in ProcessNamesList)
            {
                DocumentTextStart +=
                    "               <ProcessName>" + name + "</ProcessName>" + Environment.NewLine;
            }
            string DocumentTextEnd =
                "           </ProcessNames>" + Environment.NewLine +
                "       </ProcessMemoryOverUsageActions>" + Environment.NewLine +
                "       <EnableLowSystemMemoryConditionMonitoring>" + Settings.EnableLowSystemMemoryConditionMonitoring.ToString(CultureInfo.CurrentCulture) + "</EnableLowSystemMemoryConditionMonitoring>" + Environment.NewLine +
                "       <ShowNotificationForLowMemoryCondition>" + Settings.ShowNotificationForLowMemoryCondition.ToString(CultureInfo.CurrentCulture) + "</ShowNotificationForLowMemoryCondition>" + Environment.NewLine +
                "       <CleanSystemMemoryIfLow>" + Settings.CleanSystemMemoryIfLow.ToString(CultureInfo.CurrentCulture) + "</CleanSystemMemoryIfLow>" + Environment.NewLine +
                "   </MemoryWatchdog>" + Environment.NewLine +
                "   <ProcessSettings>" + Environment.NewLine +
                "       <CpuSettings>" + Environment.NewLine;
            foreach (ProcessDefaultCPUSettings setting in ProcessCPUDefaults)
            {
                DocumentTextEnd +=
                    "           <CpuSetting>" + Environment.NewLine +
                    "               <Name>" + setting.Name + "</Name>" + Environment.NewLine;
                if (setting.DefaultPriority.HasValue)
                {
                    DocumentTextEnd +=
                        "               <Priority>" + (int)setting.DefaultPriority.Value + "</Priority>" + Environment.NewLine;
                }
                else
                {
                    DocumentTextEnd +=
                        "               <Priority></Priority>" + Environment.NewLine;
                }
                if (setting.DefaultAffinity.HasValue)
                {
                    DocumentTextEnd +=
                        "               <Affinity>" + (int)setting.DefaultAffinity.Value + "</Affinity>" + Environment.NewLine;
                }
                else
                {
                    DocumentTextEnd +=
                        "               <Affinity></Affinity>" + Environment.NewLine;
                }
                DocumentTextEnd +=
                    "           </CpuSetting>" + Environment.NewLine;
            }
            DocumentTextEnd +=
                "       </CpuSettings>" + Environment.NewLine +
                "       <EnergyUsageSettings>" + Environment.NewLine;
            foreach (ProcessEnergyUsage setting in ProcessEnergyUsageData)
            {
                DocumentTextEnd +=
                    "           <EnergyUsageSetting>" + Environment.NewLine +
                    "               <Name>" + setting.Name + "</Name>" + Environment.NewLine +
                    "               <KeepDisplayOn>" + setting.KeepDisplayOn.ToString(CultureInfo.CurrentCulture) + "</KeepDisplayOn>" + Environment.NewLine +
                    "               <KeepSystemInWorkingState>" + setting.KeepSystemInWorkingState.ToString(CultureInfo.CurrentCulture) + "</KeepSystemInWorkingState>" + Environment.NewLine +
                    "           </EnergyUsageSetting>" + Environment.NewLine;
            }
            DocumentTextEnd +=
                "       </EnergyUsageSettings>" + Environment.NewLine +
                "       <InstanceLimits>" + Environment.NewLine;
            foreach (ProcessInstanceLimit limit in ProcessInstanceLimits)
            {
                DocumentTextEnd +=
                    "           <ProcessInstanceLimit>" + Environment.NewLine +
                    "               <Name>" + limit.Name + "</Name>" + Environment.NewLine +
                    "               <Limit>" + limit.InstanceLimit.ToString(CultureInfo.CurrentCulture) + "</Limit>" + Environment.NewLine +
                    "           </ProcessInstanceLimit>" + Environment.NewLine;
            }
            DocumentTextEnd +=
                "       </InstanceLimits>" + Environment.NewLine +
                "       <DisallowedProcesses>" + Environment.NewLine;
            foreach (DisallowedProcess process in DisallowedProcesses)
            {
                DocumentTextEnd +=
                    "           <DisallowedProcess>" + Environment.NewLine +
                    "               <Name>" + process.Name + "</Name>" + Environment.NewLine +
                    "               <NotificationEnabled>" + process.NotificationWhenTerminated.ToString(CultureInfo.CurrentCulture) + "</NotificationEnabled>" + Environment.NewLine +
                    "           </DisallowedProcess>" + Environment.NewLine;
            }
            DocumentTextEnd +=
                "       </DisallowedProcesses>" + Environment.NewLine +
                "       <PermanentProcesses>" + Environment.NewLine;
            foreach (PermanentProcess process in PermanentProcesses)
            {
                DocumentTextEnd +=
                    "           <PermanentProcess>" + Environment.NewLine +
                    "               <Name>" + process.Name + "</Name>" + Environment.NewLine +
                    "               <NotificationEnabled>" + process.NotificationWhenStarted.ToString(CultureInfo.CurrentCulture) + "</NotificationEnabled>" + Environment.NewLine +
                    "           </PermanentProcess>" + Environment.NewLine;
            }
            DocumentTextEnd +=
                "       </PermanentProcesses>" + Environment.NewLine +
                "   </ProcessSettings>" + Environment.NewLine +
                "</WatchdogSettings>";
            string FullDocumentText = DocumentTextStart + DocumentTextEnd;
            XDocument Doc = XDocument.Parse(FullDocumentText);
            Doc.Save(AppDomain.CurrentDomain.BaseDirectory + "\\WatchdogSettings.xml");
        }
        #endregion
        #region Process Watchdog Rules Management Methods
        /// <summary>
        /// Aggiunge una regola.
        /// </summary>
        /// <param name="Rule">Regola da aggiungere.</param>
        public static void AddRule(ProcessWatchdogRule Rule)
        {
            lock (RulesListLocker)
            {
                Rules.Add(Rule);
            }
            List<ProcessInfo> ActiveProcesses = ProcessesData.GetRunningProcesses();
            foreach (ProcessInfo info in ActiveProcesses)
            {
                if (info.Name == Rule.ProcessName)
                {
                    StartProcessWatchdog(info, Rule);
                }
            }
            Logger.WriteEntry(new("Nuova regola del watchdog aggiunta", EventSource.Application, EventSeverity.Information, EventAction.WatchdogRulesManipulation));
        }

        /// <summary>
        /// Rimuove una regola.
        /// </summary>
        /// <param name="Rule">Regola da rimuovere.</param>
        public static void RemoveRule(ProcessWatchdogRule Rule)
        {
            ProcessWatchdogConfiguration[] ActiveMonitoredProcesses = FindConfigurations(Rule);
            if (ActiveMonitoredProcesses.Length is > 0)
            {
                foreach (ProcessWatchdogConfiguration config in ActiveMonitoredProcesses)
                {
                    StopProcessWatchdog(config);
                }
            }
            lock (RulesListLocker)
            {
                _ = Rules.Remove(Rule);
            }
            Logger.WriteEntry(new("Regola del watchdog rimossa", EventSource.Application, EventSeverity.Information, EventAction.WatchdogRulesManipulation));
        }

        /// <summary>
        /// Modifica una regola.
        /// </summary>
        /// <param name="NewRule">Istanza di <see cref="ProcessWatchdogRule"/> da sotituire a quella già esistente.</param>
        /// <param name="OldRule">Istanza di <see cref="ProcessWatchdogRule"/> che deve essere eliminata.</param>
        public static void EditRule(ProcessWatchdogRule NewRule, ProcessWatchdogRule OldRule)
        {
            lock (RulesListLocker)
            {
                _ = Rules.Remove(OldRule);
                Rules.Add(NewRule);
            }
            ProcessWatchdogConfiguration[] RunningWatchdogs = FindConfigurations(OldRule);
            foreach (ProcessWatchdogConfiguration configuration in RunningWatchdogs)
            {
                _ = configuration.EditingCompleted.Reset();
                if (NewRule.Settings.CpuWatchdogEnabled)
                {
                    configuration.Settings.EnableCPUWatchdog(NewRule.Settings.CpuWatchdogValue, NewRule.Settings.CpuWatchdogTime);
                    configuration.ChangeCPUAction(NewRule.CPUAction.ActionType, NewRule.CPUAction.ActionValue);
                }
                else
                {
                    configuration.Settings.DisableCPUWatchdog();
                }
                if (NewRule.Settings.MemoryWatchdogEnabled)
                {
                    configuration.Settings.EnableMemoryWatchdog(NewRule.Settings.MemoryWatchdogValue, NewRule.Settings.MemoryWatchdogTime);
                    configuration.ChangeMemoryAction(NewRule.MemoryAction.ActionType, null);
                }
                else
                {
                    configuration.Settings.DisableMemoryWatchdog();
                }
                _ = configuration.EditingCompleted.Set();
            }
            Logger.WriteEntry(new("Modificata regola del watchdog", EventSource.Application, EventSeverity.Information, EventAction.WatchdogRulesManipulation));
        }

        /// <summary>
        /// Trova tutte le configurazioni che si riferiscono a processi attualmente monitorati dal watchdog in base alla regola fornita.
        /// </summary>
        /// <param name="Rule">Regola su cui le configurazioni si devono basare.</param>
        /// <returns>Un array di istanze di <see cref="ProcessWatchdogConfiguration"/>, associate a processi attualmente monitorati, che rispettano la regola fornita.</returns>
        private static ProcessWatchdogConfiguration[] FindConfigurations(ProcessWatchdogRule Rule)
        {
            List<ProcessWatchdogConfiguration> ActiveMonitoredProcesses = new();
            foreach (ProcessWatchdogConfiguration config in CurrentMonitoredProcesses.Keys)
            {
                if (config.IsAssociatedWithRule(Rule))
                {
                    ActiveMonitoredProcesses.Add(config);
                }
            }
            return ActiveMonitoredProcesses.ToArray();
        }
        #endregion
        /// <summary>
        /// Inizializza il watchdog.
        /// </summary>
        /// <param name="VM">Istanza di <see cref="ProcessInfoVM"/>.</param>
        public static void InitializeWatchdog(ProcessInfoVM VM)
        {
            ProcessesData = VM;
            List<ProcessInfo> RunningProcesses = ProcessesData.GetRunningProcesses();
            Dictionary<string, uint> ProcessesCount = new();
            uint Count;
            for (int i = 0; i < RunningProcesses.Count; i++)
            {
                if (!ProcessesCount.ContainsKey(RunningProcesses[i].Name))
                {
                    Count = 1;
                    for (int j = i + 1; j < RunningProcesses.Count; j++)
                    {
                        if (RunningProcesses[j].Name == RunningProcesses[i].Name)
                        {
                            Count += 1;
                        }
                    }
                    ProcessesCount.Add(RunningProcesses[i].Name, Count);
                }
            }
            foreach (ProcessInfo info in RunningProcesses)
            {
                if (!IsProcessDisallowed(info.Name, out bool NotificationEnabled))
                {
                    if (!IsInstanceLimitReached(info.Name, ProcessesCount[info.Name]))
                    {
                        UpdateProcessCPUSettings(info);
                        UpdateSystemWakeState(info.Name, true);
                    }
                    else
                    {
                        _ = info.TerminateProcess();
                    }
                }
                else
                {
                    if (info.TerminateProcess() && NotificationEnabled)
                    {
                        new ToastContentBuilder().AddText(Properties.Resources.DisallowedProcessToastTitle, hintMaxLines: 1).AddText(Properties.Resources.DisallowedProcessTerminatedMessageText + info.Name + " (" + info.PID + ")").Show();
                    }
                }
            }
            Logger.WriteEntry(BuildLogEntryForInformation("Applicate impostazioni dei processi", EventAction.WatchdogInizialization));
            if (Settings.ProcessWatchdogEnabled)
            {
                StartProcessWatchdog();
                Logger.WriteEntry(BuildLogEntryForInformation("Avviato watchdog dei processi", EventAction.WatchdogInizialization));
            }
            if (Settings.SystemMemoryWatchdogEnabled && Settings.EnableLowSystemMemoryConditionMonitoring)
            {
                InitializeMemoryMonitoring();
            }
        }
        #region Process Watchdog Methods
        /// <summary>
        /// Inizializza il watchdog processi.
        /// </summary>
        public static void StartProcessWatchdog()
        {
            List<ProcessInfo> ActiveProcesses = ProcessesData.GetRunningProcesses();
            List<ProcessInfo> ValidProcesses;
            ProcessWatchdogConfiguration Configuration;
            CancellationTokenSource CancellationToken;
            lock (RulesListLocker)
            {
                foreach (ProcessWatchdogRule rule in Rules)
                {
                    ValidProcesses = ActiveProcesses.FindAll(info => info.Name == rule.ProcessName);
                    foreach (ProcessInfo process in ValidProcesses)
                    {
                        process.Exit += Process_Exit;
                        CancellationToken = new();
                        Configuration = new(process, rule.Settings, rule.CPUAction, rule.MemoryAction, CancellationToken.Token);
                        _ = CurrentMonitoredProcesses.TryAdd(Configuration, CancellationToken);
                        _ = Task.Factory.StartNew(new Action<object>(WatchdogProcessing), Configuration, CancellationToken.Token);
                    }
                }
            }
            IsWatchdogEnabled = true;
        }

        private static void Process_Exit(object sender, EventArgs e)
        {
            if (!MainWindow.ProgramTerminating)
            {
                foreach (ProcessWatchdogConfiguration configuration in CurrentMonitoredProcesses.Keys)
                {
                    if (configuration.Process == (sender as ProcessInfo))
                    {
                        if (!configuration.IsWatchdogShuttingDown())
                        {
                            configuration.ProcessTerminating = true;
                            CurrentMonitoredProcesses[configuration].Cancel();
                            CurrentMonitoredProcesses[configuration].Dispose();
                            _ = CurrentMonitoredProcesses.TryRemove(configuration, out _);
                            Logger.WriteEntry(new("Watchdog del processo arrestato, nome processo: " + configuration.Process.Name + ", motivazione: processo terminato", EventSource.Application, EventSeverity.Information, EventAction.ProcessWatchdogShutdown));
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Inizializza il watchdog per un processo.
        /// </summary>
        /// <param name="Process">Processo di cui iniziare il monitoraggio.</param>
        /// <param name="Rule">Istanza di <see cref="ProcessWatchdogRule"/> che descrive la regola da utilizzare per avviare il watchdog del processo, parametro facoltativo.</param>
        /// <remarks>Se <paramref name="Rule"/> è nullo, il metodo cerca tra le regole definite una o più che regolano il comportamento del watchdog per il processo indicato da <paramref name="Process"/>, in caso contrario il valore del parametro viene utilizzato per avviare il watchdog del processo.</remarks>
        public static void StartProcessWatchdog(ProcessInfo Process, ProcessWatchdogRule Rule = null)
        {
            if (Rule is null)
            {
                lock (RulesListLocker)
                {
                    foreach (ProcessWatchdogRule rule in Rules)
                    {
                        if (rule.ProcessName == Process.Name)
                        {
                            Process.Exit += Process_Exit;
                            CancellationTokenSource CancellationToken = new();
                            ProcessWatchdogConfiguration Configuration = new(Process, rule.Settings, rule.CPUAction, rule.MemoryAction, CancellationToken.Token);
                            _ = CurrentMonitoredProcesses.TryAdd(Configuration, CancellationToken);
                            _ = Task.Factory.StartNew(new Action<object>(WatchdogProcessing), Configuration, CancellationToken.Token);
                            Logger.WriteEntry(new("Watchdog di un processo avviato, nome processo: " + Process.Name, EventSource.Application, EventSeverity.Information, EventAction.ProcessWatchdogInizialization));
                            break;
                        }
                    }
                }
            }
            else
            {
                Process.Exit += Process_Exit;
                CancellationTokenSource CancellationToken = new();
                ProcessWatchdogConfiguration Configuration = new(Process, Rule.Settings, Rule.CPUAction, Rule.MemoryAction, CancellationToken.Token);
                _ = CurrentMonitoredProcesses.TryAdd(Configuration, CancellationToken);
                _ = Task.Factory.StartNew(new Action<object>(WatchdogProcessing), Configuration, CancellationToken.Token);
                Logger.WriteEntry(new("Watchdog di un processo avviato, nome processo: " + Process.Name, EventSource.Application, EventSeverity.Information, EventAction.ProcessWatchdogInizialization));
            }
        }

        /// <summary>
        /// Arresta le operazioni del watchdog.
        /// </summary>
        public static void StopProcessWatchdog()
        {
            foreach (CancellationTokenSource token in CurrentMonitoredProcesses.Values)
            {
                token.Cancel();
                token.Dispose();
            }
            IsWatchdogEnabled = false;
            Logger.WriteEntry(new("Watchdog arrestato", EventSource.Application, EventSeverity.Information, EventAction.WatchdogShutdown));
        }

        /// <summary>
        /// Arresta le operaazioni del watchdog per un processo.
        /// </summary>
        /// <param name="Configuration">Istanza di <see cref="ProcessWatchdogConfiguration"/> associata al processo.</param>
        private static void StopProcessWatchdog(ProcessWatchdogConfiguration Configuration)
        {
            CurrentMonitoredProcesses[Configuration].Cancel();
            CurrentMonitoredProcesses[Configuration].Dispose();
            _ = CurrentMonitoredProcesses.TryRemove(Configuration, out _);
            Logger.WriteEntry(new("Watchdog del processo arrestato, nome processo: " + Configuration.Process.Name, EventSource.Application, EventSeverity.Information, EventAction.ProcessWatchdogShutdown));
        }

        /// <summary>
        /// Metodo operativo del watchdog.
        /// </summary>
        /// <param name="Configuration">Configurazione del watchdog per un processo.</param>
        private static void WatchdogProcessing(object Configuration)
        {
            ProcessWatchdogConfiguration WatchdogConfiguration = Configuration as ProcessWatchdogConfiguration;
            byte OverLimitSecondsCPU = 0;
            byte OverLimitSecondsMemory = 0;
            ProcessInfo.ProcessPriority FormerPriority = WatchdogConfiguration.Process.Priority;
            ulong FormerAffinity = WatchdogConfiguration.Process.Affinity;
            bool IsProcessTerminated = false;
            while (!WatchdogConfiguration.IsWatchdogShuttingDown())
            {
                _ = WatchdogConfiguration.EditingCompleted.WaitOne();
                if (!WatchdogConfiguration.ProcessTerminating)
                {
                    if (WatchdogConfiguration.Settings.CpuWatchdogEnabled)
                    {
                        if (WatchdogConfiguration.Process.ProcessorUsage > WatchdogConfiguration.Settings.CpuWatchdogValue)
                        {
                            OverLimitSecondsCPU += 1;
                            if (OverLimitSecondsCPU == WatchdogConfiguration.Settings.CpuWatchdogTime)
                            {
                                OverLimitSecondsCPU = 0;
                                if (WatchdogConfiguration.CPUAction.ActionType is WatchdogAction.ChangePriority)
                                {
                                    if (WatchdogConfiguration.Process.Priority != (ProcessInfo.ProcessPriority)WatchdogConfiguration.CPUAction.ActionValue)
                                    {
                                        _ = WatchdogConfiguration.Process.SetProcessPriority((ProcessInfo.ProcessPriority)WatchdogConfiguration.CPUAction.ActionValue);
                                    }
                                }
                                else if (WatchdogConfiguration.CPUAction.ActionType is WatchdogAction.ChangeAffinity)
                                {
                                    if (WatchdogConfiguration.Process.Affinity != (ulong)WatchdogConfiguration.CPUAction.ActionValue)
                                    {
                                        BitArray AffinityBits = new(BitConverter.GetBytes((ulong)WatchdogConfiguration.CPUAction.ActionValue));
                                        _ = WatchdogConfiguration.Process.SetProcessAffinity(AffinityBits);
                                    }
                                }
                                else if (WatchdogConfiguration.CPUAction.ActionType is WatchdogAction.TerminateProcess)
                                {
                                    if (WatchdogConfiguration.Process.TerminateProcess())
                                    {
                                        IsProcessTerminated = true;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            OverLimitSecondsCPU = 0;
                        }
                    }
                    if (WatchdogConfiguration.Settings.MemoryWatchdogEnabled)
                    {
                        if (WatchdogConfiguration.Process.PrivateMemoryBytes > WatchdogConfiguration.Settings.MemoryWatchdogValue)
                        {
                            OverLimitSecondsMemory += 1;
                            if (OverLimitSecondsMemory == WatchdogConfiguration.Settings.MemoryWatchdogTime)
                            {
                                OverLimitSecondsMemory = 0;
                                if (WatchdogConfiguration.MemoryAction.ActionType is WatchdogAction.EmptyWorkingSet)
                                {
                                    _ = WatchdogConfiguration.Process.EmptyWorkingSet();
                                }
                                else if (WatchdogConfiguration.MemoryAction.ActionType is WatchdogAction.TerminateProcess)
                                {
                                    if (WatchdogConfiguration.Process.TerminateProcess())
                                    {
                                        IsProcessTerminated = true;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            OverLimitSecondsMemory = 0;
                        }
                    }
                    Thread.Sleep(1000);
                }
                else
                {
                    IsProcessTerminated = true;
                    break;
                }
            }
            if (!IsProcessTerminated)
            {
                _ = WatchdogConfiguration.Process.SetProcessPriority(FormerPriority);
                _ = WatchdogConfiguration.Process.SetProcessAffinity(new(BitConverter.GetBytes(FormerAffinity)));
            }
        }
        #endregion
        #region Memory Watchdog Methods
        #region Memory Monitoring
        /// <summary>
        /// Inizializza il monitoraggio della memoria.
        /// </summary>
        public static void InitializeMemoryMonitoring()
        {
            MemoryResourceNotificationHandle = CreateMemoryNotificationObject();
            if (MemoryResourceNotificationHandle != IntPtr.Zero)
            {
                if (MemoryWatchTerminationEvent is null)
                {
                    MemoryWatchTerminationEvent = new(false);
                }
                _ = Task.Run(() => StartMemoryWatch(ProcessesData, MemoryResourceNotificationHandle, MemoryWatchTerminationEvent.SafeWaitHandle));
                LogEntry Entry = BuildLogEntryForInformation("Monitoraggio della memoria iniziato", EventAction.SystemMemoryMonitoring, null);
                Logger.WriteEntry(Entry);
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile inizializzare il monitoraggio della memoria", EventAction.SystemMemoryMonitoring, null);
                Logger.WriteEntry(Entry);
            }
        }

        /// <summary>
        /// Termina il monitoraggio della memoria.
        /// </summary>
        public static void StopMemoryMonitoring()
        {
            if (MemoryWatchTerminationEvent is not null && MainWindow.ProgramTerminating)
            {
                _ = MemoryWatchTerminationEvent.Set();
            }
            _ = CloseHandle(MemoryResourceNotificationHandle);
            if (MemoryWatchTerminationEvent is not null && MainWindow.ProgramTerminating)
            {
                MemoryWatchTerminationEvent.Dispose();
            }
            MemoryResourceNotificationHandle = IntPtr.Zero;
            LogEntry Entry = BuildLogEntryForInformation("Monitoraggio della memoria terminato", EventAction.SystemMemoryMonitoring, null);
            Logger.WriteEntry(Entry);
        }

        /// <summary>
        /// Determina se il monitoraggio della memoria è attivo.
        /// </summary>
        /// <returns>true se il monitoraggio è attivo, false altrimenti.</returns>
        public static bool IsSystemMemoryMonitored()
        {
            return MemoryResourceNotificationHandle != IntPtr.Zero && MemoryWatchTerminationEvent is not null;
        }
        #endregion
        #region Process Memory Overusage
        /// <summary>
        /// Esegue le azioni impostate in caso di utilizzo eccessivo della memoria.
        /// </summary>
        public static void ProcessMemoryOverusageActions()
        {
            if (Settings.EmptyRunningProcessesWorkingSet)
            {
                _ = ProcessesData.EmptyAllProcessesWorkingSet();
            }
            if (Settings.TerminateProcessesHighMemoryUsage)
            {
                TerminateProcessesWithHighMemoryUsage();
            }
            if (Settings.TerminateNamedProcesses)
            {
                lock (ProcessNamesListLocker)
                {
                    TerminateNamedProcesses();
                }
            }
        }

        /// <summary>
        /// Termina i processi con un utilizzo di memoria superiore al massimo consentito.
        /// </summary>
        private static void TerminateProcessesWithHighMemoryUsage()
        {
            ProcessInfo[] Processes = ProcessesData.FindProcessesWithHighMemoryUsage();
            foreach (ProcessInfo info in Processes)
            {
                _ = info.TerminateProcess();
            }
        }

        /// <summary>
        /// Termina i processi con i nomi indicati nel campo <see cref="ProcessNamesList"/>.
        /// </summary>
        private static void TerminateNamedProcesses()
        {
            List<ProcessInfo> FullProcessList = new();
            ProcessInfo[] Processes;
            foreach (string name in ProcessNamesList)
            {
                Processes = ProcessesData.FindProcessesWithName(name);
                FullProcessList.AddRange(Processes);
            }
            foreach (ProcessInfo info in FullProcessList)
            {
                _ = info.TerminateProcess();
            }
        }
        #endregion
        #region Process Names List Manipulation
        /// <summary>
        /// Aggiunge un processo alla lista di processi da terminare in caso di utilizzo eccessivo della memoria.
        /// </summary>
        /// <param name="ProcessName">Nome del processo.</param>
        /// <returns>true se la lista non contiene il processo indicato, false altrimenti.</returns>
        public static bool AddProcess(string ProcessName)
        {
            lock (ProcessNamesListLocker)
            {
                if (!ProcessNamesList.Contains(ProcessName))
                {
                    ProcessNamesList.Add(ProcessName);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Rimuove un processo alla lista di processi da terminare in caso di utilizzo eccessivo della memoria.
        /// </summary>
        /// <param name="ProcessName">Nome del processo.</param>
        public static void RemoveProcess(string ProcessName)
        {
            lock (ProcessNamesListLocker)
            {
                _ = ProcessNamesList.Remove(ProcessName);
            }
        }
        #endregion
        #endregion
        #region Process CPU Default Settings Management Methods
        #region CPU Default Settings List Management Methods
        /// <summary>
        /// Aggiunge un'istanza di <see cref="ProcessDefaultCPUSettings"/> che rappresenta le impostazioni predefinite della CPU per un processo.
        /// </summary>
        /// <param name="Name">Nome del processo.</param>
        /// <param name="Priority">Priorità del processo.</param>
        /// <param name="Affinity">Affinità del processo.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool AddProcessCPUDefaultSetting(string Name, ProcessInfo.ProcessPriority? Priority, ulong? Affinity)
        {
            ProcessDefaultCPUSettings Setting = new(Name, Priority, Affinity);
            lock (CPUDefaultSettingsListLocker)
            {
                if (!ProcessCPUDefaults.Contains(Setting))
                {
                    ProcessCPUDefaults.Add(Setting);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Rimuove un'istanza di <see cref="ProcessDefaultCPUSettings"/> che rappresenta le impostazioni predefinite della CPU per un processo.
        /// </summary>
        /// <param name="Setting">Istanza di <see cref="ProcessDefaultCPUSettings"/> da rimuovere.</param>
        public static void RemoveProcessCPUDefaultSetting(ProcessDefaultCPUSettings Setting)
        {
            lock (CPUDefaultSettingsListLocker)
            {
                _ = ProcessCPUDefaults.Remove(Setting);
            }
        }

        /// <summary>
        /// Modifica l'impostazione predefinita della CPU di un processo.
        /// </summary>
        /// <param name="Name">Nome del processo.</param>
        /// <param name="Priority">Nuova priorità del processo.</param>
        /// <param name="Affinity">Nuova Affinità del processo.</param>
        public static void EditProcessCPUDefaultSetting(string Name, ProcessInfo.ProcessPriority? Priority = null, ulong? Affinity = null)
        {
            lock (CPUDefaultSettingsListLocker)
            {
                ProcessDefaultCPUSettings Setting = ProcessCPUDefaults.First(setting => setting.Name == Name);
                Setting.EditSetting(Priority, Affinity);
            }
        }
        #endregion
        /// <summary>
        /// Imposta la priorità è l'affinità di un processo seguendo le impostazioni predefinite relative ad esso.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="ProcessInfo"/> associata al processo.</param>
        public static void UpdateProcessCPUSettings(ProcessInfo Info)
        {
            ProcessDefaultCPUSettings ProcessSetting = ProcessCPUDefaults.FirstOrDefault(setting => setting.Name == Info.Name);
            if (ProcessSetting is not null)
            {
                if (ProcessSetting.DefaultPriority.HasValue)
                {
                    _ = Info.SetProcessPriority(ProcessSetting.DefaultPriority.Value);
                }
                if (ProcessSetting.DefaultAffinity.HasValue)
                {
                    BitArray Bits = new(BitConverter.GetBytes(ProcessSetting.DefaultAffinity.Value));
                    _ = Info.SetProcessAffinity(Bits);
                }
            }
        }
        #endregion
        #region Process Energy Usage Management Methods
        #region Process Energy Usage List Management Methods
        /// <summary>
        /// Aggiunge un'istanza di <see cref="ProcessEnergyUsage"/> alla lista di impostazioni energetiche dei processi.
        /// </summary>
        /// <param name="Name">Nome del processo.</param>
        /// <param name="KeepDisplayOn">Indica se mantenere il display acceso.</param>
        /// <param name="KeepSystemInWorkingState">Indica se mantenere il sistema attivo.</param>
        /// <returns>true se l'istanza è stata aggiunta, false se essa esiste già.</returns>
        public static bool AddProcessEnergyUsageRule(string Name, bool KeepDisplayOn, bool KeepSystemInWorkingState)
        {
            ProcessEnergyUsage EnergyUsage = new(Name, KeepDisplayOn, KeepSystemInWorkingState);
            lock (ProcessEnergyUsageDataListLocker)
            {
                if (!ProcessEnergyUsageData.Contains(EnergyUsage))
                {
                    ProcessEnergyUsageData.Add(EnergyUsage);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Rimuove un'istanza di <see cref="ProcessEnergyUsage"/> dalla lista di impostazioni energetiche dei processi.
        /// </summary>
        /// <param name="Rule">Istanza di <see cref="ProcessEnergyUsage"/> da rimuovere.</param>
        public static void RemoveProcessEnergyUsageRule(ProcessEnergyUsage Rule)
        {
            lock (ProcessEnergyUsageDataListLocker)
            {
                _ = ProcessEnergyUsageData.Remove(Rule);
            }
        }

        /// <summary>
        /// Modifica le impostazioni di utilizzo energetico di un processo.
        /// </summary>
        /// <param name="Name">Nome del processo.</param>
        /// <param name="KeepDisplayOn">Indica se il mantenere il display accesso.</param>
        /// <param name="KeepSystemInWorkingState">Indica se mantenere il sistema attivo.</param>
        public static void EditProcessEnergyUsageRule(string Name, bool KeepDisplayOn, bool KeepSystemInWorkingState)
        {
            lock (ProcessEnergyUsageDataListLocker)
            {
                ProcessEnergyUsage EnergyUsageData = ProcessEnergyUsageData.First(data => data.Name == Name);
                EnergyUsageData.EditSetting(KeepDisplayOn, KeepSystemInWorkingState);
            }
        }
        #endregion
        /// <summary>
        /// Modifica l'utilizzo energetico del sistema in base a quanto indicato in un'istanza di <see cref="ProcessEnergyUsage"/>.
        /// </summary>
        /// <param name="Name">Nome del processo.</param>
        /// <param name="Add">Indica se l'istanza deve essere aggiunta o rimossa.</param>
        public static void UpdateSystemWakeState(string Name, bool Add)
        {
            lock (ProcessEnergyUsageDataListLocker)
            {
                ProcessEnergyUsage Data = ProcessEnergyUsageData.FirstOrDefault(data => data.Name == Name);
                if (Data is not null)
                {
                    if (Add)
                    {
                        if (!ActiveEnergyUsageSettings.Any(data => data.KeepDisplayOn == Data.KeepDisplayOn && data.KeepSystemInWorkingState == Data.KeepSystemInWorkingState))
                        {
                            _ = Data.KeepDisplayOn && Data.KeepSystemInWorkingState ? ChangeSystemWakeState(true, true) : ChangeSystemWakeState(false, true);
                        }
                        ActiveEnergyUsageSettings.Add(Data);
                    }
                    else
                    {
                        if (!ActiveEnergyUsageSettings.Any(data => data.KeepDisplayOn == Data.KeepDisplayOn && data.KeepSystemInWorkingState == Data.KeepSystemInWorkingState))
                        {
                            if (Data.KeepDisplayOn && Data.KeepSystemInWorkingState)
                            {
                                _ = !ActiveEnergyUsageSettings.Any(data => data.KeepSystemInWorkingState && !data.KeepDisplayOn)
                                    ? ChangeSystemWakeState(false, false)
                                    : ChangeSystemWakeState(false, true);
                            }
                        }
                        _ = ActiveEnergyUsageSettings.Remove(Data);
                    }
                }
            }
        }
        #endregion
        #region Process Instance Limits Management Methods
        #region Process Instance Limits List Management Methods
        /// <summary>
        /// Aggiunge un nuovo limite massima di istanze per un processo.
        /// </summary>
        /// <param name="Name">Nome del processo.</param>
        /// <param name="InstanceLimit">Limite massimo di istanze.</param>
        /// <returns>true se il limite è stato aggiunto, false se esiste già.</returns>
        public static bool AddProcessInstanceLimit(string Name, uint InstanceLimit)
        {
            ProcessInstanceLimit Limit = new(Name, InstanceLimit);
            lock (ProcessInstanceLimitsListLocker)
            {
                if (!ProcessInstanceLimits.Contains(Limit))
                {
                    bool IsPermanentProcess;
                    bool IsDisallowedProcess;
                    lock (PermanentProcessesListLocker)
                    {
                        IsPermanentProcess = PermanentProcesses.Any(process => process.Name == Limit.Name);
                    }
                    lock (DisallowedProcessesListLocker)
                    {
                        IsDisallowedProcess = DisallowedProcesses.Any(process => process.Name == Limit.Name);
                    }
                    if (!IsPermanentProcess && !IsDisallowedProcess)
                    {
                        ProcessInstanceLimits.Add(Limit);
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
        }

        /// <summary>
        /// Rimuove un limite massimo di istanze per un processo.
        /// </summary>
        /// <param name="Limit">Limite da rimuovere.</param>
        public static void RemoveProcessInstanceLimit(ProcessInstanceLimit Limit)
        {
            lock (ProcessInstanceLimitsListLocker)
            {
                _ = ProcessInstanceLimits.Remove(Limit);
            }
        }

        /// <summary>
        /// Modifica il limite massimo di instanze per un processo
        /// </summary>
        /// <param name="Name">Nome del processo.</param>
        /// <param name="InstanceLimit">Nuovo limite massimo di instanze.</param>
        public static void EditProcessInstanceLimit(string Name, uint InstanceLimit)
        {
            lock (ProcessInstanceLimitsListLocker)
            {
                ProcessInstanceLimit Limit = ProcessInstanceLimits.First(limit => limit.Name == Name);
                Limit.EditSetting(InstanceLimit);
            }
        }
        #endregion
        /// <summary>
        /// Determina se le istanze in esecuzione di un processo hanno raggiunto il massimo.
        /// </summary>
        /// <param name="Name">Nome del processo.</param>
        /// <param name="ProcessCount">Numero attuale di istanze.</param>
        /// <returns>true se il massimo numero di istanza è stato raggiunto, false altrimenti.</returns>
        public static bool IsInstanceLimitReached(string Name, uint ProcessCount)
        {
            lock (ProcessInstanceLimitsListLocker)
            {
                ProcessInstanceLimit Limit = ProcessInstanceLimits.FirstOrDefault(limit => limit.Name == Name);
                return Limit is not null && ProcessCount > Limit.InstanceLimit;
            }
        }
        #endregion
        #region Disallowed Processes Management Methods
        #region Disallowed Processes List Management Methods
        /// <summary>
        /// Aggiunge un processo alla lista di processi la cui esecuzione non è permessa.
        /// </summary>
        /// <param name="Name">Nome del processo.</param>
        /// <param name="ShowNotification">Indica se mostrare una notifica quando il processo viene terminato.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool AddDisallowedProcess(string Name, bool ShowNotification)
        {
            DisallowedProcess Process = new(Name, ShowNotification);
            lock (DisallowedProcessesListLocker)
            {
                if (!DisallowedProcesses.Contains(Process))
                {
                    PermanentProcess PermanentProcess = PermanentProcesses.FirstOrDefault(process => process.Name == Name);
                    if (PermanentProcess is not null)
                    {
                        lock (PermanentProcessesListLocker)
                        {
                            _ = PermanentProcesses.Remove(PermanentProcess);
                        }
                    }
                    else
                    {
                        ProcessInstanceLimit Limit = ProcessInstanceLimits.FirstOrDefault(process => process.Name == Name);
                        if (Limit is not null)
                        {
                            lock (ProcessInstanceLimitsListLocker)
                            {
                                _ = ProcessInstanceLimits.Remove(Limit);
                            }
                        }
                    }
                    DisallowedProcesses.Add(Process);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Rimuove un processo dalla lista di processi la cui esecuzione non è permessa.
        /// </summary>
        /// <param name="Process">Istanza di <see cref="DisallowedProcess"/> che rappresenta il processo da rimuovere.</param>
        public static void RemoveDisallowedProcess(DisallowedProcess Process)
        {
            lock (DisallowedProcessesListLocker)
            {
                _ = DisallowedProcesses.Remove(Process);
            }
        }
        #endregion
        /// <summary>
        /// Controlla se il processo con il nome fornito può essere eseguito.
        /// </summary>
        /// <param name="Name">Nome del processo.</param>
        /// <param name="NotificationEnabled">Indica se la notifica della terminazione del processo è attiva.</param>
        /// <returns>true se il processo deve essere terminato, false altrimenti.</returns>
        public static bool IsProcessDisallowed(string Name, out bool NotificationEnabled)
        {
            DisallowedProcess Process;
            lock (DisallowedProcessesListLocker)
            {
                Process = DisallowedProcesses.FirstOrDefault(process => process.Name == Name);
            }
            if (Process is not null)
            {
                NotificationEnabled = Process.NotificationWhenTerminated;
                return true;
            }
            else
            {
                NotificationEnabled = false;
                return false;
            }
        }
        #endregion
        #region Permanent Processes Management Methods
        #region Permanent Processes List Management Methods
        /// <summary>
        /// Aggiunge un nuovo processo da mantenere in esecuzione.
        /// </summary>
        /// <param name="Name">Nome del processo.</param>
        /// <param name="ShowNotificationWhenStarted">Indica se notificare quando il processo viene avviato.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool AddPermanentProcess(string Name, bool ShowNotificationWhenStarted)
        {
            PermanentProcess Process = new(Name, ShowNotificationWhenStarted);
            lock (PermanentProcessesListLocker)
            {
                if (!PermanentProcesses.Contains(Process) && !DisallowedProcesses.Any(process => process.Name == Name))
                {
                    ProcessInstanceLimit Limit = ProcessInstanceLimits.FirstOrDefault(process => process.Name == Name);
                    if (Limit is not null)
                    {
                        lock (ProcessInstanceLimitsListLocker)
                        {
                            _ = ProcessInstanceLimits.Remove(Limit);
                        }
                    }
                    PermanentProcesses.Add(Process);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Rimuove un processo da mantenere in esecuzione.
        /// </summary>
        /// <param name="Process">Istanza di <see cref="PermanentProcess"/> che rappresenta il processo.</param>
        public static void RemovePermanentProcess(PermanentProcess Process)
        {
            lock (PermanentProcessesListLocker)
            {
                _ = PermanentProcesses.Remove(Process);
            }
        }
        #endregion
        /// <summary>
        /// Controlla se il processo con il nome fornito deve rimanere in esecuzione.
        /// </summary>
        /// <param name="Name">Nome del processo.</param>
        /// <param name="NotificationEnabled">Indica se l'avvio del processo deve essere notificato.</param>
        /// <returns>true se il processo deve rimanere in esecuzione, false altrimenti.</returns>
        public static bool ProcessMustRun(string Name, out bool NotificationEnabled)
        {
            PermanentProcess Process;
            lock (PermanentProcessesListLocker)
            {
                Process = PermanentProcesses.FirstOrDefault(process => process.Name == Name);
            }
            if (Process is not null)
            {
                NotificationEnabled = Process.NotificationWhenStarted;
                return true;
            }
            else
            {
                NotificationEnabled = false;
                return false;
            }
        }
        #endregion
    }
}