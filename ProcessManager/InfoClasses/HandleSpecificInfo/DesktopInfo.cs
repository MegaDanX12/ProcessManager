using System;
using System.Globalization;

namespace ProcessManager.InfoClasses.HandleSpecificInfo
{
    /// <summary>
    /// Informazioi su un desktop.
    /// </summary>
    public class DesktopInfo
    {
        /// <summary>
        /// Indica se il desktop riceve input dall'utente.
        /// </summary>
        public string IsDesktopReceivingInput { get; }

        /// <summary>
        /// Indica se è permesso a processi in esecuzione in un altro account utente di impostare hook in questo processo.
        /// </summary>
        public string OtherAccountHookAllowed { get; }

        /// <summary>
        /// Dimensione dell'heap del desktop, in KB.
        /// </summary>
        public string HeapSize { get; }

        /// <summary>
        /// SID dell'utente associato all'oggetto.
        /// </summary>
        public string AssociatedUserSID { get; }

        /// <summary>
        /// Utente associato all'oggetto.
        /// </summary>
        public string AssociatedUser { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="DesktopInfo"/>.
        /// </summary>
        /// <param name="IsReceivingInput">Indica se il desktop sta ricevendo input dall'utente.</param>
        /// <param name="OtherAccountHookAllowed">Indica se è permesso a processi in esecuzione in un altro account utente di impostare hook in questo processo.</param>
        /// <param name="HeapSize">Dimensione dell'heap del desktop, in KB.</param>
        /// <param name="UserSID">SID dell'utente associato all'oggetto.</param>
        /// <param name="UserName">Utente associato all'oggetto.</param>
        public DesktopInfo(bool? IsReceivingInput, bool? OtherAccountHookAllowed, uint? HeapSize, string UserSID, string UserName)
        {
            if (IsReceivingInput.HasValue)
            {
                IsDesktopReceivingInput = IsReceivingInput.Value ? Properties.Resources.YesText : "No";
            }
            else
            {
                IsDesktopReceivingInput = Properties.Resources.UnavailableText;
            }
            if (OtherAccountHookAllowed.HasValue)
            {
                this.OtherAccountHookAllowed = OtherAccountHookAllowed.Value ? Properties.Resources.YesText : "No";
            }
            else
            {
                this.OtherAccountHookAllowed = Properties.Resources.UnavailableText;
            }
            if (HeapSize.HasValue)
            {
                this.HeapSize = Convert.ToString(HeapSize.Value, CultureInfo.CurrentCulture) + " KB";
            }
            else
            {
                this.HeapSize = Properties.Resources.UnavailableText;
            }
            AssociatedUserSID = UserSID;
            AssociatedUser = UserName;
        }
    }
}