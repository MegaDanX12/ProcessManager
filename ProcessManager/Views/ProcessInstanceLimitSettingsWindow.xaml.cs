using ProcessManager.Watchdog;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per ProcessInstaceLimitSettingsWindow.xaml
    /// </summary>
    public partial class ProcessInstanceLimitSettingsWindow : Window
    {
        public ProcessInstanceLimitSettingsWindow()
        {
            InitializeComponent();
        }

        private void InstanceLimitsDatagrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (InstanceLimitsDatagrid.Items.Count > 0)
            {
                int RowIndex = UtilityMethods.GetRowIndexFromMouseClick(e.OriginalSource);
                AddProcessInstanceLimitWindow Window = new(InstanceLimitsDatagrid.Items[RowIndex] as ProcessInstanceLimit);
                _ = Window.ShowDialog();
            }
        }
    }
}