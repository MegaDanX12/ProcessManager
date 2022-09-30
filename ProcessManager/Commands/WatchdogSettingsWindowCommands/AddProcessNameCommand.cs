using ProcessManager.Views;
using ProcessManager.Watchdog;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.WatchdogSettingsWindowCommands
{
    public class AddProcessNameCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            ProcessNameWindow Window = new(WatchdogManager.ProcessesData.GetRunningProcesses());
            _ = Window.ShowDialog();
        }
    }
}