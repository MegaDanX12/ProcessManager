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
    /// Logica di interazione per ServiceNotificationsReasonsWindow.xaml
    /// </summary>
    public partial class ServiceNotificationsReasonsWindow : Window
    {
        /// <summary>
        /// Stati del servizio.
        /// </summary>
        private readonly List<string> Reasons;

        /// <summary>
        /// Stato del servizio.
        /// </summary>
        private readonly string ServiceState;

        public ServiceNotificationsReasonsWindow(List<string> Reasons, string ServiceState)
        {
            this.Reasons = Reasons;
            this.ServiceState = ServiceState;
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (ServiceRunningCheckBox.IsChecked.Value)
            {
                Reasons.Add(Properties.Resources.ServiceRunningText);
            }
            if (ServicePausedCheckBox.IsChecked.Value)
            {
                Reasons.Add(Properties.Resources.ServiceStatePausedText);
            }
            if (ServiceStoppedCheckBox.IsChecked.Value)
            {
                Reasons.Add(Properties.Resources.ServiceStoppedText);
            }
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (ServiceState == Properties.Resources.ServiceRunningText)
            {
                ServiceRunningCheckBox.IsEnabled = false;
            }
            else if (ServiceState == Properties.Resources.ServiceStatePausedText)
            {
                ServicePausedCheckBox.IsEnabled = false;
            }
            else if (ServiceState == Properties.Resources.ServiceStoppedText)
            {
                ServiceStoppedCheckBox.IsEnabled = false;
            }
        }
    }
}