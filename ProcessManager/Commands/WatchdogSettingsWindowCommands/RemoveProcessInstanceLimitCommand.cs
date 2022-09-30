using ProcessManager.Watchdog;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.WatchdogSettingsWindowCommands
{
    public class RemoveProcessInstanceLimitCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            ProcessInstanceLimit Limit = parameter as ProcessInstanceLimit;
            WatchdogManager.RemoveProcessInstanceLimit(Limit);
        }
    }
}