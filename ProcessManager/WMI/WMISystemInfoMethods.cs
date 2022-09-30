using System.Management;
using static ProcessManager.NativeHelpers;

namespace ProcessManager.WMI
{
    /// <summary>
    /// Informazioni sull'utilizzo del file di paging.
    /// </summary>
    public struct PagefileUsageInfo
    {
        /// <summary>
        /// Dimensiona massima del file di paging, in MB.
        /// </summary>
        public uint TotalSize { get; set; }

        /// <summary>
        /// Attuale utilizzo del file di paging, in MB.
        /// </summary>
        public uint TotalInUse { get; set; }

        /// <summary>
        /// Utilizzo massimo del file di paging, in MB.
        /// </summary>
        public uint PeakUsage { get; set; }
    }

    /// <summary>
    /// Contiene metodi per recuperare informazioni sul sistema da WMI.
    /// </summary>
    public static class WMISystemInfoMethods
    {
        /// <summary>
        /// Recupera informazioni sull'utilizzo del file di paging.
        /// </summary>
        /// <param name="FilePath">Percorso del file di paging.</param>
        /// <returns>Una struttura <see cref="PagefileUsageInfo"/> con le informazioni.</returns>
        public static PagefileUsageInfo GetPagefileUsageInfo(string FilePath)
        {
            ManagementObjectSearcher Searcher = new("SELECT * FROM Win32_PageFileUsage WHERE Name =\"" + FilePath + "\"");
            ManagementObjectCollection Collection = Searcher.Get();
            PagefileUsageInfo Info = new();
            foreach (ManagementObject obj in Collection)
            {
                Info.TotalSize = (uint)obj.Properties["AllocatedBaseSize"].Value;
                Info.TotalInUse = (uint)obj.Properties["CurrentUsage"].Value;
                Info.PeakUsage = (uint)obj.Properties["PeakUsage"].Value;
            }
            return Info;
        }

        /// <summary>
        /// Crea un file di paging.
        /// </summary>
        /// <param name="Path">Percorso del nuovo file di paging.</param>
        /// <param name="InitialSize">Dimensione iniziale, in MB.</param>
        /// <param name="MaximumSize">Dimensione massima, in MB.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool CreatePagefile(string Path, uint InitialSize, uint MaximumSize)
        {
            try
            {
                ManagementClass PagefileClass = new("Win32_PageFileSetting");
                ManagementObject NewInstance = PagefileClass.CreateInstance();
                NewInstance["Name"] = Path;
                NewInstance["InitialSize"] = InitialSize;
                NewInstance["MaximumSize"] = MaximumSize;
                _ = NewInstance.Put();
                return true;
            }
            catch
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile creare un nuovo file di paging", EventAction.PageFileCreation, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Elimina un file di paging.
        /// </summary>
        /// <param name="Path">Percorso del file di paging.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool DeletePageFile(string Path)
        {
            try
            {
                string Filepath = Path.Insert(Path.IndexOf('\\'), "\\");
                ManagementObjectSearcher Searcher = new("SELECT * FROM Win32_PageFileSetting WHERE Name =\"" + Filepath + "\"");
                ManagementObjectCollection Collection = Searcher.Get();
                foreach (ManagementObject file in Collection)
                {
                    file.Delete();
                }
                return true;
            }
            catch
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile eliminare un file di paging", EventAction.PageFileDeletion, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Cambia la dimensione di un file di paging.
        /// </summary>
        /// <param name="Path">Percorso del file di paging.</param>
        /// <param name="InitialSize">Nuova dimensione iniziale, in MB.</param>
        /// <param name="MaximumSize">Nuova dimensione massima, in MB.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool ChangePagefileSize(string Path, uint InitialSize, uint MaximumSize)
        {
            try
            {
                string Filepath = Path.Insert(Path.IndexOf('\\'), "\\");
                ManagementObjectSearcher Searcher = new("SELECT * FROM Win32_PageFileSetting WHERE Name =\"" + Filepath + "\"");
                ManagementObjectCollection Collection = Searcher.Get();
                foreach (ManagementObject file in Collection)
                {
                    file["InitialSize"] = InitialSize;
                    file["MaximumSize"] = MaximumSize;
                    _ = file.Put();
                }
                return true;
            }
            catch
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile modificare le dimensioni di un file di paging", EventAction.PageFileManipulation, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }
    }
}