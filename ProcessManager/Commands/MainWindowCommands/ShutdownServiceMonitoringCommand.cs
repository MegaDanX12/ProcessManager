using ProcessManager.ViewModels;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class ShutdownServiceMonitoringCommand : ICommand
    {
        private readonly ProcessInfoVM VM;

        public ShutdownServiceMonitoringCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            Settings.ServiceMonitoringEnabled = false;
            VM.ShutdownServiceMonitoring();
        }

        public event EventHandler CanExecuteChanged;
    }
}