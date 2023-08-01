using System.Collections.Generic;
using System.Globalization;

namespace ProcessManager.InfoClasses.ServicesInfo
{
    /// <summary>
    /// Informazioni sulle azioni da eseguire in caso di crash di un servizio.
    /// </summary>
    public class ServiceFailureActions
    {
        /// <summary>
        /// Tempo dopo il quale il conteggio dei crash viene resettato se non ne avviene nessuno nel frattempo.
        /// </summary>
        public string ResetPeriod { get; }

        /// <summary>
        /// Messaggio da inviare in caso di riavvio del computer come risultato di un crash di un servizio.
        /// </summary>
        public string RebootMessage { get; }

        /// <summary>
        /// Linea di comando del processo da avviare dopo un crash di un servizio.
        /// </summary>
        public string CommandLine { get; }

        /// <summary>
        /// Azioni da eseguire.
        /// </summary>
        public List<ServiceFailureAction> Actions { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="ServiceFailureActions"/>.
        /// </summary>
        /// <param name="ResetPeriod">Tempo dopo il quale resettare il conteggio dei crash.</param>
        /// <param name="RebootMessage">Messaggio da inviare in caso di riavvio.</param>
        /// <param name="CommandLine">Linea di comando del processo da avviare.</param>
        /// <param name="Actions">Azioni da eseguire.</param>
        public ServiceFailureActions(uint ResetPeriod, string RebootMessage, string CommandLine, List<ServiceFailureAction> Actions)
        {
            this.ResetPeriod = ResetPeriod != uint.MaxValue ? ResetPeriod.ToString("D0", CultureInfo.CurrentCulture) : Properties.Resources.InfiniteText;
            this.RebootMessage = !string.IsNullOrWhiteSpace(RebootMessage) ? RebootMessage : Properties.Resources.NoneText;
            this.CommandLine = !string.IsNullOrWhiteSpace(CommandLine) ? CommandLine : Properties.Resources.NoneText2;
            this.Actions = Actions;
        }
    }
}