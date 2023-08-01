using ProcessManager.Commands.ProcessWindowsInfoWindowCommands;
using ProcessManager.Models;
using ProcessManager.ViewModels;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per ProcessWindowsInfoWindow.xaml
    /// </summary>
    public partial class ProcessWindowsInfo : Window
    {
        private Tuple<ProcessWindowsInfo, WindowInfo> PropertiesParameters;
        public ProcessWindowsInfo(ProcessInfo AssociatedProcess = null, ThreadInfo ThreadInfo = null)
        {
            ProcessWindowsInfoVM VM;
            if (AssociatedProcess is not null)
            {
                VM = new(AssociatedProcess);
            }
            else
            {
                VM = ThreadInfo is not null ? (new(null, ThreadInfo: ThreadInfo)) : (new(null));
            }
            DataContext = VM;
            InitializeComponent();
            VM.WindowDispatcher = Dispatcher;
            VM.CreateHook();
            if (AssociatedProcess != null)
            {
                AssociatedProcess.Exit += AssociatedProcess_Exit;
            }
            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            ((ProcessWindowsInfoVM)DataContext).Dispose();
        }

        private void AssociatedProcess_Exit(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => Close());
        }

        private void WindowsInfoGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            int SelectedIndex = UtilityMethods.GetRowIndexFromMouseClick(e.OriginalSource);
            WindowInfo Info = (WindowInfo)WindowsInfoGrid.Items[SelectedIndex];
            PropertiesParameters = new Tuple<ProcessWindowsInfo, WindowInfo>(this, Info);
            WindowPropertiesMenuItem.CommandParameter = PropertiesParameters;
            VisibleMenuItem.IsChecked = Info.IsWindowVisible();
            EnabledMenuItem.IsChecked = Info.IsWindowEnabled();
            AlwaysOnTopMenuItem.IsChecked = Info.IsWindowAlwaysOnTop();
            foreach (MenuItem item in OpacityMenuItem.Items)
            {
                item.IsChecked = false;
            }
            string TransparencyPercentage = Info.GetTransparencyPercentage().ToString("D0", CultureInfo.InvariantCulture);
            if (TransparencyPercentage != "-1")
            {
                foreach (MenuItem item in OpacityMenuItem.Items)
                {
                    if ((string)item.Header == TransparencyPercentage + "%")
                    {
                        item.IsChecked = true;
                        break;
                    }
                }
            }
            VisibleMenuItem.CommandParameter = Info;
            EnabledMenuItem.CommandParameter = Info;
            AlwaysOnTopMenuItem.CommandParameter = Info;
            BringToFrontMenuItem.CommandParameter = Info;
            RestoreMenuItem.CommandParameter = Info;
            MinimizeMenuItem.CommandParameter = Info;
            MaximizeMenuItem.CommandParameter = Info;
            CloseMenuItem.CommandParameter = Info;
        }

        private void OpacityMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WindowInfo Info = (WindowInfo)WindowsInfoGrid.SelectedItem;
            MenuItem FormerCheckedItem = null;
            MenuItem ClickedItem = sender as MenuItem;
            foreach (MenuItem item in OpacityMenuItem.Items)
            {
                if (item.IsChecked && !ReferenceEquals(item, ClickedItem))
                {
                    FormerCheckedItem = item;
                    item.IsChecked = false;
                    break;
                }
            }
            string ValueString = ((string)ClickedItem.Header).Replace('%', '\0');
            byte NewValue = Convert.ToByte(ValueString, CultureInfo.InvariantCulture);
            if (FormerCheckedItem != null)
            {
                FormerCheckedItem.IsChecked = !Info.SetTransparencyPercentage(NewValue);
            }
            else
            {
                ClickedItem.IsChecked = true;
            }
        }

        private void WindowsInfoGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int SelectedIndex = UtilityMethods.GetRowIndexFromMouseClick(e.OriginalSource);
            Tuple<ProcessWindowsInfo, WindowInfo> Parameters = new(this, (WindowInfo)WindowsInfoGrid.Items[SelectedIndex]);
            WindowPropertiesCommand Command = new((ProcessWindowsInfoVM)DataContext);
            Command.Execute(Parameters);
        }
    }
}