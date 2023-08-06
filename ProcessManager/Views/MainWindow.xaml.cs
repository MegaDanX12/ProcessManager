using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Globalization;
using System.Collections;
using ProcessManager.Models;
using ProcessManager.ViewModels;
using System.Security;
using ProcessManager.Commands.MainWindowCommands;
using static ProcessManager.NativeHelpers;
using System.Text;
using ProcessManager.Watchdog;

namespace ProcessManager
{

    /// <summary>
    /// Direzione ordinamento lista processi.
    /// </summary>
    public enum SortOrder
    {
        /// <summary>
        /// Ordine discendente.
        /// </summary>
        Descending = 1,
        /// <summary>
        /// Ordine ascendente.
        /// </summary>
        Ascending = 0
    }

    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Nome utente.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password dell'utente.
        /// </summary>
        public SecureString Password { get; set; }

        /// <summary>
        /// Indice della riga selezionata precedentemente.
        /// </summary>
        private int FormerSelectedRow;

        /// <summary>
        /// Indica se il programma è in chiusura.
        /// </summary>
        public static bool ProgramTerminating { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            ProcessInfoVM VM = DataContext as ProcessInfoVM;
            //VM.CreateHook();
            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            ProgramTerminating = true;
            WatchdogManager.StopProcessWatchdog();
            WatchdogManager.SaveWatchdogSettings();
            ProcessLimiter.ShutdownProcessLimiter();
            ProcessLimiter.SaveProcessLimiterSettings();
            ((ProcessInfoVM)DataContext).ServicesData.StopAllServicesMonitoring();
            ((ProcessInfoVM)DataContext).Dispose();
            LogEntry Entry = BuildLogEntryForInformation("Applicazione terminata", EventAction.ProgramTermination);
            Logger.WriteEntry(Entry);
        }

