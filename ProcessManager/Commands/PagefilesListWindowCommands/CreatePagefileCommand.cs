using ProcessManager.Views;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.PagefilesListWindowCommands
{
    public class CreatePagefileCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            PagefileInfoWindow Window = new("CreateNew");
            if (!Window.ShowDialog().Value)
            {
                _ = MessageBox.Show(Properties.Resources.PagefileCreationErrorMessage, Properties.Resources.PagefileOperationErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}