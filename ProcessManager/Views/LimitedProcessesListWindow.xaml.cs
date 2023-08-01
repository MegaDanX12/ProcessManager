using ProcessManager.Watchdog;
using ProcessManager.Models;
using ProcessManager.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per LimitedProcessesListWindow.xaml
    /// </summary>
    public partial class LimitedProcessesListWindow : Window
    {
        public LimitedProcessesListWindow()
        {
            DataContextChanged += LimitedProcessesListWindow_DataContextChanged;
            InitializeComponent();
        }

        private void LimitedProcessesListWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is not null)
            {
                ((LimitedProcessesListVM)DataContext).WindowDispatcher = Dispatcher;
                ((LimitedProcessesListVM)DataContext).SubscribeToChangeEvents();
            }
        }

        private void ProcessDatagrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            int RowIndex = UtilityMethods.GetRowIndexFromMouseClick(e.OriginalSource);
            if (RowIndex is not -1)
            {
                Tuple<string, byte> SelectedItem = (Tuple<string, byte>)ProcessDatagrid.Items[RowIndex];
                CpuUsageLimitsData Limit = ProcessLimiter.ProcessCpuLimits.FirstOrDefault(limit => limit.UsageLimit == SelectedItem.Item2);
                if (Limit is not null)
                {
                    ProcessInfo Info = Limit.LimitedProcesses.FirstOrDefault(info => info.FullPath == SelectedItem.Item1);
                    if (Info is not null)
                    {
                        TerminateProcessMenuItem.CommandParameter = Info;
                        ShowLimitInfoMenuItem.CommandParameter = Limit;
                    }
                    else
                    {
                        ProcessDatagrid.UnselectAll();
                    }
                }
                else
                {
                    ProcessDatagrid.UnselectAll();
                }
            }
        }
    }
}