using Microsoft.Win32;
using System.Windows;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per AddLimitedAppPathWindow.xaml
    /// </summary>
    public partial class AddLimitedAppPathWindow : Window
    {
        /// <summary>
        /// Percorso applicazione.
        /// </summary>
        public string AppPath { get; private set; }

        public AddLimitedAppPathWindow()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            AppPath = AppPathTextbox.Text;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ApplicationPathSelectionOpenFileDialog = new()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "exe",
                Multiselect = false,
                Title = Properties.Resources.ProcessLimiterPathSelectionDialogTitle,
                Filter = Properties.Resources.OpenFileDialogExecutableFilter
            };
            if (ApplicationPathSelectionOpenFileDialog.ShowDialog().Value)
            {
                AppPathTextbox.Text = ApplicationPathSelectionOpenFileDialog.FileName;
            }
        }
    }
}