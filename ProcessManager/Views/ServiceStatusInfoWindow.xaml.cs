using ProcessManager.InfoClasses.ServicesInfo;
using ProcessManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per ServiceStatusInfoWindow.xaml
    /// </summary>
    public partial class ServiceStatusInfoWindow : Window
    {
        public ServiceStatusInfoWindow(HostedServicesDataVM VM)
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