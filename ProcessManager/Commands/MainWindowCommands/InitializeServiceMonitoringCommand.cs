using ProcessManager.ViewModels;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class InitializeServiceMonitoringCommand : ICommand
    {
        private readonly ProcessInfoVM VM;

        public InitializeServiceMonitoringCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            Settings.ServiceMonitoringEnabled = true;
            VM.InitializeServiceMonitoring();
        }

        public event EventHandler CanExecuteChanged;
    }
}
