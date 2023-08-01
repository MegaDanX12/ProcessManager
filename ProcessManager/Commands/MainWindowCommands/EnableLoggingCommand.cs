using ProcessManager.ViewModels;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class EnableLoggingCommand : ICommand
    {
        private readonly ProcessInfoVM VM;

        public EnableLoggingCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            Settings.LogProgramActivity = true;
            Logger.Initialize(Settings.LogsPath);
        }

        public event EventHandler CanExecuteChanged;
    }
}