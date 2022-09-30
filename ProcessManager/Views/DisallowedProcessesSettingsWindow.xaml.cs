using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per DisallowedProcessesSettingsWindow.xaml
    /// </summary>
    public partial class DisallowedProcessesSettingsWindow : Window
    {
        public DisallowedProcessesSettingsWindow()
        {
            InitializeComponent();
        }

        private void DisallowedProcessesDatagrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DisallowedProcessesDatagrid.Items.Count > 0)
            {
                int RowIndex = UtilityMethods.GetRowIndexFromMouseClick(e.OriginalSource);
                DisallowedProcessEnableNotificationMenuItem.CommandParameter = DisallowedProcessesDatagrid.Items[RowIndex];
                DisallowedProcessDisableNotificationMenuItem.CommandParameter = DisallowedProcessesDatagrid.Items[RowIndex];
            }
        }
    }
}