using ProcessManager.Watchdog;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per ProcessEnergyUsageSettingsWindow.xaml
    /// </summary>
    public partial class ProcessEnergyUsageSettingsWindow : Window
    {
        public ProcessEnergyUsageSettingsWindow()
        {
            InitializeComponent();
        }

        private void EnergyUsageRulesDatagrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (EnergyUsageRulesDatagrid.Items.Count > 0)
            {
                int RowIndex = UtilityMethods.GetRowIndexFromMouseClick(e.OriginalSource);
                AddProcessEnergyUsageRuleWindow Window = new(EnergyUsageRulesDatagrid.Items[RowIndex] as ProcessEnergyUsage);
                _ = Window.ShowDialog();
            }
        }
    }
}