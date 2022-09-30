using System.ComponentModel;

namespace ProcessManager.Watchdog
{
    /// <summary>
    /// Utilizzo energetico di un processo.
    /// </summary>
    public class ProcessEnergyUsage : INotifyPropertyChanged
    {
        /// <summary>
        /// Nome del processo.
        /// </summary>
        public string Name { get; }

        private bool KeepDisplayOnValue;

        /// <summary>
        /// Indica se il monitor deve rimanere acceso.
        /// </summary>
        public bool KeepDisplayOn
        {
            get => KeepDisplayOnValue;
            private set
            {
                if (KeepDisplayOnValue != value)
                {
                    KeepDisplayOnValue = value;
                    NotifyPropertyChanged(nameof(KeepDisplayOn));
                }
            }
        }

        private bool KeepSystemInWorkingStateValue;

        /// <summary>
        /// Indica se il sistema non deve entrare in sospensione.
        /// </summary>
        public bool KeepSystemInWorkingState
        {
            get => KeepSystemInWorkingStateValue;
            private set
            {
                if (KeepSystemInWorkingStateValue != value)
                {
                    KeepSystemInWorkingStateValue = value;
                    NotifyPropertyChanged(nameof(KeepSystemInWorkingState));
                }
            }
        }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="ProcessEnergyUsage"/>.
        /// </summary>
        /// <param name="Name">Nome del processo.</param>
        /// <param name="KeepDisplayOn">Indica se il monitor deve rimanere acceso.</param>
        /// <param name="KeepSystemInWorkingState">Indica se il sistema non deve entrare in sospensione.</param>
        public ProcessEnergyUsage(string Name, bool KeepDisplayOn, bool KeepSystemInWorkingState)
        {
            this.Name = Name;
            KeepDisplayOnValue = KeepDisplayOn;
            KeepSystemInWorkingStateValue = KeepSystemInWorkingState;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Modifica la regola.
        /// </summary>
        /// <param name="KeepDisplayOn">Indica se il monitor deve rimanere accesso.</param>
        /// <param name="KeepSystemInWorkingState">Indica se il sistema non deve entrare in sospensione.</param>
        public void EditSetting(bool KeepDisplayOn, bool KeepSystemInWorkingState)
        {
            this.KeepDisplayOn = KeepDisplayOn;
            this.KeepSystemInWorkingState = KeepSystemInWorkingState;
        }

        private void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}