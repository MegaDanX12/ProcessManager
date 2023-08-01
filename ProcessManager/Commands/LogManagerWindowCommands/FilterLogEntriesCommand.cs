using ProcessManager.ViewModels;
using ProcessManager.Views;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ProcessManager.Commands.LogManagerWindowCommands
{
    public class FilterLogEntriesCommand : ICommand
    {
        private readonly LogManagerDataVM VM;

        public FilterLogEntriesCommand(LogManagerDataVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            LogEntriesFilterSettingsWindow Window = new(VM);
            Window.ShowDialog();
            ObservableCollection<LogEntry> FilteredCollection = VM.FilterSettings.ApplyFilters(VM.Entries);
            VM.Entries = FilteredCollection;
        }

        public event EventHandler CanExecuteChanged;
    }
}