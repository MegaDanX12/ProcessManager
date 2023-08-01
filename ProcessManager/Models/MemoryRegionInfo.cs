using Microsoft.Win32.SafeHandles;
using System;

namespace ProcessManager.Models
{
    /// <summary>
    /// Informazioni su una regione di memoria utilizzata da un processo.
    /// </summary>
    public class MemoryRegionInfo
    {
        /// <summary>
        /// Indirizzo di base della regione.
        /// </summary>
        public string BaseAddress { get; }

        /// <summary>
        /// Tipo di pagine contenute nella regione.
        /// </summary>
        public string PagesType { get; }

        /// <summary>
        /// Stato delle pagine contenute nella regione.
        /// </summary>
        public string PagesState { get; }

        /// <summary>
        /// Dimensione della regione.
        /// </summary>
        public string Size { get; }

        /// <summary>
        /// Tipo di protezione applicato alle pagine nella regione al momento dell'allocazione.
        /// </summary>
        public string InitialProtection { get; }

        /// <summary>
        /// Tipo di protezione attualmente applicata alle pagine nella regione.
        /// </summary>
        public string CurrentProtection { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="MemoryRegionInfo"/>.
        /// </summary>
        /// <param name="BaseAddress">Indirizzo di base della regione.</param>
        /// <param name="PagesType">Tipo di pagine nella regione.</param>
        /// <param name="PagesState">Stato delle pagine nella regione.</param>
        /// <param name="Size">Dimensione della regione.</param>
        /// <param name="InitialProtection">Tipo di protezione inizialmente applicata alle pagine nella regione.</param>
        /// <param name="CurrentProtection">Tipo di protezione attualmente applicata alle pagine nella regione.</param>
        public MemoryRegionInfo(IntPtr BaseAddress, string PagesType, string PagesState, string Size, string InitialProtection, string CurrentProtection)
        {
            this.BaseAddress = "0x" + BaseAddress.ToString("X");
            this.PagesType = PagesType;
            this.PagesState = PagesState;
            this.Size = Size;
            this.InitialProtection = InitialProtection;
            this.CurrentProtection = CurrentProtection;
        }

        /// <summary>
        /// Cambia la protezione della regione.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo a cui è associata la regione di memoria.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool ChangeRegionProtection(SafeProcessHandle ProcessHandle, string NewProtection)
        {
            if (NativeHelpers.ChangeMemoryRegionProtection(ProcessHandle, this, NewProtection, out string OldProtection))
            {
                LogEntry Entry = NativeHelpers.BuildLogEntryForInformation("Cambiata protezione di una regione di memoria, nuova protezione: " + NewProtection + ", valore precedente: " + OldProtection + ", indirizzo di base: " + BaseAddress, EventAction.MemoryInfoManipulation, ProcessHandle);
                Logger.WriteEntry(Entry);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Libera la regione.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo a cui è associata la regione di memoria.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool FreeRegion(SafeProcessHandle ProcessHandle)
        {
            if (NativeHelpers.FreeMemoryRegion(ProcessHandle, this))
            {
                LogEntry Entry = NativeHelpers.BuildLogEntryForInformation("Regione di memoria liberata, indirizzo di base: " + BaseAddress, EventAction.MemoryInfoManipulation, ProcessHandle);
                Logger.WriteEntry(Entry);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Annulla la mappatura della regione.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo a cui è associata la regione di memoria.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool DecommitRegion(SafeProcessHandle ProcessHandle)
        {
            if (NativeHelpers.DecommitMemoryRegion(ProcessHandle, this))
            {
                LogEntry Entry = NativeHelpers.BuildLogEntryForInformation("Annullata la mappatura di una regione di memoria, indirizzo di base: " + BaseAddress, EventAction.MemoryInfoManipulation, ProcessHandle);
                Logger.WriteEntry(Entry);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}