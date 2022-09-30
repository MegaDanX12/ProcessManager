using ProcessManager.ViewModels;
using ProcessManager.Watchdog;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per ProcessLimiterSettingsWindow.xaml
    /// </summary>
    public partial class ProcessLimiterSettingsWindow : Window
    {
        public ProcessLimiterSettingsWindow(ProcessLimiterSettingsVM VM)
        {
            DataContext = VM;
            InitializeComponent();
        }

        private void CpuLimitsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CpuUsageLimitsData Limit = (CpuUsageLimitsData)CpuLimitsComboBox.SelectedItem;
            if (Limit is not null)
            {
                foreach (string path in Limit.ExecutablePaths)
                {
                    _ = LimitedApplicationsListBox.Items.Add(path);
                }
                AddPathButton.CommandParameter = new Tuple<CpuUsageLimitsData, ProcessInfoVM>(Limit, ((ProcessLimiterSettingsVM)DataContext).MainViewModel);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LimitedApplicationsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Tuple<CpuUsageLimitsData, string> Parameters = new((CpuUsageLimitsData)CpuLimitsComboBox.SelectedItem, (string)LimitedApplicationsListBox.SelectedItem);
            RemovePathButton.CommandParameter = Parameters;
        }
    }
}