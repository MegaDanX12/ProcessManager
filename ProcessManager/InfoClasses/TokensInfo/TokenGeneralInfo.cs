using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessManager.InfoClasses.TokensInfo
{
    /// <summary>
    /// Contiene informazioni generali su un token.
    /// </summary>
    public class TokenGeneralInfo
    {
        /// <summary>
        /// Nome dell'utente associato al token.
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// SID dell'utente associato al token.
        /// </summary>
        public string UserSID { get; }

        /// <summary>
        /// ID della sessione Terminal Services associata al token.
        /// </summary>
        public string SessionID { get; }

        /// <summary>
        /// Indica se il token ha privilegi amministrativi.
        /// </summary>
        public bool? IsElevated { get; }

        /// <summary>
        /// Indica se la virtualizzazione è permessa per il token.
        /// </summary>
        public bool? IsVirtualizationAllowed { get; }

        /// <summary>
        /// Indica se la virtualizzazione è attiva per il token.
        /// </summary>
        public bool? IsVirtualizationEnabled { get; }

        /// <summary>
        /// SID dell'app container associato al token.
        /// </summary>
        public string AppContainerSID { get; }

        /// <summary>
        /// Livello di integrità.
        /// </summary>
        public string IntegrityLevel { get; }

        /// <summary>
        /// Nome dell'account proprietario.
        /// </summary>
        public string Owner { get; }

        /// <summary>
        /// Nome del gruppo primario.
        /// </summary>
        public string PrimaryGroup { get; }

        /// <summary>
        /// Nome della fonte del token.
        /// </summary>
        public string SourceName { get; }

        /// <summary>
        /// Valore LUID che identifica la fonte del token.
        /// </summary>
        public string SourceLUID { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="TokenGeneralInfo"/>.
        /// </summary>
        /// <param name="Username">Nome dell'utente associato al token.</param>
        /// <param name="UserSID">SID dell'utente associato al token.</param>
        /// <param name="SessionID">ID della sessione Terminal Services associata al token.</param>
        /// <param name="IsElevated">Indica se il token ha privilegi amministrativi.</param>
        /// <param name="IsVirtualizationAllowed">Indica se la virtualizzazione è permessa per il token.</param>
        /// <param name="IsVirtualizationEnabled">Indica se la virtualizzazione è abilitata per il token.</param>
        /// <param name="AppContainerSID">SID dell'app container associato al token.</param>
        /// <param name="IntegrityLevel">Livello di integrità del token.</param>
        /// <param name="Owner">Proprietario predefinito per nuovi oggetti.</param>
        /// <param name="PrimaryGroup">Gruppo primario per i nuovi oggetti.</param>
        /// <param name="Source">Nome della fonte del token.</param>
        /// <param name="SourceLUID">LUID che identifica la fonte del token.</param>
        public TokenGeneralInfo(string Username, string UserSID, string SessionID, bool? IsElevated, bool? IsVirtualizationAllowed, bool? IsVirtualizationEnabled, string AppContainerSID, string IntegrityLevel, string Owner, string PrimaryGroup, string Source, string SourceLUID)
        {
            this.Username = Username;
            this.UserSID = UserSID;
            this.SessionID = SessionID;
            this.IsElevated = IsElevated;
            this.IsVirtualizationAllowed = IsVirtualizationAllowed;
            this.IsVirtualizationEnabled = IsVirtualizationEnabled;
            this.AppContainerSID = AppContainerSID;
            this.IntegrityLevel = IntegrityLevel;
            this.Owner = Owner;
            this.PrimaryGroup = PrimaryGroup;
            SourceName = Source;
            this.SourceLUID = SourceLUID;
        }
    }
}