using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ProcessManager.Views;
using ProcessManager.Watchdog;

namespace ProcessManager.Commands.ProcessLimiterSettingsWindowCommands
{
    public class AddLimitCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            AddLimitWindow Window = new();
            _ = Window.ShowDialog();
            if (ProcessLimiter.ProcessCpuLimits.Any(limit => limit.UsageLimit == Window.NewLimitValue))
            {
                _ = MessageBox.Show(Properties.Resources.ProcessLimiterAddLimitAlreadyExistsMessage, Properties.Resources.ProcessLimiterAddLimitAlreadyExistsTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                if (!ProcessLimiter.AddNewLimit(Window.NewLimitValue))
                {
                    _ = MessageBox.Show(Properties.Resources.ProcessLimiterAddLimitErrorMessage, Properties.Resources.ProcessLimiterLimitsEditErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}