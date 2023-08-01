using ProcessManager.ViewModels;
using System.Windows;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per PagefilesListWindow.xaml
    /// </summary>
    public partial class PagefilesListWindow : Window
    {
        public PagefilesListWindow()
        {
            InitializeComponent();
            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        private void Dispatcher_ShutdownStarted(object sender, System.EventArgs e)
        {
            ((PageFileListVM)DataContext).Dispose();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PagefilesListDatagrid_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            int Index = UtilityMethods.GetRowIndexFromMouseClick(e.OriginalSource);
            ChangePagefileSizesMenuItem.CommandParameter = PagefilesListDatagrid.Items[Index];
            DeletePagefileMenuItem.CommandParameter = PagefilesListDatagrid.Items[Index];
        }
    }
}