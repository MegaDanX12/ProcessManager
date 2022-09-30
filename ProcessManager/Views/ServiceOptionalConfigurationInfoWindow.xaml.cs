using ProcessManager.InfoClasses.ServicesInfo;
using ProcessManager.ViewModels;
using System.Windows;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per ServiceOptionalConfigurationInfoWindow.xaml
    /// </summary>
    public partial class ServiceOptionalConfigurationInfoWindow : Window
    {
        public ServiceOptionalConfigurationInfoWindow(HostedServicesDataVM VM)
        {
            DataContext = VM;
            Services.ServiceDeleted += Services_ServiceDeleted;
            InitializeComponent();
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