using Microsoft.Win32.SafeHandles;
using ProcessManager.Models;
using ProcessManager.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per ProcessPropertiesWindowNoNet.xaml
    /// </summary>
    public partial class ProcessPropertiesWindowNoNet : Window
    {
        public ProcessPropertiesWindowNoNet(SafeProcessHandle Handle, Dictionary<string, string> EnabledSettings, ProcessPropertiesVM VM)
        {
            DataContext = VM;
            InitializeComponent();
            GeneraTabDetailsButton.CommandParameter = EnabledSettings;
            Tuple<SafeProcessHandle, string> ImageInfoCommandParameter = new(Handle, NativeHelpers.GetProcessFullPathNT(Handle));
            ImageInfoButton.CommandParameter = ImageInfoCommandParameter;
            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
            ((ProcessPropertiesVM)DataContext).Info.Exit += Info_Exit;
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {

        }

        private void Info_Exit(object sender, EventArgs e)
        {
            Close();
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            ((ProcessPropertiesVM)DataContext).Info.Exit -= Info_Exit;
            ((ProcessPropertiesVM)DataContext).Dispose();
        }

        private void ThreadsDatagrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ProcessPropertiesVM VM = (ProcessPropertiesVM)DataContext;
            int SelectedIndex = UtilityMethods.GetRowIndexFromMouseClick(e.OriginalSource);
            ThreadInfo BindingSource = VM.ThreadsInfo[SelectedIndex];
            StartLabel.Content = VM.ThreadsInfo[SelectedIndex].CreationTime;
            BasePriorityLabel.Content = VM.ThreadsInfo[SelectedIndex].BasePriority;
            Binding CyclesBinding = new("CycleTime")
            {
                Source = BindingSource
            };
            _ = CyclesLabel.SetBinding(ContentProperty, CyclesBinding);
            Binding KernelTimeBinding = new("KernelTime")
            {
                Source = BindingSource
            };
            _ = KernelTimeLabel.SetBinding(ContentProperty, KernelTimeBinding);
            Binding UserTimeBinding = new("UserTime")
            {
                Source = BindingSource
            };
            _ = UserTimeLabel.SetBinding(ContentProperty, UserTimeBinding);
            Binding DynamicPriorityBinding = new("DynamicPriority")
            {
                Source = BindingSource
            };
            _ = DynamicPriorityLabel.SetBinding(ContentProperty, DynamicPriorityBinding);
            Binding IdealProcessorBinding = new("IdealProcessor")
            {
                Source = BindingSource
            };
            _ = IdealProcessorLabel.SetBinding(ContentProperty, IdealProcessorBinding);
        }

        private void GeneralHandler(object sender, RoutedEventArgs e)
        {
            ((CheckBox)sender).IsChecked = ((CheckBox)sender).IsChecked.Value ? false : true;
        }

        private void GroupsDatagrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            GroupsDatagrid.SelectedIndex = -1;
        }

        private void ThreadsDatagrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            int RowIndex = UtilityMethods.GetRowIndexFromMouseClick(e.OriginalSource);
            if (RowIndex != -1)
            {
                ThreadsDatagrid.SelectedIndex = RowIndex;
                TerminateThreadMenuItem.CommandParameter = ThreadsDatagrid.Items[RowIndex];
                SuspendThreadMenuItem.CommandParameter = ThreadsDatagrid.Items[RowIndex];
                ResumeThreadMenuItem.CommandParameter = ThreadsDatagrid.Items[RowIndex];
                WindowsMenuItem.CommandParameter = ThreadsDatagrid.Items[RowIndex];
                string Priority = ((ThreadInfo)ThreadsDatagrid.Items[RowIndex]).Priority;
                ulong Affinity = ((ThreadInfo)ThreadsDatagrid.Items[RowIndex]).Affinity;
                BitArray AffinityMask = new(BitConverter.GetBytes(Affinity));
                foreach (MenuItem item in PrioritySubMenu.Items)
                {
                    if (item.IsChecked)
                    {
                        item.IsChecked = false;
                        break;
                    }
                    item.CommandParameter = ThreadsDatagrid.Items[RowIndex];
                }
                foreach (MenuItem item in AffinitySubMenu.Items)
                {
                    if (item.IsChecked)
                    {
                        item.IsChecked = false;
                        break;
                    }
                    item.CommandParameter = ThreadsDatagrid.Items[RowIndex];
                }
                switch (Priority)
                {
                    case "TimeCritical":
                        TimeCriticalPriorityMenuItem.IsChecked = true;
                        break;
                    case "Highest":
                        HighPriorityMenuItem.IsChecked = true;
                        break;
                    case "AboveNormal":
                        AboveNormalPriorityMenuItem.IsChecked = true;
                        break;
                    case "Normal":
                        NormalPriorityMenuItem.IsChecked = true;
                        break;
                    case "BelowNormal":
                        BelowNormalPriorityMenuItem.IsChecked = true;
                        break;
                    case "Lowest":
                        LowPriorityMenuItem.IsChecked = true;
                        break;
                    case "Idle":
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

        private void PriorityMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ThreadInfo Info = (ThreadInfo)ThreadsDatagrid.Items[ThreadsDatagrid.SelectedIndex];
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
                    case "Critica":
                        FormerCheckedItem.IsChecked = !Info.SetThreadPriority("Critica");
                        break;
                    case "Alta":
                        FormerCheckedItem.IsChecked = !Info.SetThreadPriority("Alta");
                        break;
                    case "Sopra il normale":
                        FormerCheckedItem.IsChecked = !Info.SetThreadPriority("Sopra il normale");
                        break;
                    case "Normale":
                        FormerCheckedItem.IsChecked = !Info.SetThreadPriority("Normale");
                        break;
                    case "Sotto al normale":
                        FormerCheckedItem.IsChecked = !Info.SetThreadPriority("Sotto al normale");
                        break;
                    case "Bassa":
                        FormerCheckedItem.IsChecked = !Info.SetThreadPriority("Bassa");
                        break;
                    case "Inattivo":
                        FormerCheckedItem.IsChecked = !Info.SetThreadPriority("Inattivo");
                        break;
                }
            }
            else
            {
                ClickedItem.IsChecked = true;
            }
        }

        private void PrivilegesDatagrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            int RowIndex = UtilityMethods.GetRowIndexFromMouseClick(e.OriginalSource);
            PrivilegesDatagrid.SelectedIndex = RowIndex;
            ProcessPropertiesVM VM = (ProcessPropertiesVM)DataContext;
            Tuple<TokenInfo, int> Parameters = new(VM.TokenInfo, RowIndex);
            if (RowIndex != -1)
            {
                PrivilegesDatagrid.SelectedIndex = RowIndex;
                EnablePrivilegeContextMenuItem.CommandParameter = Parameters;
                DisablePrivilegeContextMenuItem.CommandParameter = Parameters;
                RemovePrivilegeContextMenuItem.CommandParameter = Parameters;
            }
            e.Handled = true;
        }

        private void ModulesDatagrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            int RowIndex = UtilityMethods.GetRowIndexFromMouseClick(e.OriginalSource);
            ModulesDatagrid.SelectedIndex = RowIndex;
            ProcessPropertiesVM VM = (ProcessPropertiesVM)DataContext;
            ModuleInfo Info = (ModuleInfo)ModulesDatagrid.Items[RowIndex];
            Tuple<SafeProcessHandle, string> Parameters = new(VM.Handle, Info.FullPath);
            if (RowIndex != -1)
            {
                PEHeaderInfoContextMenuItem.CommandParameter = Parameters;
                OpenFolderContextMenuItem.CommandParameter = Info.FullPath;
                FilePropertiesContextMenuItem.CommandParameter = Info;
            }
            e.Handled = true;
        }

        private void MemoryDatagrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            int RowIndex = UtilityMethods.GetRowIndexFromMouseClick(e.OriginalSource);
            MemoryDatagrid.SelectedIndex = RowIndex;
            if (RowIndex != -1)
            {
                FreeMemoryMenuItem.CommandParameter = MemoryDatagrid.Items[RowIndex];
                DecommitMemoryMenuItem.CommandParameter = MemoryDatagrid.Items[RowIndex];
            }
            e.Handled = true;
        }

        private void ProtectionChangeHandler(object sender, RoutedEventArgs e)
        {
            ProcessPropertiesVM VM = (ProcessPropertiesVM)DataContext;
            if (!VM.ChangeMemoryRegionProtection((MemoryRegionInfo)MemoryDatagrid.Items[MemoryDatagrid.SelectedIndex], (string)((MenuItem)sender).Header))
            {
                _ = MessageBox.Show(Properties.Resources.MemoryRegionChangeProtectionErrorMessage, Properties.Resources.MemoryRegionChangeProtectionErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HandlesDatagrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            int RowIndex = UtilityMethods.GetRowIndexFromMouseClick(e.OriginalSource);
            HandlesDatagrid.SelectedIndex = RowIndex;
            if (RowIndex != -1)
            {
                CloseHandleMenuItem.CommandParameter = HandlesDatagrid.Items[RowIndex];
                HandlePropertiesMenuItem.CommandParameter = HandlesDatagrid.Items[RowIndex];
            }
            e.Handled = true;
        }
    }
}