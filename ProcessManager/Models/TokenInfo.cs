using Microsoft.Win32.SafeHandles;
using ProcessManager.InfoClasses.OtherInfo;
using ProcessManager.InfoClasses.TokensInfo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessManager.Models
{
    /// <summary>
    /// Contiene informazioni su un token di accesso.
    /// </summary>
    public class TokenInfo : IDisposable, INotifyPropertyChanged
    {
        /// <summary>
        /// Handle al token.
        /// </summary>
        private readonly SafeAccessTokenHandle TokenHandle;
        private bool disposedValue;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Informazioni generali.
        /// </summary>
        public TokenGeneralInfo GeneralInfo { get; }

        /// <summary>
        /// Informazioni sui gruppi inclusi.
        /// </summary>
        public List<TokenGroupInfo> GroupsInfo { get; }


        private List<TokenPrivilegeInfo> PrivilegesInfoValue;

        /// <summary>
        /// Informazioni sui privilegi applicati.
        /// </summary>
        public List<TokenPrivilegeInfo> PrivilegesInfo 
        {
            get 
            {
                return PrivilegesInfoValue;
            } 
            private set 
            {
                if (PrivilegesInfoValue != value)
                {
                    PrivilegesInfoValue = value;
                    NotifyPropertyChanged(nameof(PrivilegesInfo));
                }
            }
        }

        /// <summary>
        /// Statistiche.
        /// </summary>
        public TokenStatistics Statistics { get; }

        /// <summary>
        /// Capacità.
        /// </summary>
        public List<TokenGroupInfo> Capabilities { get; }

        /// <summary>
        /// Richieste utente.
        /// </summary>
        public List<ClaimSecurityAttribute> UserClaims { get; }

        /// <summary>
        /// Richieste dispositivo.
        /// </summary>
        public List<ClaimSecurityAttribute> DeviceClaims { get; }


        /// <summary>
        /// Inizializza una nuova istanza di <see cref="TokenInfo"/>.
        /// </summary>
        /// <param name="GeneralInfo">Informazioni generali.</param>
        /// <param name="Groups">Informazioni sui gruppi inclusi.</param>
        /// <param name="Privileges">Informazioni sui privilegi.</param>
        /// <param name="Statistics">Statistiche del token.</param>
        /// <param name="Capabilities">Capacità del token.</param>
        /// <param name="UserClaims">Richieste utente.</param>
        /// <param name="DeviceClaims">Richieste dispositivo.</param>
        public TokenInfo(IntPtr TokenHandle, TokenGeneralInfo GeneralInfo, List<TokenGroupInfo> Groups, List<TokenPrivilegeInfo> Privileges, TokenStatistics Statistics, List<TokenGroupInfo> Capabilities, List<ClaimSecurityAttribute> UserClaims, List<ClaimSecurityAttribute> DeviceClaims)
        {
            this.TokenHandle = new SafeAccessTokenHandle(TokenHandle);
            this.GeneralInfo = GeneralInfo;
            GroupsInfo = Groups;
            PrivilegesInfoValue = Privileges;
            this.Statistics = Statistics;
            this.Capabilities = Capabilities;
            this.UserClaims = UserClaims;
            this.DeviceClaims = DeviceClaims;
        }

        /// <summary>
        /// Abilita un privilegio nel token.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo a cui appartiene il token.</param>
        /// <param name="PrivilegeName">Nome del privilegio.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool EnablePrivilege(SafeProcessHandle ProcessHandle, string PrivilegeName)
        {
            if (NativeHelpers.EnableTokenPrivilege(ProcessHandle, TokenHandle, PrivilegeName))
            {
                LogEntry Entry = NativeHelpers.BuildLogEntryForInformation("Privilegio abilitato nel token di un processo, nome privilegio: " + PrivilegeName, EventAction.TokenInfoManipulation, ProcessHandle);
                Logger.WriteEntry(Entry);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Disabilita un privilegio nel token.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo a cui appartiene il token.</param>
        /// <param name="PrivilegeName">Nome del privilegio.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool DisablePrivilege(SafeProcessHandle ProcessHandle, string PrivilegeName)
        {
            if (NativeHelpers.DisableTokenPrivilege(ProcessHandle, TokenHandle, PrivilegeName))
            {
                LogEntry Entry = NativeHelpers.BuildLogEntryForInformation("Privilegio disabilitato nel token di un processo, nome privilegio: " + PrivilegeName, EventAction.TokenInfoManipulation, ProcessHandle);
                Logger.WriteEntry(Entry);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Rimuove un privilegio dal token.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo a cui appartiene il token.</param>
        /// <param name="PrivilegeName">Nome del privilegio.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool RemovePrivilege(SafeProcessHandle ProcessHandle, string PrivilegeName)
        {
            if (NativeHelpers.RemovePrivilegeFromToken(ProcessHandle, TokenHandle, PrivilegeName))
            {
                LogEntry Entry = NativeHelpers.BuildLogEntryForInformation("Privilegio rimosso dal token di un processo, nome privilegio: " + PrivilegeName, EventAction.TokenInfoManipulation, ProcessHandle);
                Logger.WriteEntry(Entry);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Aggiorna i dati sui privilegi.
        /// </summary>
        public void UpdatePrivilegesData()
        {
            PrivilegesInfo = NativeHelpers.GetUpdatedTokenPrivileges(TokenHandle.DangerousGetHandle());
        }

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
                    TokenHandle.Close();
                    TokenHandle.Dispose();
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