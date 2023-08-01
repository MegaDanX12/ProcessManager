using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using static ProcessManager.NativeHelpers;
using ProcessManager.Models;

namespace ProcessManager.Watchdog
{
    /// <summary>
    /// Limitatore processi.
    /// </summary>
    public static class ProcessLimiter
    {
        /// <summary>
        /// Limiti CPU.
        /// </summary>
        public static ObservableCollection<CpuUsageLimitsData> ProcessCpuLimits { get; } = new();

        /// <summary>
        /// Arresta il limitatore processi.
        /// </summary>
        public static void ShutdownProcessLimiter()
        {
            foreach (CpuUsageLimitsData limit in ProcessCpuLimits)
            {
                limit.Dispose();
            }
        }

        /// <summary>
        /// Aggiunge ai job del limitatore processi i processi attualmente attivi che sono stati limitati.
        /// </summary>
        /// <param name="ActiveProcesses"></param>
        public static void LimitRunningProcesses(List<ProcessInfo> ActiveProcesses)
        {
            List<ProcessInfo> ProcessesToLimit = new();
            List<ProcessInfo> FoundProcesses;
            foreach (CpuUsageLimitsData limit in ProcessCpuLimits)
            {
                foreach (string path in limit.ExecutablePaths)
                {
                    FoundProcesses = ActiveProcesses.FindAll(process => process.FullPath == path);
                    ProcessesToLimit.AddRange(FoundProcesses);
                }
                foreach (ProcessInfo process in ProcessesToLimit)
                {
                    _ = limit.AddProcess(process);
                }
                ProcessesToLimit.Clear();
            }
            Logger.WriteEntry(BuildLogEntryForInformation("Iniziata limitazione processi", EventAction.ProcessLimiterInitialization));
        }

        /// <summary>
        /// Aggiunge al job associato al limite, i processi con il percorso indicato.
        /// </summary>
        /// <param name="Limit">Limite CPU.</param>
        /// <param name="Path">Percorso delle applicazioni.</param>
        /// <param name="ActiveProcesses">Processi attualmente in esecuzione.</param>
        public static void LimitRunningProcesses(CpuUsageLimitsData Limit, string Path, List<ProcessInfo> ActiveProcesses)
        {
            foreach (ProcessInfo process in ActiveProcesses)
            {
                if (process.FullPath == Path)
                {
                    _ = Limit.AddProcess(process);
                    Logger.WriteEntry(BuildLogEntryForInformation("Iniziata limitazione processo, nome processo: " + process.Name, EventAction.ProcessLimiterInitialization));
                }
            }
        }

        /// <summary>
        /// Salva le impostazioni del limitatore processi.
        /// </summary>
        public static void SaveProcessLimiterSettings()
        {
            string DocumentTextStart =
                "<ProcessLimiterSettings>" + Environment.NewLine +
                "   <Limits>" + Environment.NewLine;
            string ProcessLimiterInfoText = string.Empty;
            foreach (CpuUsageLimitsData Data in ProcessCpuLimits)
            {
                ProcessLimiterInfoText +=
                    "       <Limit>" + Environment.NewLine +
                    "           <Value>" + Data.UsageLimit.ToString("D0", CultureInfo.CurrentCulture) + "</Value>" + Environment.NewLine +
                    "           <Executables>" + Environment.NewLine;
                foreach (string path in Data.ExecutablePaths)
                {
                    ProcessLimiterInfoText +=
                        "           <Path>" + path + "</Path>" + Environment.NewLine;
                }
                ProcessLimiterInfoText +=
                    "           </Executables>" + Environment.NewLine +
                    "       </Limit>";
                DocumentTextStart += ProcessLimiterInfoText;
                ProcessLimiterInfoText = string.Empty;
            }
            string DocumentTextEnd =
                "   </Limits>" + Environment.NewLine +
                "</ProcessLimiterSettings>";
            string FullDocumentText = DocumentTextStart + DocumentTextEnd;
            XDocument Doc = XDocument.Parse(FullDocumentText);
            Doc.Save(AppDomain.CurrentDomain.BaseDirectory + "\\ProcessLimiterSettings.xml");
        }

        /// <summary>
        /// Carica le impostazioni del limitatore processi.
        /// </summary>
        public static void LoadProcessLimiterSettings()
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\ProcessLimiterSettings.xml"))
            {
                XDocument Doc = XDocument.Load(AppDomain.CurrentDomain.BaseDirectory + "\\ProcessLimiterSettings.xml");
                byte LimitValue;
                List<string> ExecutablePaths = new();
                IEnumerable<XElement> LimitElements = Doc.Descendants("Limit");
                IEnumerable<XElement> PathElements;
                IntPtr JobHandle;
                foreach (XElement element in LimitElements)
                {
                    LimitValue = Convert.ToByte(element.Element("Value").Value, CultureInfo.CurrentCulture);
                    PathElements = element.Elements("Path");
                    foreach (XElement pathelement in PathElements)
                    {
                        ExecutablePaths.Add(pathelement.Value);
                    }
                    JobHandle = CreateProcessLimiterJobObject(LimitValue);
                    if (JobHandle != IntPtr.Zero)
                    {
                        ProcessCpuLimits.Add(new(LimitValue, JobHandle, ExecutablePaths));
                    }
                    ExecutablePaths.Clear();
                }
            }
        }

        /// <summary>
        /// Aggiunge un nuovo limite.
        /// </summary>
        /// <param name="UsageLimit">Percentuale di utilizzo.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool AddNewLimit(byte UsageLimit)
        {
            IntPtr JobHandle = CreateProcessLimiterJobObject(UsageLimit);
            if (JobHandle != IntPtr.Zero)
            {
                ProcessCpuLimits.Add(new(UsageLimit, JobHandle));
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Rimuove un limite.
        /// </summary>
        /// <param name="ProcessCpuLimit">Istanza di <see cref="CpuUsageLimitsData"/> da eliminare.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool RemoveLimit(CpuUsageLimitsData ProcessCpuLimit, out bool LimitedProcessesExist)
        {
            LimitedProcessesExist = ProcessCpuLimit.LimitedProcesses.Count > 0;
            ProcessCpuLimit.Dispose();
            return ProcessCpuLimits.Remove(ProcessCpuLimit);
        }
    }
}