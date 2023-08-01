using ProcessManager.Commands.LogManagerWindowCommands;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace ProcessManager.ViewModels
{
    public class LogManagerDataVM : INotifyPropertyChanged
    {

        private ObservableCollection<LogEntry> EntriesValue;

        /// <summary>
        /// Voci di log.
        /// </summary>
        public ObservableCollection<LogEntry> Entries 
        { 
            get
            {
                return EntriesValue;
            }
            set
            {
                if (EntriesValue != value)
                {
                    EntriesValue = value;
                    NotifyPropertyChanged(nameof(Entries));
                }
            }
        }

        /// <summary>
        /// Impostazioni di filtraggio della lista delle voci.
        /// </summary>
        public LoggingEntriesFilteringSettings FilterSettings { get; }

        /// <summary>
        /// Comando per filtrare la lista.
        /// </summary>
        public ICommand FilterListCommand { get; }

        /// <summary>
        /// Comando per pulire il file di log.
        /// </summary>
        public ICommand ClearLogCommand { get; }

        /// <summary>
        /// Comando per aggiornare le voci di log.
        /// </summary>
        public ICommand UpdateEntriesCommand { get; }


        public LogManagerDataVM()
        {
            Entries = new(Logger.GetLogEntries());
            FilterSettings = new(Entries);
            FilterListCommand = new FilterLogEntriesCommand(this);
            ClearLogCommand = new ClearLogCommand(this);
            UpdateEntriesCommand = new UpdateEntriesCommand(this);
        }

        private void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}