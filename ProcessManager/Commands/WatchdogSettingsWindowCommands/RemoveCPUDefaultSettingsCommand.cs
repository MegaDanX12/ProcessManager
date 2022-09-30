using ProcessManager.Watchdog;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.WatchdogSettingsWindowCommands
{
    public class RemoveCPUDefaultSettingsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            ProcessDefaultCPUSettings Setting = parameter as ProcessDefaultCPUSettings;
            WatchdogManager.RemoveProcessCPUDefaultSetting(Setting);
        }
    }
}