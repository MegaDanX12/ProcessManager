using ProcessManager.Watchdog;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.WatchdogSettingsWindowCommands
{
    public class EnableNotificationForPermanentProcessesCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            PermanentProcess PermanentProcess = parameter as PermanentProcess;
            PermanentProcess.EnableNotification();
        }
    }
}