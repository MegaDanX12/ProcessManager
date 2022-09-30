using ProcessManager.Watchdog;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.WatchdogSettingsWindowCommands
{
    public class EnableNotificationForDisallowedProcessTerminationCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            DisallowedProcess Process = parameter as DisallowedProcess;
            Process.EnableNotification();
        }
    }
}