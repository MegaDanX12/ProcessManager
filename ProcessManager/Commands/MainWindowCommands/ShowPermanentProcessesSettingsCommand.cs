using ProcessManager.Views;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class ShowPermanentProcessesSettingsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            KeepOnlineProcessesSettingsWindow Window = new();
            _ = Window.ShowDialog();
        }
    }
}