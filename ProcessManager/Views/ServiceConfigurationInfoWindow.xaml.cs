using ProcessManager.Commands.ServiceConfigurationInfoWindowCommands;
using ProcessManager.InfoClasses.ServicesInfo;
using ProcessManager.ViewModels;
using System.Windows;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per ServiceConfigurationInfoWindow.xaml
    /// </summary>
    public partial class ServiceConfigurationInfoWindow : Window
    {
        public ServiceConfigurationInfoWindow(HostedServicesDataVM VM)
        {
            DataContext = VM;
            ShowServiceOptionalConfigurationCommand Command = new(VM);
            Services.ServiceDeleted += Services_ServiceDeleted;
            InitializeComponent();
            ShowOptionalConfigurationInfoButton.Command = Command;
        }

        private void Services_ServiceDeleted(object sender, Service e)
        {
            HostedServicesDataVM VM = DataContext as HostedServicesDataVM;
            if (VM.CurrentService == e)
            {
                Services.ServiceDeleted -= Services_ServiceDeleted;
                Close();
            }
        }
    }
}