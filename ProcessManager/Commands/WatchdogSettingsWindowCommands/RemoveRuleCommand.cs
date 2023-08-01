using ProcessManager.Watchdog;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.WatchdogSettingsWindowCommands
{
    public class RemoveRuleCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            ProcessWatchdogRule Rule = parameter as ProcessWatchdogRule;
            WatchdogManager.RemoveRule(Rule);
        }
    }
}