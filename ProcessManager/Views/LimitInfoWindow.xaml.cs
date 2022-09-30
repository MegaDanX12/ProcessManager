using ProcessManager.ViewModels;
using System.Windows;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per LimitInfoWindow.xaml
    /// </summary>
    public partial class LimitInfoWindow : Window
    {
        public LimitInfoWindow(JobInfoVM VM)
        {
            DataContext = VM;
            InitializeComponent();
        }
    }
}