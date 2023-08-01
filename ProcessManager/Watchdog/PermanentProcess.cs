using System.ComponentModel;

namespace ProcessManager.Watchdog
{
    /// <summary>
    /// Processo che deve rimanere in esecuzione.
    /// </summary>
    public class PermanentProcess : INotifyPropertyChanged
    {
        /// <summary>
        /// Nome del processo.
        /// </summary>
        public string Name { get; }

        private bool NotificationWhenStartedValue;

        /// <summary>
        /// Indica se mostrare una notifica quando il processo viene terminato.
        /// </summary>
        public bool NotificationWhenStarted
        {
            get => NotificationWhenStartedValue;
            private set
            {
                if (NotificationWhenStartedValue != value)
                {
                    NotificationWhenStartedValue = value;
                    NotifyPropertyChanged(nameof(NotificationWhenStarted));
                }
            }
        }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="PermanentProcess"/>.
        /// </summary>
        /// <param name="Name">Nome del processo.</param>
        /// <param name="NotificationWhenStarted">Indica se mostrare una notifica quando il processo viene avviato.</param>
        public PermanentProcess(string Name, bool NotificationWhenStarted)
        {
            this.Name = Name;
            NotificationWhenStartedValue = NotificationWhenStarted;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Abilita le notifiche in caso di avvio del processo.
        /// </summary>
        public void EnableNotification()
        {
            NotificationWhenStarted = true;
        }

        /// <summary>
        /// Disabilita le notifiche in caso di avvio del processo.
        /// </summary>
        public void DisableNotification()
        {
            NotificationWhenStarted = false;
        }

        private void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}