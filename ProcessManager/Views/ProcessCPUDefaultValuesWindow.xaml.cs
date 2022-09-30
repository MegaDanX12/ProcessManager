using ProcessManager.Watchdog;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per ProcessCPUDefaultValuesWindow.xaml
    /// </summary>
    public partial class ProcessCPUDefaultValuesWindow : Window
    {
        public ProcessCPUDefaultValuesWindow()
        {
            InitializeComponent();
        }

        private void CPUDefaultSettingsDatagrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (CPUDefaultSettingsDatagrid.Items.Count > 0)
            {
                int RowIndex = UtilityMethods.GetRowIndexFromMouseClick(e.OriginalSource);
                AddProcessCPUDefaultSettingWindow Window = new(CPUDefaultSettingsDatagrid.Items[RowIndex] as ProcessDefaultCPUSettings);
                _ = Window.ShowDialog();
            }
        }
    }
}