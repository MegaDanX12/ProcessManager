using ProcessManager.Watchdog;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.WatchdogSettingsWindowCommands
{
    public class RemoveProcessEnergyUsageRuleCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            ProcessEnergyUsage Rule = parameter as ProcessEnergyUsage;
            WatchdogManager.RemoveProcessEnergyUsageRule(Rule);
        }
    }
}