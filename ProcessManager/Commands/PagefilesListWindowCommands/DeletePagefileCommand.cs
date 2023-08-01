using ProcessManager.Models;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.PagefilesListWindowCommands
{
    public class DeletePagefileCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            PageFileInfo Info = parameter as PageFileInfo;
            if (!Info.Delete())
            {
                _ = MessageBox.Show(Properties.Resources.PagefileDeletionErrorMessage, Properties.Resources.PagefileOperationErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}