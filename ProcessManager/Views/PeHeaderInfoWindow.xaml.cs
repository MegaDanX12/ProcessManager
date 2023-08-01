using ProcessManager.ViewModels;
using System.Windows;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per PeHeaderInfoWindow.xaml
    /// </summary>
    public partial class PeHeaderInfoWindow : Window
    {
        public PeHeaderInfoWindow(PeHeaderInfoVM VM)
        {
            DataContext = VM;
            InitializeComponent();
        }
    }
}