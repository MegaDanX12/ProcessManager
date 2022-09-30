using System.ComponentModel;

namespace ProcessManager.Watchdog
{
    /// <summary>
    /// Informazioni sul numero massimo istanze per un processo.
    /// </summary>
    public class ProcessInstanceLimit : INotifyPropertyChanged
    {
        /// <summary>
        /// Nome del processo.
        /// </summary>
        public string Name { get; }

        private uint InstanceLimitValue;

        /// <summary>
        /// Numero massimo di istanze.
        /// </summary>
        public uint InstanceLimit
        {
            get => InstanceLimitValue;
            private set
            {
                if (InstanceLimitValue != value)
                {
                    InstanceLimitValue = value;
                    NotifyPropertyChanged(nameof(InstanceLimit));
                }
            }
        }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="ProcessInstanceLimit"/>.
        /// </summary>
        /// <param name="Name">Nome del processo.</param>
        /// <param name="InstanceLimit">Numero massimo di istanze.</param>
        public ProcessInstanceLimit(string Name, uint InstanceLimit)
        {
            this.Name = Name;
            this.InstanceLimit = InstanceLimit;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Modifica il numero massimo di istanze.
        /// </summary>
        /// <param name="InstanceLimit">Nuovo limite di istanze.</param>
        public void EditSetting(uint InstanceLimit)
        {
            this.InstanceLimit = InstanceLimit;
        }

        private void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}