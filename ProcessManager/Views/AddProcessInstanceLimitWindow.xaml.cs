using ProcessManager.Watchdog;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per AddProcessInstanceLimitWindow.xaml
    /// </summary>
    public partial class AddProcessInstanceLimitWindow : Window
    {
        /// <summary>
        /// Indica se si sta modificando un limite già esistente.
        /// </summary>
        private readonly bool EditMode;

        public AddProcessInstanceLimitWindow(ProcessInstanceLimit Limit = null)
        {
            InitializeComponent();
            if (Limit is not null)
            {
                EditMode = true;
                ProcessNameTextbox.Text = Limit.Name;
                ProcessNameTextbox.IsEnabled = false;
                InstanceLimitTextbox.Text = Limit.InstanceLimit.ToString("D0", CultureInfo.CurrentCulture);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsDataValid())
            {
                if (!EditMode)
                {
                    if (WatchdogManager.AddProcessInstanceLimit(ProcessNameTextbox.Text, Convert.ToUInt32(InstanceLimitTextbox.Text, CultureInfo.CurrentCulture)))
                    {
                        Close();
                    }
                    else
                    {
                        _ = MessageBox.Show(Properties.Resources.InvalidInstanceLimitMessageText, Properties.Resources.InvalidEnergyUsageRuleDataMessageTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    WatchdogManager.EditProcessInstanceLimit(ProcessNameTextbox.Text, Convert.ToUInt32(InstanceLimitTextbox.Text, CultureInfo.CurrentCulture));
                    Close();
                }
            }
            else
            {
                _ = MessageBox.Show(Properties.Resources.InstanceLimitAlreadyExistsMessageText, Properties.Resources.InstanceLimitAlreadyExistsMessageTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Controlla se i dati sono validi.
        /// </summary>
        /// <returns>true se i dati sono validi, false altrimenti.</returns>
        private bool IsDataValid()
        {
            if (string.IsNullOrWhiteSpace(ProcessNameTextbox.Text))
            {
                return false;
            }
            else
            {
                if (!Path.IsPathRooted(ProcessNameTextbox.Text))
                {
                    if (!ProcessNameTextbox.Text.Any(character => character == Path.DirectorySeparatorChar || character == Path.AltDirectorySeparatorChar))
                    {
                        if (!Path.HasExtension(ProcessNameTextbox.Text))
                        {
                            return false;
                        }
                        else
                        {
                            if (Path.GetExtension(ProcessNameTextbox.Text) is not ".exe")
                            {
                                return false;
                            }
                            else
                            {
                                if (Path.GetFileNameWithoutExtension(ProcessNameTextbox.Text) is "ProcessManager")
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            if (uint.TryParse(InstanceLimitTextbox.Text, out uint Result))
            {
                if (Result is 0)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}