using ProcessManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per ProcessTokenAdvancedInfoWindow.xaml
    /// </summary>
    public partial class ProcessTokenAdvancedInfoWindow : Window
    {
        public ProcessTokenAdvancedInfoWindow(ProcessPropertiesVM VM)
        {
            DataContext = VM;
            InitializeComponent();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            ProcessPropertiesVM VM = (ProcessPropertiesVM)DataContext;
            if (VM.TokenInfo != null)
            {
                if (VM.TokenInfo.GeneralInfo.IsElevated.HasValue)
                {
                    if (VM.TokenInfo.GeneralInfo.IsElevated.Value)
                    {
                        ElevatedTextBox.Text = Properties.Resources.YesText;
                    }
                    else
                    {
                        ElevatedTextBox.Text = "No";
                    }
                }
                else
                {
                    ElevatedTextBox.Text = Properties.Resources.UnavailableText;
                }
                if (VM.TokenInfo.GeneralInfo.IsVirtualizationAllowed.HasValue)
                {
                    if (VM.TokenInfo.GeneralInfo.IsVirtualizationAllowed.Value)
                    {
                        if (VM.TokenInfo.GeneralInfo.IsVirtualizationEnabled.HasValue)
                        {
                            if (VM.TokenInfo.GeneralInfo.IsVirtualizationEnabled.Value)
                            {
                                VirtualizationTextBox.Text = Properties.Resources.EnabledText;
                            }
                            else
                            {
                                VirtualizationTextBox.Text = Properties.Resources.DisabledText;
                            }
                        }
                        else
                        {
                            VirtualizationTextBox.Text = Properties.Resources.PermittedText;
                        }
                    }
                    else
                    {
                        VirtualizationTextBox.Text = Properties.Resources.NotPermittedText;
                    }
                }
                else
                {
                    VirtualizationTextBox.Text = Properties.Resources.UnavailableText;
                }
            }
            else
            {
                ElevatedTextBox.Text = Properties.Resources.UnavailableText;
                VirtualizationTextBox.Text = Properties.Resources.UnavailableText;
            }
        }

        private void CapabilitiesDatagrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            CapabilitiesDatagrid.SelectedIndex = -1;
        }
    }
}