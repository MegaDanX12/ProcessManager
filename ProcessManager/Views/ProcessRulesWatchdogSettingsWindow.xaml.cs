using System.Windows;
using System.Windows.Input;
using ProcessManager.ViewModels;
using ProcessManager.Watchdog;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per WatchdogSettingsWindow.xaml
    /// </summary>
    public partial class ProcessRulesWatchdogSettingsWindow : Window
    {
        public ProcessRulesWatchdogSettingsWindow()
        {
            InitializeComponent();
        }

        private void RulesDatagrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (RulesDatagrid.Items.Count > 0)
            {
                int RowIndex = UtilityMethods.GetRowIndexFromMouseClick(e.OriginalSource);
                if (WatchdogSettingVM.SystemMemorySizeMB.HasValue && WatchdogSettingVM.ProcessorCoresCount.HasValue)
                {
                    AddRuleWindow Window = new(WatchdogSettingVM.SystemMemorySizeMB.Value, WatchdogSettingVM.ProcessorCoresCount.Value, (ProcessWatchdogRule)RulesDatagrid.Items[RowIndex]);
                    _ = Window.ShowDialog();
                }
                else
                {
                    _ = MessageBox.Show(Properties.Resources.UnableToRetrieveInfoWatchdogErrorMessage, Properties.Resources.UnableToRetrieveInfoWatchdogErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ProcessRulesWatchdogSettingsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.ProcessWatchdogEnabled = ProcessWatchdogEnabledCheckbox.IsChecked.Value;
        }
    }
}