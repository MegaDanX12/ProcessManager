using Microsoft.Win32.SafeHandles;
using ProcessManager.InfoClasses.HandleSpecificInfo;
using ProcessManager.Models;
using ProcessManager.ViewModels;
using ProcessManager.Views;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.SemaphoreInfoWindowCommands
{
    public class AcquireSemaphoreCommand : ICommand
    {
        private readonly ProcessPropertiesVM VM;

        public AcquireSemaphoreCommand(ProcessPropertiesVM VM)
        {
            this.VM = VM;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (!((SemaphoreInfo)Parameter).AcquireSemaphore())
            {
                MessageBox.Show(Properties.Resources.AcquireSemaphoreErrorMessage, Properties.Resources.AcquireSemaphoreErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}