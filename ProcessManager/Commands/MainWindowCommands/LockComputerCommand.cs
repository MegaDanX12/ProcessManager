using ProcessManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class LockComputerCommand : ICommand
    {
        private readonly ProcessInfoVM VM;

        public LockComputerCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (!VM.LockComputer())
            {
                _ = MessageBox.Show(Properties.Resources.LockComputerErrorMessage, Properties.Resources.LockComputerErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}