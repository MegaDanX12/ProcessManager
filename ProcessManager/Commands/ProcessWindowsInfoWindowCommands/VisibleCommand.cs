using ProcessManager.Models;
using ProcessManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessWindowsInfoWindowCommands
{
    public class VisibleCommand : ICommand
    {
        private readonly ProcessWindowsInfoVM VM;

        public VisibleCommand(ProcessWindowsInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (!VM.ChangeWindowVisibility(Parameter as WindowInfo))
            {
                MessageBox.Show(Properties.Resources.WindowVisibilityChangeErrorMessage, Properties.Resources.WindowVisibilityChangeErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}