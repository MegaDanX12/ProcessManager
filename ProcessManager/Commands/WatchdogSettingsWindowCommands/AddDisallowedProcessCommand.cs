using ProcessManager.Views;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.WatchdogSettingsWindowCommands
{
    public class AddDisallowedProcessCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            AddDisallowedProcessWindow Window = new();
            _ = Window.ShowDialog();
        }
    }
}