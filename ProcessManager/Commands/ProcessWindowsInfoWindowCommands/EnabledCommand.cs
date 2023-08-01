using ProcessManager.Models;
using ProcessManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessWindowsInfoWindowCommands
{
    public class EnabledCommand : ICommand
    {
        private readonly ProcessWindowsInfoVM VM;

        public EnabledCommand(ProcessWindowsInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (!VM.ChangeWindowEnableStatus(Parameter as WindowInfo))
            {
                MessageBox.Show(Properties.Resources.WindowEnableStatusErrorMessage, Properties.Resources.WindowEnableStatusErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}