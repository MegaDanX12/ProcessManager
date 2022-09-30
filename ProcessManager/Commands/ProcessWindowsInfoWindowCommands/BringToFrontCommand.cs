using ProcessManager.Models;
using ProcessManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessWindowsInfoWindowCommands
{
    public class BringToFrontCommand : ICommand
    {
        private readonly ProcessWindowsInfoVM VM;

        public BringToFrontCommand(ProcessWindowsInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (!VM.BringWindowToFront(Parameter as WindowInfo))
            {
                MessageBox.Show(Properties.Resources.WindowBringToFrontErrorMessage, Properties.Resources.WindowBringToFrontErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}