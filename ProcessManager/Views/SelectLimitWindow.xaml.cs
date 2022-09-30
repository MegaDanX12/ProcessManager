using ProcessManager.Watchdog;
using System.Windows;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per SelectLimitWindow.xaml
    /// </summary>
    public partial class SelectLimitWindow : Window
    {
        /// <summary>
        /// Limite scelto.
        /// </summary>
        public CpuUsageLimitsData Limit { get; private set; }

        public SelectLimitWindow()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Limit = (CpuUsageLimitsData)LimitsComboBox.Items[LimitsComboBox.SelectedIndex];
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}