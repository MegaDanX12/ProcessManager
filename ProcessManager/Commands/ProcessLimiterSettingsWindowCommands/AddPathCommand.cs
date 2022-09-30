using ProcessManager.ViewModels;
using ProcessManager.Watchdog;
using ProcessManager.Views;
using System;
using System.Windows.Input;
using System.Windows;

namespace ProcessManager.Commands.ProcessLimiterSettingsWindowCommands
{
    public class AddPathCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            AddLimitedAppPathWindow Window = new();
            _ = Window.ShowDialog();
            if (!string.IsNullOrWhiteSpace(Window.AppPath))
            {
                Tuple<CpuUsageLimitsData, ProcessInfoVM> Parameters = (Tuple<CpuUsageLimitsData, ProcessInfoVM>)parameter;
                if (!Parameters.Item1.ExecutablePaths.Contains(Window.AppPath))
                {
                    bool PathAlreadyExists = false;
                    foreach (CpuUsageLimitsData limit in ProcessLimiter.ProcessCpuLimits)
                    {
                        if (!ReferenceEquals(limit, Parameters.Item1))
                        {
                            if (limit.ExecutablePaths.Contains(Window.AppPath))
                            {
                                PathAlreadyExists = true;
                            }
                        }
                    }
                    if (!PathAlreadyExists)
                    {
                        Parameters.Item1.ExecutablePaths.Add(Window.AppPath);
                        ProcessLimiter.LimitRunningProcesses(Parameters.Item1, Window.AppPath, Parameters.Item2.GetRunningProcesses());
                    }
                    else
                    {
                        _ = MessageBox.Show(Properties.Resources.ProcessLimiterApplicationAlreadyLimitedErrorMessage, Properties.Resources.ProcessLimiterApplicationAlreadyLimitedErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    _ = MessageBox.Show(Properties.Resources.ProcessLimiterApplicationAlreadyLimitedErrorMessage, Properties.Resources.ProcessLimiterApplicationAlreadyLimitedErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}