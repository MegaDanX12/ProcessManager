using ProcessManager.Models;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.LimitedProcessesListWindowCommands
{
    public class TerminateProcessCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (!((ProcessInfo)parameter).TerminateProcess())
            {
                _ = MessageBox.Show(Properties.Resources.ProcessTerminationFailedMessage, Properties.Resources.ProcessTerminationFailedTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}