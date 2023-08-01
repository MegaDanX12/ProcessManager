using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per KeepOnlineProcessesSettingsWindow.xaml
    /// </summary>
    public partial class KeepOnlineProcessesSettingsWindow : Window
    {
        public KeepOnlineProcessesSettingsWindow()
        {
            InitializeComponent();
        }

        private void PermanentProcessesDatagrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (PermanentProcessesDatagrid.Items.Count > 0)
            {
                int RowIndex = UtilityMethods.GetRowIndexFromMouseClick(e.OriginalSource);
                PermanentProcessEnableNotificationMenuItem.CommandParameter = PermanentProcessesDatagrid.Items[RowIndex];
                PermanentProcessDisableNotificationMenuItem.CommandParameter = PermanentProcessesDatagrid.Items[RowIndex];
            }
        }
    }
}