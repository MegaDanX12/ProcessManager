using ProcessManager.InfoClasses.ServicesInfo;
using ProcessManager.ViewModels;
using ProcessManager.Views;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.ServicesInfoWindowCommands
{
    public class ListenForServiceEventsCommand : ICommand
    {
        private readonly HostedServicesDataVM VM;

        public ListenForServiceEventsCommand(HostedServicesDataVM VM)
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
            Service Service = Parameter as Service;
            List<string> Reasons = new();
            ServiceNotificationsReasonsWindow Window = new(Reasons, Service.State);
            Window.ShowDialog();
            if (!VM.StartServiceMonitoring(Service, Reasons.ToArray()))
            {
                MessageBox.Show(Properties.Resources.ServiceMonitoringStartErrorMessage, Properties.Resources.ServiceMonitoringStartErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}