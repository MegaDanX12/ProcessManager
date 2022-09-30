using ProcessManager.InfoClasses.ServicesInfo;
using ProcessManager.Models;
using ProcessManager.ViewModels;
using ProcessManager.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class ShowServicesListCommand : ICommand
    {
        private readonly ProcessInfoVM VM;

        public ShowServicesListCommand(ProcessInfoVM VM)
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
            if (VM.ServicesData.FeatureAvailable)
            {
                HostedServicesDataVM ServicesVM = new(null, null, VM.ServicesData);
                ServicesInfoWindow Window = new(ServicesVM);
                Window.Show();
            }
            else
            {
                MessageBox.Show(Properties.Resources.ServiceMonitoringUnavailableErrorMessage, Properties.Resources.ServiceMonitoringUnavailableErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}