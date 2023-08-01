using ProcessManager.Models;
using System.Threading;

namespace ProcessManager.Watchdog
{
    /// <summary>
    /// Impostazioni del watchdog per un processo.
    /// </summary>
    public struct WatchdogSettings
    {
        /// <summary>
        /// Indica se il watchdog della CPU è attivo.
        /// </summary>
        public bool CpuWatchdogEnabled { get; private set; }

        /// <summary>
        /// Valore massimo utilizzo CPU, in percentuale.
        /// </summary>
        public uint CpuWatchdogValue { get; private set; }

        /// <summary>
        /// Tempo di controllo CPU.
        /// </summary>
        public uint CpuWatchdogTime { get; private set; }

        /// <summary>
        /// Indica se il watchdog della memoria è attivo.
        /// </summary>
        public bool MemoryWatchdogEnabled { get; private set; }

        /// <summary>
        /// Valore massimo utilizzo memoria, in bytes.
        /// </summary>
        public ulong MemoryWatchdogValue { get; private set; }

        /// <summary>
        /// Tempo di controllo memoria.
        /// </summary>
        public uint MemoryWatchdogTime { get; private set; }

        /// <summary>
        /// Inizializza i campi della struttura <see cref="WatchdogSettings"/>.
        /// </summary>
        /// <param name="CpuWatchdog">Indica se il watchdog della CPU è attivo.</param>
        /// <param name="CpuValue">Valore massimo dell'utilizzo CPU in percentuale.</param>
        /// <param name="CpuTime">Tempo di controllo in secondi per il watchdog CPU.</param>
        /// <param name="MemoryWatchdog">Indica se il watchdog della memoria è attivo.</param>
        /// <param name="MemoryValue">Valore massimo in bytes dell'utilizzo di memoria.</param>
        /// <param name="MemoryTime">Tempo di controllo in secondi per il watchdog della memoria.</param>
        public WatchdogSettings(bool CpuWatchdog, uint CpuValue, uint CpuTime, bool MemoryWatchdog, ulong MemoryValue, uint MemoryTime)
        {
            CpuWatchdogEnabled = CpuWatchdog;
            CpuWatchdogValue = CpuValue;
            CpuWatchdogTime = CpuTime;
            MemoryWatchdogEnabled = MemoryWatchdog;
            MemoryWatchdogValue = MemoryValue;
            MemoryWatchdogTime = MemoryTime;
        }

        /// <summary>
        /// Abilita il watchdog CPU.
        /// </summary>
        /// <param name="Value">Valore massimo utilizzo CPU, in percentuale.</param>
        /// <param name="Time">Tempo di controllo.</param>
        public void EnableCPUWatchdog(uint Value, uint Time)
        {
            CpuWatchdogEnabled = true;
            CpuWatchdogValue = Value;
            CpuWatchdogTime = Time;
        }

        /// <summary>
        /// Disabilita il watchdog CPU.
        /// </summary>
        public void DisableCPUWatchdog()
        {
            CpuWatchdogEnabled = false;
            CpuWatchdogValue = 0;
            CpuWatchdogTime = 0;
        }

        /// <summary>
        /// Abilita il watchdog della memoria.
        /// </summary>
        /// <param name="Value">Valore massimo utilizzo memoria, in bytes.</param>
        /// <param name="Time">Tempo di controllo.</param>
        public void EnableMemoryWatchdog(ulong Value, uint Time)
        {
            MemoryWatchdogEnabled = true;
            MemoryWatchdogValue = Value;
            MemoryWatchdogTime = Time;
        }

        /// <summary>
        /// Disabilita il watchdog della memoria.
        /// </summary>
        public void DisableMemoryWatchdog()
        {
            MemoryWatchdogEnabled = false;
            MemoryWatchdogValue = 0;
            MemoryWatchdogTime = 0;
        }

        public override bool Equals(object obj)
        {
            return obj is WatchdogSettings settings &&
                   CpuWatchdogEnabled == settings.CpuWatchdogEnabled &&
                   CpuWatchdogValue == settings.CpuWatchdogValue &&
                   CpuWatchdogTime == settings.CpuWatchdogTime &&
                   MemoryWatchdogEnabled == settings.MemoryWatchdogEnabled &&
                   MemoryWatchdogValue == settings.MemoryWatchdogValue &&
                   MemoryWatchdogTime == settings.MemoryWatchdogTime;
        }

