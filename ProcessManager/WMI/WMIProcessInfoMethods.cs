using static ProcessManager.NativeHelpers;
using System;
using System.Globalization;
using System.Linq;
using System.Management;

namespace ProcessManager.WMI
{
    /// <summary>
    /// Contiene metodi per recupera informazioni sui processi da WMI.
    /// </summary>
    public static class WMIProcessInfoMethods
    {
        /// <summary>
        /// Recupera il valore di priorità base del processo.
        /// </summary>
        /// <returns>Una stringa che rappresenta la priorità base del processo.</returns>
        public static string GetProcessBasePriority(uint PID)
        {
            using ManagementObjectSearcher Searcher = new("SELECT Priority FROM Win32_Process WHERE ProcessId = " + PID);
            using ManagementObjectCollection Processes = Searcher.Get();
            uint Priority = (uint)Processes.Cast<ManagementObject>().SingleOrDefault()["Priority"];
            return Priority.ToString("N0", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Recupera il nome del processo padre di questo processo.
        /// </summary>
        /// <returns>Il nome del processo padre oppure una stringa vuota se il processo padre è terminato.</returns>
        public static string GetParentProcessName(DateTime CreationTime, uint PID)
{
            using ManagementObjectSearcher Searcher = new("SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = " + PID);
            using ManagementObjectCollection Processes = Searcher.Get();
            uint ParentPID = (uint)Processes.Cast<ManagementObject>().SingleOrDefault()["ParentProcessId"];
            string ParentName = NativeHelpers.GetParentProcessName(ParentPID, CreationTime);
            return string.IsNullOrWhiteSpace(ParentName) ? Properties.Resources.UnavailableText : ParentName + " (" + ParentPID + ")";
        }

        /// <summary>
        /// Recupera la priorità base di un thread.
        /// </summary>
        /// <param name="TID">ID del thread.</param>
        /// <param name="PID">Id del processo a cui il thread appartiene.</param>
        /// <returns>La priorità base del thread.</returns>
        public static uint GetThreadBasePriority(uint TID, uint PID)
        {
            using ManagementObjectSearcher Searcher = new("SELECT * FROM Win32_Thread WHERE Handle = " + TID + " AND ProcessHandle = " + PID);
            using ManagementObjectCollection Threads = Searcher.Get();
            ManagementObject ThreadObject = Threads.Cast<ManagementObject>().SingleOrDefault();
            if (ThreadObject != null)
            {
                return (uint)ThreadObject.Properties["PriorityBase"].Value;
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare la priorità base di un thread, relativo oggetto WMI non trovato", EventAction.ThreadPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return 0;
            }
        }
    }
}