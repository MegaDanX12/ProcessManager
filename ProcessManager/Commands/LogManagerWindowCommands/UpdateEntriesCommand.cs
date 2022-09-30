using ProcessManager.ViewModels;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.LogManagerWindowCommands
{
    public class UpdateEntriesCommand : ICommand
    {
        private readonly LogManagerDataVM VM;

        public UpdateEntriesCommand(LogManagerDataVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            VM.Entries.Clear();
            LogEntry[] Entries = Logger.GetLogEntries();
            foreach (LogEntry entry in Entries)
            {
                VM.Entries.Add(entry);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}