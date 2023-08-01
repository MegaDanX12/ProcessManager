using System.Collections.Generic;

namespace ProcessManager.Watchdog
{
    /// <summary>
    /// Azione da eseguire quando l'utilizzo della CPU o della memoria da parte di un processo ha superato il massimo.
    /// </summary>
    public enum WatchdogAction
    {
        /// <summary>
        /// Cambia l'affinità del processo.
        /// </summary>
        ChangeAffinity,
        /// <summary>
        /// Cambia la priorità del processo.
        /// </summary>
        ChangePriority,
        /// <summary>
        /// Pulizia del working set.
        /// </summary>
        EmptyWorkingSet,
        /// <summary>
        /// Termina il processo.
        /// </summary>
        TerminateProcess
    }

    /// <summary>
    /// Rappresenta le azioni da eseguire quando l'utilizzo della CPU o della memoria da parte di un processo ha superato il massimo.
    /// </summary>
    public class ProcessWatchdogAction
    {
        /// <summary>
        /// Azione da eseguire.
        /// </summary>
        public WatchdogAction ActionType { get; private set; }

        /// <summary>
        /// Valore da utilizzare per eseguire l'azione.
        /// </summary>
        public object ActionValue { get; private set; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="ProcessWatchdogAction"/>.
        /// </summary>
        /// <param name="Action">Azione da eseguire.</param>
        /// <param name="Value">Valore da utilizzare per eseguire l'azione.</param>
        public ProcessWatchdogAction(WatchdogAction Action, object Value)
        {
            ActionType = Action;
            ActionValue = Value;
        }

        /// <summary>
        /// Cambia l'azione da eseguire.
        /// </summary>
        /// <param name="Action">Nuova azione da eseguire.</param>
        /// <param name="Value">Valore da utilizzare per eseguire l'azione.</param>
        public void ChangeAction(WatchdogAction Action, object Value)
        {
            ActionType = Action;
            ActionValue = Value;
        }

        public override bool Equals(object obj)
        {
            return obj is ProcessWatchdogAction action &&
                   ActionType == action.ActionType &&
                   EqualityComparer<object>.Default.Equals(ActionValue, action.ActionValue);
        }

        public override int GetHashCode()
        {
            int hashCode = 916225939;
            hashCode = (hashCode * -1521134295) + ActionType.GetHashCode();
            hashCode = (hashCode * -1521134295) + EqualityComparer<object>.Default.GetHashCode(ActionValue);
            return hashCode;
        }
    }
}