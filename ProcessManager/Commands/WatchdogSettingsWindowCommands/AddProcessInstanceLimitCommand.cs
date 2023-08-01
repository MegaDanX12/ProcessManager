using ProcessManager.Views;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.WatchdogSettingsWindowCommands
{
    public class AddProcessInstanceLimitCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            AddProcessInstanceLimitWindow Window = new();
            _ = Window.ShowDialog();
        }
    }
}