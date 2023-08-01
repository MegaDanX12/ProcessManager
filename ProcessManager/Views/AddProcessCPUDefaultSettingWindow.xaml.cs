using ProcessManager.Models;
using ProcessManager.Watchdog;
using System.Linq;
using System.Windows;
using System.IO;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per AddProcessCPUDefaultSettingWindow.xaml
    /// </summary>
    public partial class AddProcessCPUDefaultSettingWindow : Window
    {

        /// <summary>
        /// Conteggio core del processore.
        /// </summary>
        private readonly uint? CoreCount;

        /// <summary>
        /// Indica se è in corso la modifica di una regola esistente.
        /// </summary>
        private readonly bool EditMode;

        public AddProcessCPUDefaultSettingWindow(ProcessDefaultCPUSettings Setting = null)
        {
            CoreCount = NativeHelpers.GetNumberOfProcessorCores();
            InitializeComponent();
            _ = PriorityCombobox.Items.Add(Properties.Resources.ProcessPriorityIdleText);
            _ = PriorityCombobox.Items.Add(Properties.Resources.ProcessPriorityBelowNormalText);
            _ = PriorityCombobox.Items.Add(Properties.Resources.ProcessPriorityNormalText);
            _ = PriorityCombobox.Items.Add(Properties.Resources.ProcessPriorityAboveNormalText);
            _ = PriorityCombobox.Items.Add(Properties.Resources.ProcessPriorityHighText);
            _ = PriorityCombobox.Items.Add(Properties.Resources.ProcessPriorityRealTimeText);
            if (!CoreCount.HasValue)
            {
                AffinityTextbox.IsEnabled = false;
            }
            if (Setting is not null)
            {
                EditMode = true;
                ProcessNameTextbox.Text = Setting.Name;
                ProcessNameTextbox.IsEnabled = false;
                if (Setting.DefaultPriority.HasValue)
                {
                    switch (Setting.DefaultPriority)
                    {
                        case ProcessInfo.ProcessPriority.Idle:
                            PriorityCombobox.SelectedIndex = 0;
                            break;
                        case ProcessInfo.ProcessPriority.BelowNormal:
                            PriorityCombobox.SelectedIndex = 1;
                            break;
                        case ProcessInfo.ProcessPriority.Normal:
                            PriorityCombobox.SelectedIndex = 2;
                            break;
                        case ProcessInfo.ProcessPriority.AboveNormal:
                            PriorityCombobox.SelectedIndex = 3;
                            break;
                        case ProcessInfo.ProcessPriority.High:
                            PriorityCombobox.SelectedIndex = 4;
                            break;
                        case ProcessInfo.ProcessPriority.RealTime:
                            PriorityCombobox.SelectedIndex = 5;
                            break;
                    }
                }
                if (Setting.DefaultAffinity.HasValue)
                {
                    string Affinity = UtilityMethods.BuildAffinityString(Setting.DefaultAffinity.Value);
                    AffinityTextbox.Text = Affinity;
                }
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
                ProcessInfo.ProcessPriority? Priority = null;
                ulong? Affinity = null;
                if (PriorityCombobox.SelectedIndex is not -1)
                {
                    switch (PriorityCombobox.SelectedIndex)
                    {
                        case 0:
                            Priority = ProcessInfo.ProcessPriority.Idle;
                            break;
                        case 1:
                            Priority = ProcessInfo.ProcessPriority.BelowNormal;
                            break;
                        case 2:
                            Priority = ProcessInfo.ProcessPriority.Normal;
                            break;
                        case 3:
                            Priority = ProcessInfo.ProcessPriority.AboveNormal;
                            break;
                        case 4:
                            Priority = ProcessInfo.ProcessPriority.High;
                            break;
                        case 5:
                            Priority = ProcessInfo.ProcessPriority.RealTime;
                            break;
                    }
                }
                if (!string.IsNullOrWhiteSpace(AffinityTextbox.Text))
                {
                    Affinity = UtilityMethods.GetAffinityValue(AffinityTextbox.Text);
                }
                if (!EditMode)
                {
                    if (WatchdogManager.AddProcessCPUDefaultSetting(ProcessNameTextbox.Text, Priority, Affinity))
                    {
                        Close();
                    }
                    else
                    {
                        _ = MessageBox.Show(Properties.Resources.ProcessCPUDefaultSettingAlreadyExistMessageText, Properties.Resources.ProcessCPUDefaultSettingAlreadyExistMessageTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    WatchdogManager.EditProcessCPUDefaultSetting(ProcessNameTextbox.Text, Priority, Affinity);
                    Close();
                }
            }
            else
            {
                _ = MessageBox.Show(Properties.Resources.InvalidCPUDefaultSettingMessageText, Properties.Resources.InvalidCPUDefaultSettingMessageTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Controlla se i dati forniti sono validi.
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
            if (PriorityCombobox.SelectedIndex is -1 && string.IsNullOrWhiteSpace(AffinityTextbox.Text))
            {
                return false;
            }
            if (!string.IsNullOrWhiteSpace(AffinityTextbox.Text))
            {
                string[] Cores = AffinityTextbox.Text.Split(',');
                if (Cores.Length is 1)
{
                    if (uint.TryParse(Cores[0], out uint Result))
{
                        if (Result > CoreCount - 1)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    string[] CoreRangeComponents;
                    foreach (string core in Cores)
                    {
                        if (core.Contains("-"))
                        {
                            if (core.Count(character => character is '-') is 1)
                            {
                                CoreRangeComponents = core.Split('-');
                                foreach (string component in CoreRangeComponents)
                                {
                                    if (uint.TryParse(component, out uint Result))
                                    {
                                        if (Result > CoreCount - 1)
                                        {
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        return false;
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
                            if (uint.TryParse(core, out uint Result))
                            {
                                if (Result > CoreCount - 1)
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}