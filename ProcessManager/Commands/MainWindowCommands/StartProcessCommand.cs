using ProcessManager.ViewModels;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class StartProcessCommand : ICommand
    {
        private readonly ProcessInfoVM VM;

        public StartProcessCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            VM.StartProcess();
        }

        public event EventHandler CanExecuteChanged;
    }
}