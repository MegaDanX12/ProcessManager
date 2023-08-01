using ProcessManager.Models;
using System.Linq;
using System.Windows;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per AddLimitWindow.xaml
    /// </summary>
    public partial class AddLimitWindow : Window
    {
        /// <summary>
        /// Valore del nuovo limite.
        /// </summary>
        public byte NewLimitValue { get; private set; }
        public AddLimitWindow()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (!byte.TryParse(LimitValueTextBox.Text, out byte LimitValue))
            {
                _ = MessageBox.Show(Properties.Resources.AddLimitWindowInvalidLimitValueErrorMessage, Properties.Resources.AddLimitWindowInvalidLimitValueErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                if (LimitValue is 0 or >= 100)
                {
                    _ = MessageBox.Show(Properties.Resources.AddLimitWindowInvalidLimitValueErrorMessage, Properties.Resources.AddLimitWindowInvalidLimitValueErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    NewLimitValue = LimitValue;
                    Close();
                }
            }
        }
    }
}