        public override int GetHashCode()
        {
            int hashCode = 1594029809;
            hashCode = (hashCode * -1521134295) + CpuWatchdogEnabled.GetHashCode();
            hashCode = (hashCode * -1521134295) + CpuWatchdogValue.GetHashCode();
            hashCode = (hashCode * -1521134295) + CpuWatchdogTime.GetHashCode();
            hashCode = (hashCode * -1521134295) + MemoryWatchdogEnabled.GetHashCode();
            hashCode = (hashCode * -1521134295) + MemoryWatchdogValue.GetHashCode();
            hashCode = (hashCode * -1521134295) + MemoryWatchdogTime.GetHashCode();
            return hashCode;
        }
    }

    /// <summary>
    /// Contiene la configurazione del watchdog per un processo.
    /// </summary>
    public class ProcessWatchdogConfiguration
    {
        /// <summary>
        /// Istanza di <see cref="ProcessInfo"/> associata al processo monitorato.
        /// </summary>
        public ProcessInfo Process { get; }

        /// <summary>
        /// Impostazioni del watchdog per il processo.
        /// </summary>
        public WatchdogSettings Settings { get; }

        /// <summary>
        /// Azione da eseguire nel caso l'utilizzo della CPU supera il limite.
        /// </summary>
        public ProcessWatchdogAction CPUAction { get; }

        /// <summary>
        /// Azione da eseguire nel caso l'utilizzo della memoria supera il limite.
        /// </summary>
        public ProcessWatchdogAction MemoryAction { get; }

        /// <summary>
        /// Token per l'annullamento dell'attività.
        /// </summary>
        private readonly CancellationToken CancellationToken;

        /// <summary>
        /// Indica se il processo sta per essere terminato.
        /// </summary>
        public bool ProcessTerminating { get; set; }

        /// <summary>
        /// Evento utilizzato per bloccare l'attuazione della regola durante la modifica.
        /// </summary>
        public ManualResetEvent EditingCompleted { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="ProcessWatchdogConfiguration"/>.
        /// </summary>
        /// <param name="Process">Istanza di <see cref="ProcessInfo"/> associata al processo monitorato.</param>
        /// <param name="Settings">Impostazioni del monitoraggio del processo.</param>
        /// <param name="CPUAction">Azione da eseguire se l'uso della CPU supera il limite.</param>
        /// <param name="MemoryAction">Azione da eseguire se l'uso della memoria supera il limite.</param>
        public ProcessWatchdogConfiguration(ProcessInfo Process, WatchdogSettings Settings, ProcessWatchdogAction CPUAction, ProcessWatchdogAction MemoryAction, CancellationToken CancellationToken)
        {
            this.Process = Process;
            this.Settings  = Settings;
            this.CPUAction = CPUAction;
            this.MemoryAction = MemoryAction;
            this.CancellationToken = CancellationToken;
            EditingCompleted = new(true);
        }

        /// <summary>
        /// Cambia l'azione da eseguire quando l'utilizzo della CPU supera il limite.
        /// </summary>
        /// <param name="Action">Azione da eseguire,</param>
        /// <param name="ActionValue">Valore da utilizzare per l'azione.</param>
        public void ChangeCPUAction(WatchdogAction Action, object ActionValue)
        {
            if (Action is WatchdogAction.ChangeAffinity or WatchdogAction.ChangePriority or WatchdogAction.TerminateProcess)
            {
                CPUAction.ChangeAction(Action, ActionValue);
            }
        }

        /// <summary>
        /// Cambia l'azione da eseguire quando l'utilizzo della memoria supera il limite.
        /// </summary>
        /// <param name="Action">Azione da eseguire,</param>
        /// <param name="ActionValue">Valore da utilizzare per l'azione.</param>
        public void ChangeMemoryAction(WatchdogAction Action, object ActionValue)
        {
            if (Action is WatchdogAction.EmptyWorkingSet or WatchdogAction.TerminateProcess)
            {
                MemoryAction.ChangeAction(Action, ActionValue);
            }
        }

        /// <summary>
        /// Controlla se è stato richiesto l'arresto del watchdog.
        /// </summary>
        /// <returns>true se è stato richiesto l'arresto del watchdog, altrimenti false.</returns>
        public bool IsWatchdogShuttingDown()
        {
            return CancellationToken.IsCancellationRequested;
        }

        /// <summary>
        /// Controlla che l'istanza di questa configurazione rispetta le regole stabilite nell'istanza di <see cref="ProcessWatchdogRule"/> fornita.
        /// </summary>
        /// <param name="Rule">Regola in base alla quale confrontare questa istanza.</param>
        /// <returns>true se l'istanza si basa sulla regola, false altrimenti.</returns>
        public bool IsAssociatedWithRule(ProcessWatchdogRule Rule)
        {
            return Process.Name == Rule.ProcessName && Settings.Equals(Rule.Settings) && CPUAction.Equals(Rule.CPUAction) && MemoryAction.Equals(Rule.MemoryAction);
        }
    }
}