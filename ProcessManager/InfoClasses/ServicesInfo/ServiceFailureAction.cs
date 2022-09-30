using System.Globalization;

namespace ProcessManager.InfoClasses.ServicesInfo
{
    /// <summary>
    /// Rappresenta un'azione da eseguire in caso di crash di un servizio.
    /// </summary>
    public class ServiceFailureAction
    {
        /// <summary>
        /// Tipo di azione.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Tempo di attesa prima di effettuare l'azione.
        /// </summary>
        public string ActionDelay { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="ServiceFailureAction"/>.
        /// </summary>
        /// <param name="Type">Tipo di azione.</param>
        /// <param name="Delay">Tempo di attesa.</param>
        public ServiceFailureAction(string Type, uint Delay)
        {
            this.Type = Type;
            ActionDelay = Delay.ToString("D0", CultureInfo.CurrentCulture);
        }
    }
}