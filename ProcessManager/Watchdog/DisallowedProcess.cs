using System.ComponentModel;

namespace ProcessManager.Watchdog
{
    /// <summary>
    /// Processo la cui esecuzione non è permessa.
    /// </summary>
    public class DisallowedProcess : INotifyPropertyChanged
    {
        /// <summary>
        /// Nome del processo.
        /// </summary>
        public string Name { get; }

        private bool NotificationWhenTerminatedValue;

        /// <summary>
        /// Indica se mostrare una notifica quando il processo viene terminato.
        /// </summary>
        public bool NotificationWhenTerminated 
        {
            get => NotificationWhenTerminatedValue;
            private set
            {
                if (NotificationWhenTerminatedValue != value)
                {
                    NotificationWhenTerminatedValue = value;
                    NotifyPropertyChanged(nameof(NotificationWhenTerminated));
                }
            }
        }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="DisallowedProcess"/>.
        /// </summary>
        /// <param name="Name">Nome del processo.</param>
        /// <param name="NotificationWhenTerminated">Indica se mostrare una notifica quando il processo viene terminato.</param>
        public DisallowedProcess(string Name, bool NotificationWhenTerminated)
        {
            this.Name = Name;
            NotificationWhenTerminatedValue = NotificationWhenTerminated;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Abilita le notifiche in caso di terminazione del processo.
        /// </summary>
        public void EnableNotification()
        {
            NotificationWhenTerminated = true;
        }

        /// <summary>
        /// Disabilita le notifiche in caso di terminazione del processo.
        /// </summary>
        public void DisableNotification()
        {
            NotificationWhenTerminated = false;
        }

        private void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}