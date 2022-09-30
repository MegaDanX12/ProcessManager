using ProcessManager.Models;
using ProcessManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessWindowsInfoWindowCommands
{
    public class RestoreWindowCommand : ICommand
    {
        private readonly ProcessWindowsInfoVM VM;

        public RestoreWindowCommand(ProcessWindowsInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (!VM.RestoreWindow(Parameter as WindowInfo))
            {
                MessageBox.Show(Properties.Resources.WindowRestoreErrorMessage, Properties.Resources.WindowRestoreErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}