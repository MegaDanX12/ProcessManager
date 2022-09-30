namespace ProcessManager.Watchdog
{
    /// <summary>
    /// Rappresenta una regola del watchdog.
    /// </summary>
    public class ProcessWatchdogRule
    {
        /// <summary>
        /// Nome del processo a cui applicare la regola.
        /// </summary>
        public string ProcessName { get; }

        /// <summary>
        /// Impostazioni della regola.
        /// </summary>
        public WatchdogSettings Settings { get; }

        /// <summary>
        /// Azione da eseguire in caso l'utilizzo della CPU superi il massimo.
        /// </summary>
        public ProcessWatchdogAction CPUAction { get; }

        /// <summary>
        /// Azione da eseguire in caso l'utilizzo della memoria superi il massimo.
        /// </summary>
        public ProcessWatchdogAction MemoryAction { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="ProcessWatchdogRule"/>.
        /// </summary>
        /// <param name="ProcessName">Nome del processo a cui applicare la regola.</param>
        /// <param name="Settings">Impostazioni della regola.</param>
        /// <param name="CPUAction">Azione del watchdog CPU.</param>
        /// <param name="MemoryAction">Azione del watchdog memoria.</param>
        public ProcessWatchdogRule(string ProcessName, WatchdogSettings Settings, ProcessWatchdogAction CPUAction, ProcessWatchdogAction MemoryAction)
        {
            this.ProcessName = ProcessName;
            this.Settings = Settings;
            this.CPUAction = CPUAction;
            this.MemoryAction = MemoryAction;
        }
    }
}