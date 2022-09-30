namespace ProcessManager.InfoClasses.HandleSpecificInfo
{
    /// <summary>
    /// Informazioni su una window station.
    /// </summary>
    public class WindowStationInfo
    {
        /// <summary>
        /// Indica se la window station ha superfici visibili.
        /// </summary>
        public string IsVisible { get; }

        /// <summary>
        /// SID dell'utente associato all'oggetto.
        /// </summary>
        public string AssociatedUserSID { get; }

        /// <summary>
        /// Utente associato all'oggetto.
        /// </summary>
        public string AssociatedUser { get; }

        /// <summary>
        /// Inizializza una nuova istanza della classe <see cref="WindowStationInfo"/>.
        /// </summary>
        /// <param name="IsVisible">Indica se la window station ha superfici visibili.</param>
        /// <param name="UserSID">SID dell'utente associato all'oggetto.</param>
        /// <param name="UserName">Utente associato all'oggetto.</param>
        public WindowStationInfo(bool? IsVisible, string UserSID, string UserName)
        {
            if (IsVisible.HasValue)
            {
                this.IsVisible = IsVisible.Value ? Properties.Resources.YesText : "No";
            }
            else
            {
                this.IsVisible = Properties.Resources.UnavailableText;
            }
            AssociatedUser = UserSID;
            AssociatedUser = UserName;
        }
    }
}