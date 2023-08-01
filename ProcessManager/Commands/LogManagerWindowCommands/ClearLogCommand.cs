using ProcessManager.ViewModels;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.LogManagerWindowCommands
{
    public class ClearLogCommand : ICommand
    {
        private readonly LogManagerDataVM VM;

        public ClearLogCommand(LogManagerDataVM VM)
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
            Logger.ClearLog();
        }

        public event EventHandler CanExecuteChanged;
    }
}