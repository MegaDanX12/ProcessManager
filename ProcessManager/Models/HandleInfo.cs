using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessManager.Models
{
    /// <summary>
    /// Contiene informazioni su un handle.
    /// </summary>
    public class HandleInfo
    {
        /// <summary>
        /// Tipo di handle.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Nome dell'handle.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Valore numerico dell'handle.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Indirizzo di memoria dove si trova l'oggetto a cui questo handle si riferisce.
        /// </summary>
        public string ObjectAddress { get; }

        /// <summary>
        /// Tipo di accesso consentito all'oggetto.
        /// </summary>
        public string GrantedAccess { get; }

        /// <summary>
        /// Numero di riferimenti.
        /// </summary>
        public string ReferencesCount { get; }

        /// <summary>
        /// Numero di handle.
        /// </summary>
        public string HandlesCount { get; }

        /// <summary>
        /// 
        /// </summary>
        public string PagedPoolUsage { get; }

        /// <summary>
        /// 
        /// </summary>
        public string NonPagedPoolUsage { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="HandleInfo"/>.
        /// </summary>
        /// <param name="Type">Tipo di handle.</param>
        /// <param name="Value">Valore numerico dell'handle.</param>
        /// <param name="Name">Nome dell'handle.</param>
        /// <param name="ObjectAddress">Indirizzo di memoria dell'oggetto a cui l'handle si riferisce.</param>
        /// <param name="GrantedAccess">Tipo di accesso consentito all'handle.</param>
        /// <param name="ReferencesCount">Numero di riferimenti.</param>
        /// <param name="HandlesCount">Numero di handle.</param>
        /// <param name="PagedPoolUsage"></param>
        /// <param name="NonPagedPoolUsage"></param>
        public HandleInfo(string Type, string Name, string Value, string ObjectAddress, string GrantedAccess, string ReferencesCount, string HandlesCount, string PagedPoolUsage, string NonPagedPoolUsage)
        {
            this.Type = Type;
            this.Name = Name;
            this.Value = Value;
            this.ObjectAddress = ObjectAddress;
            this.GrantedAccess = GrantedAccess;
            this.ReferencesCount = ReferencesCount;
            this.HandlesCount = HandlesCount;
            this.PagedPoolUsage = PagedPoolUsage;
            this.NonPagedPoolUsage = NonPagedPoolUsage;
        }

        /// <summary>
        /// Chiude l'handle.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool CloseHandle(SafeProcessHandle Handle)
        {
            if (IntPtr.Size == 4)
            {
                IntPtr HandleToClose = new(Convert.ToInt32(Value, 16));
                if (NativeHelpers.CloseRemoteHandle(Handle, HandleToClose))
                {
                    LogEntry Entry = NativeHelpers.BuildLogEntryForInformation("Handle remoto chiuso, tipo: " + Type, EventAction.HandleManipulation, Handle);
                    Logger.WriteEntry(Entry);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                IntPtr HandleToClose = new(Convert.ToInt64(Value, 16));
                if (NativeHelpers.CloseRemoteHandle(Handle, HandleToClose))
                {
                    LogEntry Entry = NativeHelpers.BuildLogEntryForInformation("Handle remoto chiuso, tipo: " + Type, EventAction.HandleManipulation, Handle);
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
}