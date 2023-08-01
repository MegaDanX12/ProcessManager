using Microsoft.Win32.SafeHandles;
using ProcessManager.Models;
using System.ComponentModel;

namespace ProcessManager.InfoClasses.HandleSpecificInfo
{
    /// <summary>
    /// Informazioni su un evento.
    /// </summary>
    public class EventInfo : INotifyPropertyChanged
    {
        /// <summary>
        /// Handle al processo a cui appartiene il mutante.
        /// </summary>
        private readonly SafeProcessHandle ProcessHandle;

        /// <summary>
        /// Istanza di <see cref="HandleInfo"/> relativa al mutante.
        /// </summary>
        private readonly HandleInfo AssociatedInfoInstance;

        /// <summary>
        /// Tipo di evento.
        /// </summary>
        public string EventType { get; }

        private string EventStateValue;

        /// <summary>
        /// Stato dell'evento (segnalato o meno).
        /// </summary>
        public string EventState
        {
            get
            {
                return EventStateValue;
            }
            set
            {
                if (EventStateValue != value)
                {
                    EventStateValue = value;
                    NotifyPropertyChanged(nameof(EventState));
                }
            }
        }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="EventInfo"/>.
        /// </summary>
        /// <param name="EventType">Tipo di evento.</param>
        /// <param name="EventState">Stato dell'evento.</param>
        /// <param name="ProcessHandle">Handle al processo proprietario dell'evento.</param>
        /// <param name="Info">Istanza di <see cref="HandleInfo"/> associata all'evento.</param>
        public EventInfo(string EventType, bool? EventState, SafeProcessHandle ProcessHandle, HandleInfo Info)
        {
            this.ProcessHandle = ProcessHandle;
            AssociatedInfoInstance = Info;
            this.EventType = EventType;
            if (EventState.HasValue)
            {
                EventStateValue = EventState.Value ? Properties.Resources.YesText : "No";
            }
            else
            {
                EventStateValue = Properties.Resources.UnavailableText;
            }
        }

        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        public void UpdateData()
        {
            EventInfo NewInfo = NativeHelpers.GetEventSpecificInfo(ProcessHandle, AssociatedInfoInstance, true);
            EventState = NewInfo.EventState;
        }

        private void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}