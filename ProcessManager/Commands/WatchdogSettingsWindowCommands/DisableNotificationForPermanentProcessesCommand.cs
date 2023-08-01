using System;
using System.Windows.Input;
using ProcessManager.Watchdog;

namespace ProcessManager.Commands.WatchdogSettingsWindowCommands
{
    public class DisableNotificationForPermanentProcessesCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            PermanentProcess PermanentProcess = parameter as PermanentProcess;
            PermanentProcess.DisableNotification();
        }
    }
}