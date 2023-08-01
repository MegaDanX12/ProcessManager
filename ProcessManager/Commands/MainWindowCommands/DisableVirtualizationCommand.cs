using ProcessManager.Models;
using ProcessManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class DisableVirtualizationCommand : ICommand
    {
        private readonly ProcessInfoVM VM;

        public DisableVirtualizationCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (!VM.DisableVirtualization(Parameter as ProcessInfo))
            {
                _ = MessageBox.Show(Properties.Resources.DisableVirtualizationErrorMessage, Properties.Resources.DisableVirtualizationErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}