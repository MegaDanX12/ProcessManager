using ProcessManager.Watchdog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per AddDisallowedProcessWindow.xaml
    /// </summary>
    public partial class AddDisallowedProcessWindow : Window
    {
        /// <summary>
        /// Indica se la finestra è stata aperta per modificare un istanza già esistente.
        /// </summary>
        private readonly bool EditMode;

        public AddDisallowedProcessWindow()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsDataValid())
            {
                if (WatchdogManager.AddDisallowedProcess(ProcessNameTextbox.Text, NotificationOnTerminationCheckbox.IsChecked.Value))
                {
                    Close();
                }
                else
                {
                    _ = MessageBox.Show(Properties.Resources.DisallowedProcessAlreadyExistsMessageText, Properties.Resources.DisallowedProcessAlreadyExistsMessageTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                _ = MessageBox.Show(Properties.Resources.DisallowedProcessInvalidDataMessageText, Properties.Resources.DisallowedProcessInvalidDataMessageTitle, MessageBoxButton.OK, MessageBoxImage.Error);
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
            return true;
        }
    }
}