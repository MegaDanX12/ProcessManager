using ProcessManager.Watchdog;
using System.IO;
using System.Linq;
using System.Windows;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per AddProcessEnergyUsageRuleCommand.xaml
    /// </summary>
    public partial class AddProcessEnergyUsageRuleWindow : Window
    {
        /// <summary>
        /// Indica se la finestra è stata aperta per modificare una regola.
        /// </summary>
        private readonly bool EditMode;

        public AddProcessEnergyUsageRuleWindow(ProcessEnergyUsage Rule = null)
        {
            InitializeComponent();
            if (Rule is not null)
            {
                EditMode = true;
                ProcessNameTextbox.Text = Rule.Name;
                ProcessNameTextbox.IsEnabled = false;
                KeepDisplayOnCheckbox.IsChecked = Rule.KeepDisplayOn;
                KeepSystemInWorkingStateCheckbox.IsChecked = Rule.KeepSystemInWorkingState;
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
                    if (WatchdogManager.AddProcessEnergyUsageRule(ProcessNameTextbox.Text, KeepDisplayOnCheckbox.IsChecked.Value, KeepSystemInWorkingStateCheckbox.IsChecked.Value))
                    {
                        Close();
                    }
                    else
                    {
                        _ = MessageBox.Show(Properties.Resources.EnergyUsageRuleAlreadyExistsMessageText, Properties.Resources.EnergyUsageRuleAlreadyExistMessageTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    WatchdogManager.EditProcessEnergyUsageRule(ProcessNameTextbox.Text, KeepDisplayOnCheckbox.IsChecked.Value, KeepSystemInWorkingStateCheckbox.IsChecked.Value);
                    Close();
                }
            }
            else
            {
                _ = MessageBox.Show(Properties.Resources.InvalidEnergyUsageRuleDataMessageText, Properties.Resources.InvalidEnergyUsageRuleDataMessageTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Controlla che i dati siano validi.
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
            if (KeepSystemInWorkingStateCheckbox.IsChecked == false && KeepDisplayOnCheckbox.IsChecked == true)
            {
                KeepSystemInWorkingStateCheckbox.IsChecked = true;
            }
            return true;
        }
    }
}