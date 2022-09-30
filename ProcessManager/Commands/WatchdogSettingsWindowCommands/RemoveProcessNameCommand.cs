using ProcessManager.Watchdog;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.WatchdogSettingsWindowCommands
{
    public class RemoveProcessNameCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (!string.IsNullOrWhiteSpace(parameter as string))
            {
                WatchdogManager.RemoveProcess(parameter as string);
            }
            else
            {
                _ = MessageBox.Show(Properties.Resources.MemoryWatchdogNoProcessNameText, Properties.Resources.MemoryWatchdogNoProcessNameTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}