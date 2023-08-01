using ProcessManager.ViewModels;
using ProcessManager.Models;
using System;
using System.Windows.Input;
using System.Windows;

namespace ProcessManager.Commands.ProcessWindowsInfoWindowCommands
{
    public class MinimizeCommand : ICommand
    {
        private readonly ProcessWindowsInfoVM VM;

        public MinimizeCommand(ProcessWindowsInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (!VM.MinimizeWindow(Parameter as WindowInfo))
            {
                MessageBox.Show(Properties.Resources.WindowMinimizeErrorMessage, Properties.Resources.WindowMinimizeErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}