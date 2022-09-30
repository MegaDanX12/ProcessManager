using ProcessManager.Watchdog;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessLimiterSettingsWindowCommands
{
    public class RemovePathCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Tuple<CpuUsageLimitsData, string> Parameters = (Tuple<CpuUsageLimitsData, string>)parameter;
            _ = Parameters.Item1.ExecutablePaths.Remove(Parameters.Item2);
        }
    }
}