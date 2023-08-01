using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessManager.Models
{
    /// <summary>
    /// Informazioni generali su un processo.
    /// </summary>
    public class ProcessGeneralInfo
    {
        /// <summary>
        /// Descrizione del processo.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Percorso completo.
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// Versione dell'applicazione.
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Linea di comando.
        /// </summary>
        public string CommandLine { get; }

        /// <summary>
        /// Tempo passato dall'avvio del processo.
        /// </summary>
        public string RunningTime { get; }

        /// <summary>
        /// Indirizzo di memoria dove si trova il PEB (Process Environment Block).
        /// </summary>
        public string PEBAddress { get; }

        /// <summary>
        /// Nome del processo padre.
        /// </summary>
        public string ParentName { get; }

        /// <summary>
        /// Politiche di mitigazione.
        /// </summary>
        public string MitigationPolicies { get; }

        /// <summary>
        /// Tipo di protezione del processo.
        /// </summary>
        public string ProtectionType { get; }

        /// <summary>
        /// Dizionario che contiene informazioni dettagliate sulle politiche di mitigazione attive.
        /// </summary>
        public Dictionary<string, string> EnabledSettings { get; private set; }

        /// <summary>
        /// Inizializza una nuova istanza della classe <see cref="ProcessGeneralInfo"/>.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <param name="Description">Descrizione del processo.</param>
        /// <param name="CreationTime">Data e ora di creazione del processo.</param>
        /// <param name="PID">ID del processo.</param>
        /// <param name="FullPath">Percorso completo.</param>
        /// <param name="Version">Versione.</param>
        /// <param name="CommandLine">Linea di comando.</param>
        /// <param name="ParentName">Nome del processo padre.</param>
        public ProcessGeneralInfo(SafeProcessHandle Handle, string Description, DateTime CreationTime, uint PID, string FullPath, string Version, string CommandLine, string ParentName)
        {
            this.Description = Description;
            this.FullPath = FullPath;
            this.Version = Version;
            this.CommandLine = CommandLine;
            RunningTime = CalculateProcessRunningTime(CreationTime);
            PEBAddress = NativeHelpers.GetProcessPEBAddress(Handle);
            this.ParentName = ParentName;
            MitigationPolicies = GetProcessMitigationPolicies(Handle);
            ProtectionType = NativeHelpers.GetProcessProtectionType(Handle);
        }

        /// <summary>
        /// Recupera le politiche di mitigazione attive per il processo.
        /// </summary>
        /// <param name="AssociatedProcessHandle">Handle al processo.</param>
        /// <returns>Le politiche attive come stringa.</returns>
        private string GetProcessMitigationPolicies(SafeProcessHandle AssociatedProcessHandle)
        {
            ProcessMitigationPoliciesData Data = NativeHelpers.GetProcessMitigationPolicies(AssociatedProcessHandle);
            if (Data != null)
            {
                string ActivePolicies = Data.GetActivePolicies(out Dictionary<string, string> EnabledSettings);
                this.EnabledSettings = EnabledSettings;
                return ActivePolicies;
            }
            else
            {
                return Properties.Resources.UnavailableText;
            }
        }

        /// <summary>
        /// Calcola il tempo di esecuzione del processo.
        /// </summary>
        /// <returns>Il tempo di esecuzione del processo come stringa.</returns>
        private static string CalculateProcessRunningTime(DateTime CreationTime)
        {
            TimeSpan RunningTime = DateTime.Now.Subtract(CreationTime);
            StringBuilder sb = new();
            if (RunningTime.Days > 0)
            {
                _ = RunningTime.Days == 1
                    ? sb.Append(RunningTime.Days + " " + Properties.Resources.DayText + ",")
                    : sb.Append(RunningTime.Days + " " + Properties.Resources.DaysText + ",");
            }
            _ = sb.Append(' ');
            if (RunningTime.Hours > 0)
            {
                _ = RunningTime.Hours == 1
                    ? sb.Append(RunningTime.Hours + " " + Properties.Resources.HourText + ",")
                    : sb.Append(RunningTime.Hours + " " + Properties.Resources.HoursText + ",");
            }
            _ = sb.Append(' ');
            if (RunningTime.Minutes > 0)
            {
                _ = RunningTime.Minutes == 1
                    ? sb.Append(RunningTime.Minutes + " " + Properties.Resources.MinuteText)
                    : sb.Append(RunningTime.Minutes + " " + Properties.Resources.MinutesText);
            }
            _ = sb.Append(" " + Properties.Resources.AgoText);
            return sb.ToString();
        }
    }
}