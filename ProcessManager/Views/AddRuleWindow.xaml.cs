using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.IO;
using ProcessManager.Models;
using System.Globalization;
using ProcessManager.Watchdog;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per AddRuleWindow.xaml
    /// </summary>
    public partial class AddRuleWindow : Window
    {
        /// <summary>
        /// Dimensione della memoria di sistema.
        /// </summary>
        private readonly uint SystemMemorySize;

        /// <summary>
        /// Conteggio core del processore.
        /// </summary>
        private readonly uint CoreCount;

        /// <summary>
        /// Associazione tra i valori validi della priorità del processo associati all'azione del watchdog CPU e i valori enumerativi compresi nell'enumerazione <see cref="ProcessInfo.ProcessPriority"/>.
        /// </summary>
        private readonly Dictionary<string, ProcessInfo.ProcessPriority> StringPriorityAssociations = new()
        {
            { "AboveNormal", ProcessInfo.ProcessPriority.AboveNormal },
            { "BelowNormal", ProcessInfo.ProcessPriority.BelowNormal },
            { "High", ProcessInfo.ProcessPriority.High },
            { "Idle", ProcessInfo.ProcessPriority.Idle },
            { "Normal", ProcessInfo.ProcessPriority.Normal },
            { "RealTime", ProcessInfo.ProcessPriority.RealTime }
        };

        /// <summary>
        /// Associazione tra le stringhe rappresentati le azioni del watchdog e il valori enumerativi compresi nell'enumerazione <see cref="WatchdogAction"/>.
        /// </summary>
        private readonly Dictionary<string, WatchdogAction> StringWatchdogActionAssociations = new()
        {
            { Properties.Resources.WatchdogActionsChangeAffinityText, WatchdogAction.ChangeAffinity },
            { Properties.Resources.WatchdogActionsChangePriorityText, WatchdogAction.ChangePriority },
            { Properties.Resources.WatchdogActionsEmptyWorkingSetText, WatchdogAction.EmptyWorkingSet },
            { Properties.Resources.WatchdogActionsTerminateProcessText, WatchdogAction.TerminateProcess }
        };

        /// <summary>
        /// Regola da modificare.
        /// </summary>
        private readonly ProcessWatchdogRule RuleToEdit;

        public AddRuleWindow(uint SystemMemorySize, uint CoreCount, ProcessWatchdogRule Rule = null)
        {
            this.SystemMemorySize = SystemMemorySize;
            this.CoreCount = CoreCount;
            RuleToEdit = Rule;
            InitializeComponent();
            _ = CpuActionCombobox.Items.Add(Properties.Resources.WatchdogActionsChangeAffinityText);
            _ = CpuActionCombobox.Items.Add(Properties.Resources.WatchdogActionsChangePriorityText);
            _ = CpuActionCombobox.Items.Add(Properties.Resources.WatchdogActionsTerminateProcessText);
            _ = MemoryActionCombobox.Items.Add(Properties.Resources.WatchdogActionsEmptyWorkingSetText);
            _ = MemoryActionCombobox.Items.Add(Properties.Resources.WatchdogActionsTerminateProcessText);
            if (Rule is not null)
            {
                if (Rule.Settings.CpuWatchdogEnabled)
                {
                    CpuUsageMaxPercentageTextbox.Text = Rule.Settings.CpuWatchdogValue.ToString("D0", CultureInfo.CurrentCulture);
                    CpuWatchdogControlTimeTextbox.Text = Rule.Settings.CpuWatchdogTime.ToString("D0", CultureInfo.CurrentCulture);
                    if (Rule.CPUAction.ActionType is WatchdogAction.ChangeAffinity)
                    {
                        CpuActionCombobox.SelectedIndex = 0;
                        CpuActionValueTextbox.Text = UtilityMethods.BuildAffinityString((ulong)Rule.CPUAction.ActionValue);
                    }
                    else if (Rule.CPUAction.ActionType is WatchdogAction.ChangePriority)
                    {
                        CpuActionCombobox.SelectedIndex = 1;
                        switch ((ProcessInfo.ProcessPriority)Rule.CPUAction.ActionValue)
                        {
                            case ProcessInfo.ProcessPriority.RealTime:
                                CpuActionValueTextbox.Text = "RealTime";
                                break;
                            case ProcessInfo.ProcessPriority.AboveNormal:
                                CpuActionValueTextbox.Text = "AboveNormal";
                                break;
                            case ProcessInfo.ProcessPriority.High:
                                CpuActionValueTextbox.Text = "High";
                                break;
                            case ProcessInfo.ProcessPriority.Normal:
                                CpuActionValueTextbox.Text = "Normal";
                                break;
                            case ProcessInfo.ProcessPriority.BelowNormal:
                                CpuActionValueTextbox.Text = "BelowNormal";
                                break;
                            case ProcessInfo.ProcessPriority.Idle:
                                CpuActionValueTextbox.Text = "Idle";
                                break;
                        }
                    }
                    else if (Rule.CPUAction.ActionType is WatchdogAction.TerminateProcess)
                    {
                        CpuActionCombobox.SelectedIndex = 2;
                        CpuActionValueTextbox.Text = string.Empty;
                    }
                }
                else
                {
                    CpuUsageMaxPercentageTextbox.Text = "0";
                    CpuWatchdogControlTimeTextbox.Text = "0";
                    CpuActionCombobox.SelectedIndex = -1;
                    CpuActionValueTextbox.Text = string.Empty;
                }
                if (Rule.Settings.MemoryWatchdogEnabled)
                {
                    MemoryUsageMaxValueTextbox.Text = Rule.Settings.MemoryWatchdogValue.ToString("D0", CultureInfo.CurrentCulture);
                    MemoryWatchdogControlTimeTextbox.Text = Rule.Settings.MemoryWatchdogTime.ToString("D0", CultureInfo.CurrentCulture);
                    switch (Rule.MemoryAction.ActionType)
                    {
                        case WatchdogAction.EmptyWorkingSet:
                            MemoryActionCombobox.SelectedIndex = 0;
                            break;
                        case WatchdogAction.TerminateProcess:
                            MemoryActionCombobox.SelectedIndex = 1;
                            break;
                    }
                }
                else
                {
                    MemoryUsageMaxValueTextbox.Text = "0";
                    MemoryWatchdogControlTimeTextbox.Text = "0";
                    MemoryActionCombobox.SelectedIndex = -1;
                }
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsDataValid(out string Reason))
            {
                uint CpuMaxUsage = uint.Parse(CpuUsageMaxPercentageTextbox.Text, CultureInfo.CurrentCulture);
                uint CpuWatchdogControlTime = uint.Parse(CpuWatchdogControlTimeTextbox.Text, CultureInfo.CurrentCulture);
                uint MemoryMaxUsage = uint.Parse(MemoryUsageMaxValueTextbox.Text, CultureInfo.CurrentCulture) / 1024 / 1024;
                uint MemoryWatchdogControlTime = uint.Parse(MemoryWatchdogControlTimeTextbox.Text, CultureInfo.CurrentCulture);
                bool CpuWatchdogEnabled = CpuMaxUsage is not 0 && CpuWatchdogControlTime is not 0;
                bool MemoryWatchdogEnabled = MemoryMaxUsage is not 0 && MemoryWatchdogControlTime is not 0;
                if (!CpuWatchdogEnabled && !MemoryWatchdogEnabled)
                {
                    _ = MessageBox.Show(Properties.Resources.NoWatchdogActiveErrorMessage, Properties.Resources.NoWatchdogActiveErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    WatchdogSettings Settings = new(CpuWatchdogEnabled, CpuMaxUsage, CpuWatchdogControlTime, MemoryWatchdogEnabled, MemoryMaxUsage, MemoryWatchdogControlTime);
                    object CpuWatchdogValue = (string)CpuActionCombobox.SelectedItem == Properties.Resources.WatchdogActionsChangeAffinityText ?
                        UtilityMethods.GetAffinityValue((string)CpuActionCombobox.SelectedItem) :
                        StringPriorityAssociations[(string)CpuActionCombobox.SelectedItem];
                    ProcessWatchdogAction CpuAction = new(StringWatchdogActionAssociations[(string)CpuActionCombobox.SelectedItem], CpuWatchdogValue);
                    ProcessWatchdogAction MemoryAction = new(StringWatchdogActionAssociations[(string)MemoryActionCombobox.SelectedItem], null);
                    ProcessWatchdogRule Rule = new(ProcessNameTextbox.Text, Settings, CpuAction, MemoryAction);
                    if (RuleToEdit is null)
                    {
                        WatchdogManager.AddRule(Rule);
                    }
                    else
                    {
                        WatchdogManager.EditRule(Rule, RuleToEdit);
                    }
                    Close();
                }
            }
            else
            {
                string Message;
                switch (Reason)
                {
                    case "NoProcessName":
                        Message = Properties.Resources.InvalidWatchdogRuleDataErrorMessage + Properties.Resources.InvalidRuleReasonNoProcessName;
                        _ = MessageBox.Show(Message, Properties.Resources.InvalidWatchdogRuleDataErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    case "ProcessNameNoExtension":
                    case "ProcessNameInvalidExtension":
                        Message = Properties.Resources.InvalidWatchdogRuleDataErrorMessage + Properties.Resources.InvalidRuleReasonProcessNameNoExtension;
                        _ = MessageBox.Show(Message, Properties.Resources.InvalidWatchdogRuleDataErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    case "InvalidCpuMaxUsage":
                        Message = Properties.Resources.InvalidWatchdogRuleDataErrorMessage + Properties.Resources.InvalidRuleReasonCpuUsageMaxInvalid;
                        _ = MessageBox.Show(Message, Properties.Resources.InvalidWatchdogRuleDataErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    case "InvalidCpuWatchdogControlTime":
                        Message = Properties.Resources.InvalidWatchdogRuleDataErrorMessage + Properties.Resources.InvalidRuleReasonCpuControlTimeInvalid;
                        _ = MessageBox.Show(Message, Properties.Resources.InvalidWatchdogRuleDataErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    case "InvalidMemoryMaxUsage":
                        Message = Properties.Resources.InvalidWatchdogRuleDataErrorMessage + Properties.Resources.InvalidRuleReasonMemoryUsageMaxInvalid;
                        _ = MessageBox.Show(Message, Properties.Resources.InvalidWatchdogRuleDataErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    case "InvalidMemoryWatchdogControlTime":
                        Message = Properties.Resources.InvalidWatchdogRuleDataErrorMessage + Properties.Resources.InvalidRuleReasonMemoryControlTimeInvalid;
                        _ = MessageBox.Show(Message, Properties.Resources.InvalidWatchdogRuleDataErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    case "NoCpuActionSelected":
                        Message = Properties.Resources.InvalidWatchdogRuleDataErrorMessage + Properties.Resources.InvalidRuleReasonCpuActionNotSelected;
                        _ = MessageBox.Show(Message, Properties.Resources.InvalidWatchdogRuleDataErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    case "InvalidCoresValue":
                        Message = Properties.Resources.InvalidWatchdogRuleDataErrorMessage + Properties.Resources.InvalidRuleReasonCpuActionValueAffinity;
                        _ = MessageBox.Show(Message, Properties.Resources.InvalidWatchdogRuleDataErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    case "NoCoresValue":
                        Message = Properties.Resources.InvalidWatchdogRuleDataErrorMessage + Properties.Resources.InvalidRuleReasonNoCpuActionValueAffinity;
                        _ = MessageBox.Show(Message, Properties.Resources.InvalidWatchdogRuleDataErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    case "InvalidPriorityValue":
                        Message = Properties.Resources.InvalidWatchdogRuleDataErrorMessage + Properties.Resources.InvalidRuleReasonCpuActionValuePriority;
                        _ = MessageBox.Show(Message, Properties.Resources.InvalidWatchdogRuleDataErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    case "NoPriorityValue":
                        Message = Properties.Resources.InvalidWatchdogRuleDataErrorMessage + Properties.Resources.InvalidRuleReasonNoCpuActionValuePriority;
                        _ = MessageBox.Show(Message, Properties.Resources.InvalidWatchdogRuleDataErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    case "NoMemoryActionSelected":
                        Message = Properties.Resources.InvalidWatchdogRuleDataErrorMessage + Properties.Resources.InvalidRuleReasonMemoryActionNotSelected;
                        _ = MessageBox.Show(Message, Properties.Resources.InvalidWatchdogRuleDataErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    case "ProcessNameCurrentProcess":
                        Message = Properties.Resources.InvalidWatchdogRuleDataErrorMessage + Properties.Resources.InvalidRuleReasonCurrentProcessName;
                        _ = MessageBox.Show(Message, Properties.Resources.InvalidWatchdogRuleDataErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    case "PathProvided":
                        Message = Properties.Resources.InvalidWatchdogRuleDataErrorMessage + Properties.Resources.InvalidRuleReasonInvalidPath;
                        _ = MessageBox.Show(Message, Properties.Resources.InvalidWatchdogRuleDataErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Controlla che i dati forniti per la regola del watchdog siano validi e fornisce una motivazione in caso contrario.
        /// </summary>
        /// <param name="InvalidReason">Motivazione per cui i dati non sono validi.</param>
        /// <returns>true se i dati sono validi, false altrimenti.</returns>
        private bool IsDataValid(out string InvalidReason)
        {
            if (string.IsNullOrWhiteSpace(ProcessNameTextbox.Text))
            {
                InvalidReason = "NoProcessName";
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
                            InvalidReason = "ProcessNameNoExtension";
                            return false;
                        }
                        else
                        {
                            if (Path.GetExtension(ProcessNameTextbox.Text) is not ".exe")
                            {
                                InvalidReason = "ProcessNameInvalidExtension";
                                return false;
                            }
                            else
                            {
                                if (Path.GetFileNameWithoutExtension(ProcessNameTextbox.Text) is "ProcessManager")
                                {
                                    InvalidReason = "ProcessNameCurrentProcess";
                                    return false;
                                }
                            }
                        }
                    }
                    else
                    {
                        InvalidReason = "PathProvided";
                        return false;
                    }
                }
                else
                {
                    InvalidReason = "PathProvided";
                    return false;
                }
            }
            if (uint.TryParse(CpuUsageMaxPercentageTextbox.Text, out uint Result))
            {
                if (Result is > 100)
                {
                    InvalidReason = "InvalidCpuMaxUsage";
                    return false;
                }
            }
            else
            {
                InvalidReason = "InvalidCpuMaxUsage";
                return false;
            }
            if (!uint.TryParse(CpuWatchdogControlTimeTextbox.Text, out Result))
            {
                InvalidReason = "InvalidCpuWatchdogControlTime";
                return false;
            }
            if (uint.TryParse(MemoryUsageMaxValueTextbox.Text, out Result))
            {
                if (Result > SystemMemorySize)
                {
                    InvalidReason = "InvalidMemoryMaxUsage";
                    return false;
                }
            }
            else
            {
                InvalidReason = "InvalidMemoryMaxUsage";
                return false;
            }
            if (!uint.TryParse(MemoryWatchdogControlTimeTextbox.Text, out Result))
            {
                InvalidReason = "InvalidMemoryWatchdogControlTime";
                return false;
            }
            if (CpuActionCombobox.SelectedIndex is -1)
            {
                InvalidReason = "NoCpuActionSelected";
                return false;
            }
            else
            {
                if (CpuActionCombobox.SelectedIndex is 0)
                {
                    if (!string.IsNullOrWhiteSpace(CpuActionValueTextbox.Text))
                    {
                        string[] Cores = CpuActionValueTextbox.Text.Split(',');
                        if (Cores.Length is 1)
                        {
                            if (uint.TryParse(Cores[0], out Result))
                            {
                                if (Result > CoreCount - 1)
                                {
                                    InvalidReason = "InvalidCoresValue";
                                    return false;
                                }
                            }
                            else
                            {
                                InvalidReason = "InvalidCoresValue";
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
                                            if (uint.TryParse(component, out Result))
                                            {
                                                if (Result > CoreCount - 1)
                                                {
                                                    InvalidReason = "InvalidCoresValue";
                                                    return false;
                                                }
                                            }
                                            else
                                            {
                                                InvalidReason = "InvalidCoresValue";
                                                return false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        InvalidReason = "InvalidCoresValue";
                                        return false;
                                    }
                                    
                                }
                                else
{
                                    if (uint.TryParse(core, out Result))
                                    {
                                        if (Result > CoreCount - 1)
                                        {
                                            InvalidReason = "InvalidCoresValue";
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        InvalidReason = "InvalidCoresValue";
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        InvalidReason = "NoCoresValue";
                        return false;
                    }
                }
                else if (CpuActionCombobox.SelectedIndex is 1)
                {
                    if (!string.IsNullOrWhiteSpace(CpuActionValueTextbox.Text))
                    {
                        if (!StringPriorityAssociations.ContainsKey(CpuActionValueTextbox.Text))
                        {
                            InvalidReason = "InvalidPriorityValue";
                            return false;
                        }
                    }
                    else
                    {
                        InvalidReason = "NoPriorityValue";
                        return false;
                    }
                }
            }
            if (MemoryActionCombobox.SelectedIndex is -1)
            {
                InvalidReason = "NoMemoryActionSelected";
                return false;
            }
            InvalidReason = "NoReason";
            return true;
        }
    }
}