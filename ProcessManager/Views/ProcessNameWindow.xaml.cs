using ProcessManager.Models;
using ProcessManager.Watchdog;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per ProcessNameWindow.xaml
    /// </summary>
    public partial class ProcessNameWindow : Window
    {
        /// <summary>
        /// Processi in esecuzione al momento della visualizzazione della finestra.
        /// </summary>
        private readonly List<string> RunningProcesses = new();

        public ProcessNameWindow(List<ProcessInfo> Processes)
        {
            foreach (ProcessInfo info in Processes)
            {
                RunningProcesses.Add(info.Name);
            }
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ProcessNameTextbox.Text))
            {
                string Extension = Path.GetExtension(ProcessNameTextbox.Text);
                if (!string.IsNullOrWhiteSpace(Extension))
                {
                    if (Extension != ".exe")
                    {
                        _ = MessageBox.Show(Properties.Resources.MemoryWatchdogInvalidNameText, Properties.Resources.MemoryWatchdogInvalidNameTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        if (!WatchdogManager.AddProcess(ProcessNameTextbox.Text))
                        {
                            _ = MessageBox.Show(Properties.Resources.MemoryWatchdogUnableToAddProcessText, Properties.Resources.MemoryWatchdogUnableToAddProcessTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            Close();
                        }
                    }
                }
                else
                {
                    ProcessNameTextbox.Text += ".exe";
                    if (!WatchdogManager.AddProcess(ProcessNameTextbox.Text))
                    {
                        _ = MessageBox.Show(Properties.Resources.MemoryWatchdogUnableToAddProcessText, Properties.Resources.MemoryWatchdogUnableToAddProcessTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        Close();
                    }
                }
            }
            else
            {
                _ = MessageBox.Show(Properties.Resources.MemoryWatchdogNoProcessNameText, Properties.Resources.MemoryWatchdogNoProcessNameTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}