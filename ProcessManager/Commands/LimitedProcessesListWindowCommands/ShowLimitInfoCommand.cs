using ProcessManager.Watchdog;
using ProcessManager.ViewModels;
using ProcessManager.Views;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.LimitedProcessesListWindowCommands
{
    public class ShowLimitInfoCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            CpuUsageLimitsData Limit = parameter as CpuUsageLimitsData;
            JobInfoVM VM = new(Limit);
            LimitInfoWindow Window = new(VM);
            _ = Window.ShowDialog();
        }
    }
}