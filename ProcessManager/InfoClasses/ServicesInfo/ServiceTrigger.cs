using System;
using System.Collections.Generic;

namespace ProcessManager.InfoClasses.ServicesInfo
{
    public class ServiceTrigger
    {
        /// <summary>
        /// Tipo di trigger.
        /// </summary>
        public string TriggerType { get; }

        /// <summary>
        /// Azione da eseguire.
        /// </summary>
        public string Action { get; }

        /// <summary>
        /// Sottotipo del trigger.
        /// </summary>
        public string TriggerSubType { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="ServiceTrigger"/>.
        /// </summary>
        /// <param name="TriggerType">Tipo di trigger.</param>
        /// <param name="Action">Azione da eseguire.</param>
        /// <param name="TriggerSubType">Sottotipo del trigger.</param>
        /// <param name="SpecificData">Dati specifici del trigger.</param>
        public ServiceTrigger(string TriggerType, string Action, string TriggerSubType)
        {
            this.TriggerType = TriggerType;
            this.Action = Action;
            this.TriggerSubType = TriggerSubType;
        }
    }
}