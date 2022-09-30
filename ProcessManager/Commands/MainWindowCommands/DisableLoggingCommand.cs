using ProcessManager.ViewModels;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class DisableLoggingCommand : ICommand
    {
        private readonly ProcessInfoVM VM;

        public DisableLoggingCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            Settings.LogProgramActivity = false;
        }

        public event EventHandler CanExecuteChanged;
    }
}