using ProcessManager.InfoClasses.ServicesInfo;
using ProcessManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.ServicesInfoWindowCommands
{
    public class StopServiceStatusMonitoringCommand : ICommand
    {
        private readonly HostedServicesDataVM VM;

        public StopServiceStatusMonitoringCommand(HostedServicesDataVM VM)
        {
            this.VM = VM;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (!VM.StopServiceMonitoring(Parameter as Service))
            {
                MessageBox.Show(Properties.Resources.ServiceMonitoringStopErrorMessage, Properties.Resources.ServiceMonitoringStopErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}