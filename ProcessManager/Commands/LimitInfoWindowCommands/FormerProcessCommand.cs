using ProcessManager.ViewModels;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.LimitInfoWindowCommands
{
    public class FormerProcessCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly JobInfoVM VM;

        public FormerProcessCommand(JobInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            VM.FormerProcess();
        }
    }
}