using ProcessManager.InfoClasses.HandleSpecificInfo;
using ProcessManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.SemaphoreInfoWindowCommands
{
    public class ReleaseSemaphoreCommand : ICommand
    {
        private readonly ProcessPropertiesVM VM;

        public ReleaseSemaphoreCommand(ProcessPropertiesVM VM)
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
            if (!((SemaphoreInfo)Parameter).ReleaseSemaphore())
            {
                MessageBox.Show(Properties.Resources.ReleaseSemaphoreErrorMessage, Properties.Resources.ReleaseSemaphoreErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}