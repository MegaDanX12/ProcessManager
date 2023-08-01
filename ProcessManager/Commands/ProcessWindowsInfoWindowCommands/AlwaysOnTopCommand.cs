using ProcessManager.Models;
using ProcessManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessWindowsInfoWindowCommands
{
    public class AlwaysOnTopCommand : ICommand
    {
        private readonly ProcessWindowsInfoVM VM;

        public AlwaysOnTopCommand(ProcessWindowsInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (!VM.ChangeWindowTopMostStatus(Parameter as WindowInfo))
            {
                MessageBox.Show(Properties.Resources.WindowTopmostStatusChangeErrorMessage, Properties.Resources.WindowTopmostStatusChangeErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}