using ProcessManager.ViewModels;
using System.Windows;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per WindowPropertiesWindow.xaml
    /// </summary>
    public partial class WindowPropertiesWindow : Window
    {
        public WindowPropertiesWindow(ProcessWindowsInfoVM VM)
        {
            InitializeComponent();
            DataContext = VM;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}