        private void ProcessDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int RowIndex = UtilityMethods.GetRowIndexFromMouseClick(e.OriginalSource);
            if (RowIndex != -1)
            {
                PropertiesCommand Command = new(ProcessDataGrid.DataContext as ProcessInfoVM);
                Command.Execute(ProcessDataGrid.SelectedItem as ProcessInfo);
            }
        }

        private void ProcessDataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            int RowIndex = UtilityMethods.GetRowIndexFromMouseClick(e.OriginalSource);
            if (RowIndex != -1)
            {
                ProcessDataGrid.SelectedIndex = RowIndex;
                ProcessInfo Info = (ProcessInfo)ProcessDataGrid.Items[RowIndex];
                ShowChildrenProcessesMenuItem.CommandParameter = Info;
                TerminateProcessMenuItem.CommandParameter = Info;
                TerminateProcessTreeMenuItem.CommandParameter = Info;
                if (Info.Name == "ProcessManager.exe")
                {
                    PropertiesMenuItem.IsEnabled = false;
                }
                else
                {
                    PropertiesMenuItem.IsEnabled = true;
                    PropertiesMenuItem.CommandParameter = Info;
                }
                DebugMenuItem.CommandParameter = Info;
                EnableVirtualizationMenuItem.CommandParameter = Info;
                DisableVirtualizationMenuItem.CommandParameter = Info;
                ShowProcessWindowsInfoMenuItem.CommandParameter = Info;
                ShowHostedServicesMenuItem.CommandParameter = Info;
                EmptyWorkingSetMenuItem.CommandParameter = Info;
                SetMaximumWorkingSetSizeMenuItem.CommandParameter = Info;
                SetMinimumWorkingSetSizeMenuItem.CommandParameter = Info;
                ProcessInfo.ProcessPriority Priority = Info.Priority;
                ulong ProcessAffinity = Info.Affinity;
                BitArray AffinityMask = new(BitConverter.GetBytes(ProcessAffinity));
                foreach (MenuItem item in PrioritySubMenu.Items)
                {
                    if (item.IsChecked)
                    {
                        item.IsChecked = false;
                        break;
                    }
                    item.CommandParameter = Info;
                }
                foreach (MenuItem item in AffinitySubMenu.Items)
                {
                    if (item.IsChecked)
                    {
                        item.IsChecked = false;
                        break;
                    }
                    item.CommandParameter = Info;
                }
                switch (Priority)
                {
                    case ProcessInfo.ProcessPriority.RealTime:
                        RealTimePriorityMenuItem.IsChecked = true;
                        break;
                    case ProcessInfo.ProcessPriority.AboveNormal:
                        AboveNormalPriorityMenuItem.IsChecked = true;
                        break;
                    case ProcessInfo.ProcessPriority.High:
                        HighPriorityMenuItem.IsChecked = true;
                        break;
                    case ProcessInfo.ProcessPriority.Normal:
                        NormalPriorityMenuItem.IsChecked = true;
                        break;
                    case ProcessInfo.ProcessPriority.BelowNormal:
                        LowPriorityMenuItem.IsChecked = true;
                        break;
                    case ProcessInfo.ProcessPriority.Idle:
                        IdlePriorityMenuItem.IsChecked = true;
                        break;
                }
                for (int i = 0; i < AffinityMask.Count; i++)
                {
                    if (AffinityMask[i])
                    {
                        foreach (MenuItem Item in AffinitySubMenu.Items)
                        {
                            if (((string)Item.Header).Contains(i.ToString(CultureInfo.CurrentCulture)))
                            {
                                Item.IsChecked = true;
                                break;
                            }
                        }
                    }
                }
            }
            e.Handled = true;
        }

        private void ProcessManagerMainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!SearchTextbox.IsKeyboardFocused)
            {
                if (ProcessDataGrid.Focus())
                {
                    Key PressedKey = e.Key;
                    KeyConverter Converter = new();
                    string KeyChar = Converter.ConvertToString(PressedKey);
                    FormerSelectedRow = ProcessDataGrid.SelectedIndex;
                    int CurrentlySelectedRow = ProcessDataGrid.SelectedIndex;
                    if (e.KeyboardDevice.Modifiers == ModifierKeys.None)
                    {
                        if (char.IsLetterOrDigit(KeyChar, 0))
                        {
                            if (ProcessDataGrid.SelectedIndex == -1)
                            {
                                foreach (ProcessInfo info in ProcessDataGrid.Items)
                                {
                                    if (info.Name.StartsWith(KeyChar, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        ProcessDataGrid.SelectedItem = info;
                                        ProcessDataGrid.ScrollIntoView(info);
                                        CurrentlySelectedRow = ProcessDataGrid.SelectedIndex;
                                        break;
                                    }
                                }
                                FormerSelectedRow = CurrentlySelectedRow;
                            }
                            else
                            {
                                ProcessInfo Info;
                                for (int i = CurrentlySelectedRow + 1; i < ProcessDataGrid.Items.Count; i++)
                                {
                                    Info = (ProcessInfo)ProcessDataGrid.Items[i];
                                    if (Info.Name.StartsWith(KeyChar, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        ProcessDataGrid.SelectedItem = Info;
                                        ProcessDataGrid.ScrollIntoView(Info);
                                        CurrentlySelectedRow = i;
                                        break;
                                    }
                                }
                                if (FormerSelectedRow == CurrentlySelectedRow)
                                {
                                    for (int i = 0; i < ProcessDataGrid.Items.Count; i++)
                                    {
                                        Info = (ProcessInfo)ProcessDataGrid.Items[i];
                                        if (Info.Name.StartsWith(KeyChar, StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            ProcessDataGrid.SelectedItem = Info;
                                            ProcessDataGrid.ScrollIntoView(Info);
                                            CurrentlySelectedRow = i;
                                            break;
                                        }
                                    }
                                    FormerSelectedRow = CurrentlySelectedRow;
                                }
                                else
                                {
                                    FormerSelectedRow = CurrentlySelectedRow;
                                }
                            }
                        }
                    }
                }
                e.Handled = true;
            }
        }

        private void PriorityMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ProcessInfo Info = (ProcessInfo)ProcessDataGrid.Items[ProcessDataGrid.SelectedIndex];
            MenuItem FormerCheckedItem = null;
            MenuItem ClickedItem = (MenuItem)sender;
            foreach (MenuItem item in PrioritySubMenu.Items)
            {
                if (item.IsChecked && !ReferenceEquals(item, ClickedItem))
                {
                    FormerCheckedItem = item;
                    item.IsChecked = false;
                    break;
                }
            }
            if (FormerCheckedItem != null)
            {
                switch (ClickedItem.Header)
                {
                    case "Tempo reale":
                    case "Realtime":
                        FormerCheckedItem.IsChecked = !Info.SetProcessPriority(ProcessInfo.ProcessPriority.RealTime);
                        break;
                    case "Alta":
                    case "High":
                        FormerCheckedItem.IsChecked = !Info.SetProcessPriority(ProcessInfo.ProcessPriority.High);
                        break;
                    case "Sopra il normale":
                    case "Above normal":
                        FormerCheckedItem.IsChecked = !Info.SetProcessPriority(ProcessInfo.ProcessPriority.AboveNormal);
                        break;
                    case "Normale":
                    case "Normal":
                        FormerCheckedItem.IsChecked = !Info.SetProcessPriority(ProcessInfo.ProcessPriority.Normal);
                        break;
                    case "Bassa":
                    case "Low":
                        FormerCheckedItem.IsChecked = !Info.SetProcessPriority(ProcessInfo.ProcessPriority.BelowNormal);
                        break;
                    case "Inattivo":
                    case "Idle":
                        FormerCheckedItem.IsChecked = !Info.SetProcessPriority(ProcessInfo.ProcessPriority.Idle);
                        break;
                }
            }
            else
            {
                ClickedItem.IsChecked = true;
            }
        }

        private void ServiceCreationNotificationMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            ((ProcessInfoVM)DataContext).EnableServiceCreationNotifications();
        }

        private void ServiceCreationNotificationMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            ((ProcessInfoVM)DataContext).DisableServiceCreationNotifications();
        }

        private void ServiceDeletionNotificationMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            ((ProcessInfoVM)DataContext).EnableServiceDeletionNotifications();
        }

        private void ServiceDeletionNotificationMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            ((ProcessInfoVM)DataContext).DisableServiceDeletionNotifications();
        }

        private void ProcessDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.Item is ProcessInfo Info)
            {
                if (Info.Name is not "System Idle Process" and not "System" and not "Secure System" and not "Registry" and not "Memory Compression")
                {
                    StringBuilder TooltipBuilder = new();
                    if (Info.CommandLine != Properties.Resources.UnavailableText)
                    {
                        _ = TooltipBuilder.AppendLine(Info.CommandLine);
                    }
                    if (CheckDataAvailability("File", Info))
                    {
                        if (TooltipBuilder.Length > 0)
                        {
                            _ = TooltipBuilder.AppendLine();
                        }
                        _ = TooltipBuilder.AppendLine("File:");
                        if (Info.FullPath != Properties.Resources.UnavailableText)
                        {
                            _ = TooltipBuilder.AppendLine("\t" + Info.FullPath);
                        }
                        if (Info.Description != Properties.Resources.UnavailableText)
                        {
                            _ = TooltipBuilder.Append("\t" + Info.Description);
                        }
                        if (Info.Version != Properties.Resources.UnavailableText)
                        {
                            _ = TooltipBuilder.AppendLine(" " + Info.Version);
                        }
                        if (Info.CompanyName != Properties.Resources.UnavailableText)
                        {
                            _ = TooltipBuilder.AppendLine("\t" + Info.CompanyName);
                        }
                    }
                    if ((Info.Name.Contains("dllhost") || Info.Name.Contains("Dllhost")) && (Info.FullPath.Contains("System32") || Info.FullPath.Contains("system32")))
                    {
                        if (Info.ComInterfaceName != Properties.Resources.UnavailableText)
                        {
                            if (TooltipBuilder.Length > 0)
                            {
                                _ = TooltipBuilder.AppendLine();
                            }
                            _ = TooltipBuilder.AppendLine("Com target:");
                            _ = TooltipBuilder.AppendLine("\t" + Info.ComInterfaceName);
                        }
                    }
                    if (CheckDataAvailability("OtherInfo", Info))
                    {
                        if (TooltipBuilder.Length > 0)
                        {
                            _ = TooltipBuilder.AppendLine();
                        }
                        _ = TooltipBuilder.AppendLine(Properties.Resources.OtherInfoText);
                        if (Info.PackageName != Properties.Resources.UnavailableText && Info.PackageName != Properties.Resources.NoneText)
                        {
                            _ = TooltipBuilder.AppendLine("\tPackage: " + Info.PackageName);
                        }
                        if (Info.IsProcessInJob.Value)
                        {
                            _ = TooltipBuilder.AppendLine("\t" + Properties.Resources.ProcessIs32BitText);
                        }
                        if (Info.Is32BitProcess.Value)
                        {
                            _ = TooltipBuilder.AppendLine("\t" + Properties.Resources.ProcessIsInAJobText);
                        }
                        if (Info.IsProcessElevated.Value)
                        {
                            _ = TooltipBuilder.AppendLine("\t" + Properties.Resources.ProcessIsElevatedText);
                        }
                    }
                    if (TooltipBuilder.Length > 0)
                    {
                        e.Row.ToolTip = TooltipBuilder.ToString();
                    }
                }
                else
                {
                    e.Row.ToolTip = null;
                }
            }
        }

        private void SearchTextbox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter)
            {
                ((ProcessInfoVM)DataContext).FilterValue = SearchTextbox.Text;
                ((ProcessInfoVM)DataContext).FilterProcessList();
            }
        }

        private void ProcessDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ = ProcessDataGrid.Focus();
            if (e.AddedItems.Count > 0)
            {
                ProcessDataGrid.ScrollIntoView(e.AddedItems[0]);
            }
        }

        /// <summary>
        /// Controla se almeno uno dei dati del tipo indicato sono disponibili per un processo.
        /// </summary>
        /// <param name="Type">Tipo di dati da controllare.</param>
        /// <param name="Info">Istanza di <see cref="ProcessInfo"/> che rappresenta il processo.</param>
        /// <returns>true se uno o più dei dati del tipo richiesto sono disponibili, false altrimenti.</returns>
        private static bool CheckDataAvailability(string Type, ProcessInfo Info)
        {
            bool IsDataAvailable = true;
            switch (Type)
            {
                case "File":
                    if (Info.FullPath == Properties.Resources.UnavailableText)
                    {
                        IsDataAvailable = false;
                    }
                    if (Info.Description == Properties.Resources.UnavailableText)
                    {
                        IsDataAvailable = false;
                    }
                    if (Info.Version == Properties.Resources.UnavailableText)
                    {
                        IsDataAvailable = false;
                    }
                    if (Info.CompanyName == Properties.Resources.UnavailableText)
                    {
                        IsDataAvailable = false;
                    }
                    break;
                case "OtherInfo":
                    if (Info.PackageName == Properties.Resources.UnavailableText && Info.PackageName == Properties.Resources.NoneText)
                    {
                        IsDataAvailable = false;
                    }
                    if (Info.IsProcessInJob.HasValue)
                    {
                        IsDataAvailable = false;
                    }
                    if (Info.Is32BitProcess.HasValue)
                    {
                        IsDataAvailable = false;
                    }
                    if (Info.IsProcessElevated.HasValue)
                    {
                        IsDataAvailable = false;
                    }
                    break;
            }
            return IsDataAvailable;
        }
    }
}