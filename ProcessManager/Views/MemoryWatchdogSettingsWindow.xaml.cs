using ProcessManager.ViewModels;
using ProcessManager.Watchdog;
using System;
using System.Globalization;
using System.Windows;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per MemoryWatchdogSettingsWindow.xaml
    /// </summary>
    public partial class MemoryWatchdogSettingsWindow : Window
    {
        public MemoryWatchdogSettingsWindow()
        {
            InitializeComponent();
        }

        private void TerminateProcessesHighMemoryUsageCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (!WatchdogSettingVM.SystemMemorySizeMB.HasValue)
            {
                TerminateProcessesHighMemoryUsageCheckbox.IsChecked = false;
                ProcessMaxMemoryUsageTextbox.Text = "3072";
                _ = MessageBox.Show(Properties.Resources.MemoryWatchdogUnavailableInfoText, Properties.Resources.MemoryWatchdogUnavailableInfoTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void MemorySettingsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (CheckData())
            {
                Settings.SystemMemoryWatchdogEnabled = SystemMemoryWatchdogEnabledCheckbox.IsChecked.Value;
                Settings.MaxMemoryUsagePercentage = Convert.ToUInt32(MemoryUsageTextbox.Text, CultureInfo.CurrentCulture);
                Settings.TerminateProcessesHighMemoryUsage = TerminateProcessesHighMemoryUsageCheckbox.IsChecked.Value;
                Settings.MaxProcessMemoryUsage = Convert.ToUInt32(ProcessMaxMemoryUsageTextbox.Text, CultureInfo.CurrentCulture);
                Settings.TerminateNamedProcesses = TerminateNamedProcessesCheckbox.IsChecked.Value;
                Settings.EnableLowSystemMemoryConditionMonitoring = EnableLowSystemMemoryConditionMonitoringCheckbox.IsChecked.Value;
                Settings.ShowNotificationForLowMemoryCondition = ShowLowMemoryConditionNotificationCheckbox.IsChecked.Value;
                Settings.CleanSystemMemoryIfLow = CleanMemoryIfLowCheckbox.IsChecked.Value;
                if (Settings.SystemMemoryWatchdogEnabled && Settings.EnableLowSystemMemoryConditionMonitoring && !WatchdogManager.IsSystemMemoryMonitored())
                {
                    WatchdogManager.InitializeMemoryMonitoring();
                }
            }
            else
            {
                _ = MessageBox.Show(Properties.Resources.InvalidMemoryWatchdogDataText, Properties.Resources.InvalidMemoryWatchdogDataTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Controlla che i dati forniti siano validi.
        /// </summary>
        /// <returns>true se i dati sono validi, false altrimenti.</returns>
        private bool CheckData()
        {
            if (string.IsNullOrWhiteSpace(MemoryUsageTextbox.Text))
            {
                MemoryUsageTextbox.Text = "0";
            }
            else
            {
                if (!uint.TryParse(MemoryUsageTextbox.Text, out uint Result))
                {
                    return false;
                }
                else
                {
                    if (Result > 100)
                    {
                        return false;
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(ProcessMaxMemoryUsageTextbox.Text))
            {
                return false;
            }
            else
            {
                if (!uint.TryParse(ProcessMaxMemoryUsageTextbox.Text, out uint Result))
                {
                    return false;
                }
                else
                {
                    if (WatchdogSettingVM.SystemMemorySizeMB.HasValue)
                    {
                        if (Result is 0 || Result > WatchdogSettingVM.SystemMemorySizeMB.Value)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}