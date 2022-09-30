using ProcessManager.ViewModels;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.WindowPropertiesWindowCommands
{
    public class RefreshWindowDataCommand : ICommand
    {
        private readonly ProcessWindowsInfoVM VM;

        public RefreshWindowDataCommand(ProcessWindowsInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            VM.UpdateSingleWindowData();
        }

        public event EventHandler CanExecuteChanged;
    }
}