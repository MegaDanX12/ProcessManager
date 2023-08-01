using ProcessManager.Models;
using System.ComponentModel;

namespace ProcessManager.Watchdog
{
    /// <summary>
    /// Impostazioni predefinite della CPU per un processo.
    /// </summary>
    public class ProcessDefaultCPUSettings : INotifyPropertyChanged
    {
        /// <summary>
        /// Nome del processo.
        /// </summary>
        public string Name { get; }

        private ProcessInfo.ProcessPriority? DefaultPriorityValue;

        /// <summary>
        /// Priorità predefinita.
        /// </summary>
        public ProcessInfo.ProcessPriority? DefaultPriority
        {
            get => DefaultPriorityValue;
            private set
            {
                if (DefaultPriorityValue != value)
                {
                    DefaultPriorityValue = value;
                    NotifyPropertyChanged(nameof(DefaultPriority));
                }
            }
        }

        private ulong? DefaultAffinityValue;

        /// <summary>
        /// Affinità predefinita.
        /// </summary>
        public ulong? DefaultAffinity
        {
            get => DefaultAffinityValue;
            private set
            {
                if (DefaultAffinityValue != value)
                {
                    DefaultAffinityValue = value;
                    NotifyPropertyChanged(nameof(DefaultAffinity));
                }
            }
        }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="ProcessDefaultCPUSettings"/>.
        /// </summary>
        /// <param name="Name">Nome del processo.</param>
        /// <param name="Priority">Priorità predefinità del processo.</param>
        /// <param name="Affinity">Affinità predefinita del processo.</param>
        public ProcessDefaultCPUSettings(string Name, ProcessInfo.ProcessPriority? Priority, ulong? Affinity)
        {
            this.Name = Name;
            DefaultPriorityValue = Priority;
            DefaultAffinityValue = Affinity;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Modifica la regola.
        /// </summary>
        /// <param name="Priority">Nuova priorità</param>
        /// <param name="Affinity">Nuova affinità.</param>
        public void EditSetting(ProcessInfo.ProcessPriority? Priority = null, ulong? Affinity = null)
        {
            if (Priority.HasValue)
            {
                DefaultPriority = Priority.Value;
            }
            if (Affinity.HasValue)
            {
                DefaultAffinity = Affinity.Value;
            }
        }

        private void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}