using ProcessManager.InfoClasses.ServicesInfo;
using ProcessManager.Models;
using ProcessManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per ServicesInfoWindow.xaml
    /// </summary>
    public partial class ServicesInfoWindow : Window
    {
        private int FormerSelectedRow;

        public ServicesInfoWindow(HostedServicesDataVM VM)
        {
            DataContext = VM;
            Services.ServiceDeleted += Services_ServiceDeleted;
            Services.ServiceAdded += Services_ServiceAdded;
            if (VM.Info != null)
            {
                VM.Info.Exit += Info_Exit;
            }
            InitializeComponent();
        }

        private void Services_ServiceAdded(object sender, Service e)
        {
            ((HostedServicesDataVM)DataContext).Services.Add(e);
        }

        private void Services_ServiceDeleted(object sender, Service e)
        {
            foreach (Service service in ServicesInfoDataGrid.Items)
            {
                if (service == e)
                {
                    ((HostedServicesDataVM)DataContext).Services.Remove(service);
                }
            }
            if (((HostedServicesDataVM)DataContext).Services.Count == 0)
            {
                Close();
            }
        }

        private void Info_Exit(object sender, EventArgs e)
        {
            Close();
        }

        private void ServicesInfoDataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            int RowIndex = UtilityMethods.GetRowIndexFromMouseClick(e.OriginalSource);
            if (RowIndex != -1)
            {
                ((HostedServicesDataVM)DataContext).CurrentService = (Service)ServicesInfoDataGrid.Items[RowIndex];
                ListenForServiceEventsMenuItem.CommandParameter = ServicesInfoDataGrid.Items[RowIndex];
                StopListeningForServiceEventsMenuItem.CommandParameter = ServicesInfoDataGrid.Items[RowIndex];
                e.Handled = true;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Services.ServiceDeleted -= Services_ServiceDeleted;
            Services.ServiceAdded -= Services_ServiceAdded;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (ServicesInfoDataGrid.Focus())
            {
                Key PressedKey = e.Key;
                KeyConverter Converter = new();
                string KeyChar = Converter.ConvertToString(PressedKey);
                FormerSelectedRow = ServicesInfoDataGrid.SelectedIndex;
                int CurrentlySelectedRow = ServicesInfoDataGrid.SelectedIndex;
                if (e.KeyboardDevice.Modifiers == ModifierKeys.None)
                {
                    if (char.IsLetterOrDigit(KeyChar, 0))
                    {
                        if (ServicesInfoDataGrid.SelectedIndex == -1)
                        {
                            foreach (Service info in ServicesInfoDataGrid.Items)
                            {
                                if (info.Name.StartsWith(KeyChar, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    ServicesInfoDataGrid.SelectedItem = info;
                                    ServicesInfoDataGrid.ScrollIntoView(info);
                                    CurrentlySelectedRow = ServicesInfoDataGrid.SelectedIndex;
                                    break;
                                }
                            }
                            FormerSelectedRow = CurrentlySelectedRow;
                        }
                        else
                        {
                            Service Info;
                            for (int i = CurrentlySelectedRow + 1; i < ServicesInfoDataGrid.Items.Count; i++)
                            {
                                Info = (Service)ServicesInfoDataGrid.Items[i];
                                if (Info.Name.StartsWith(KeyChar, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    ServicesInfoDataGrid.SelectedItem = Info;
                                    ServicesInfoDataGrid.ScrollIntoView(Info);
                                    CurrentlySelectedRow = i;
                                    break;
                                }
                            }
                            if (FormerSelectedRow == CurrentlySelectedRow)
                            {
                                for (int i = 0; i < ServicesInfoDataGrid.Items.Count; i++)
                                {
                                    Info = (Service)ServicesInfoDataGrid.Items[i];
                                    if (Info.Name.StartsWith(KeyChar, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        ServicesInfoDataGrid.SelectedItem = Info;
                                        ServicesInfoDataGrid.ScrollIntoView(Info);
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
}