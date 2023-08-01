using ProcessManager.Models;
using ProcessManager.Views;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.PagefilesListWindowCommands
{
    public class ChangePagefileSizeCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            PagefileInfoWindow Window = new("ChangeSize", parameter as PageFileInfo);
            if (!Window.ShowDialog().Value)
            {
                _ = MessageBox.Show(Properties.Resources.PagefileChangeSizeErrorMessage, Properties.Resources.PagefileOperationErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}