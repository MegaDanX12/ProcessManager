using ProcessManager.Views;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.WatchdogSettingsWindowCommands
{
    public class AddRuleCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            uint? MemorySize = NativeHelpers.GetSystemMemorySize();
            uint? ProcessorCoresCount = NativeHelpers.GetNumberOfProcessorCores();
            if (MemorySize.HasValue && ProcessorCoresCount.HasValue)
            {
                AddRuleWindow Window = new(MemorySize.Value, ProcessorCoresCount.Value);
                _ = Window.ShowDialog();
            }
            else
            {
                _ = MessageBox.Show(Properties.Resources.UnableToRetrieveInfoWatchdogErrorMessage, Properties.Resources.UnableToRetrieveInfoWatchdogErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}