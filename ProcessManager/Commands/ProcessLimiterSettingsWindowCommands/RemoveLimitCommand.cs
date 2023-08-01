using ProcessManager.Watchdog;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessLimiterSettingsWindowCommands
{
    public class RemoveLimitCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (!ProcessLimiter.RemoveLimit((CpuUsageLimitsData)parameter, out bool LimitedProcessesExist))
            {
                _ = MessageBox.Show(Properties.Resources.ProcessLimiterRemoveLimitErrorMessage, Properties.Resources.ProcessLimiterLimitsEditErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                if (LimitedProcessesExist)
                {
                    _ = MessageBox.Show(Properties.Resources.RemoveLimitProcessesRunningWarningMessage, Properties.Resources.RemoveLimitProcessesRunningWarningTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}