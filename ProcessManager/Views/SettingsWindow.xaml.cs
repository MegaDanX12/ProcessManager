using ProcessManager.ViewModels;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            CheckData();
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\Settings.xml"))
            {
                BuildSettingsXML();
            }
            else
            {
                Settings.ProcessDataUpdateRate = Convert.ToUInt32(DataUpdateRateTextbox.Text, CultureInfo.CurrentCulture);
                Settings.SafeMode = SafeModeCheckbox.IsChecked.Value;
                Settings.AllowProcessMemoryManipulation = MemoryManipulationCheckbox.IsChecked.Value;
                Settings.ServiceMonitoringEnabled = ServiceMonitoringCheckbox.IsChecked.Value;
                Settings.LogProgramActivity = LoggingEnabledCheckbox.IsChecked.Value;
                Settings.LogsPath = LogsPathTextbox.Text;
                Settings.KeepOldLogs = KeepOldLogsCheckbox.IsChecked.Value;
                Settings.MaxLogSize = Convert.ToUInt32(MaxLogSizeTextbox.Text, CultureInfo.CurrentCulture);
                Settings.LoggingLevel = (LogLevel)LogLevelCombobox.SelectedIndex;
                Settings.ForceLogOffIfHung = ForceLogoffIfHungCheckbox.IsChecked.Value;
                Settings.ForceOtherSessionsLogOffOnShutdown = ForceOtherSessionsLogOffOnShutdownCheckbox.IsChecked.Value;
                Settings.ForceCurrentSessionLogOffOnShutdown = ForceCurrentSessionsLogOffOnShutdownCheckbox.IsChecked.Value;
                Settings.InstallUpdatesBeforeShutdown = InstallUpdatesBeforeShutdownCheckbox.IsChecked.Value;
                Settings.ManualPowerDown = ManualPowerDownCheckbox.IsChecked.Value;
                Settings.AutomaticUpdatesCheck = AutomaticUpdateCheckCheckbox.IsChecked.Value;
                Settings.UpdateCheckRate = (Settings.UpdateCheckRateValues)UpdateCheckRateCombobox.SelectedIndex;
                string[] HourComponents = UpdateHourTextbox.Text.Split(':');
                Settings.UpdateHour = new(1, 1, 1, Convert.ToInt32(HourComponents[0], CultureInfo.CurrentCulture), Convert.ToInt32(HourComponents[1], CultureInfo.CurrentCulture), 0);
                Settings.UpdateDayOfWeek = (Settings.WeekDay)UpdateWeekDayCombobox.SelectedIndex;
                Settings.UpdateDownloadOnly = DownloadOnlyCheckbox.IsChecked.Value;
                Settings.UpdateDownloadCompletedNotifications = UpdateCompletedDownloadNotificationsCheckbox.IsChecked.Value;
                Settings.UpdateDownloadAfterConfirmation = UpdateDownloadAfterConfirmationCheckbox.IsChecked.Value;
                Settings.UpdateInstallStartedNotifications = UpdateInstallStartedNotificationsCheckbox.IsChecked.Value;
                Settings.UpdateInstallAfterConfirmation = UpdateInstallAfterConfirmationCheckbox.IsChecked.Value;
                Settings.RestartAfterUpdate = RestartAfterUpdateCheckbox.IsChecked.Value;
                if ((ProcessDataSource)DataSourceCombobox.SelectedIndex != Settings.DataSource)
                {
                    ProcessInfoVM.HasDataSourceChanged = true;
                }
                Settings.DataSource = (ProcessDataSource)DataSourceCombobox.SelectedIndex;
                Settings.UpdateSettingsFile();
            }
            if (Settings.LogProgramActivity)
            {
                Logger.Initialize(Settings.LogsPath);
            }
            Close();
        }

        /// <summary>
        /// Controlla la validità dei dati forniti e li sostituisce con i valori predefiniti se non sono validi.
        /// </summary>
        private void CheckData()
        {
            if (string.IsNullOrWhiteSpace(DataUpdateRateTextbox.Text) || !uint.TryParse(DataUpdateRateTextbox.Text, out _))
            {
                DataUpdateRateTextbox.Text = "500";
            }
            if (string.IsNullOrWhiteSpace(MaxLogSizeTextbox.Text) || !uint.TryParse(MaxLogSizeTextbox.Text, out _))
            {
                MaxLogSizeTextbox.Text = "50";
            }
            if (string.IsNullOrWhiteSpace(LogsPathTextbox.Text))
            {
                LogsPathTextbox.Text = AppDomain.CurrentDomain.BaseDirectory;
            }
            if (string.IsNullOrWhiteSpace(UpdateHourTextbox.Text))
            {
                UpdateHourTextbox.Text = "15:00";
            }
            else
            {
                if (UpdateHourTextbox.Text.Length is 5)
                {
                    if (UpdateHourTextbox.Text.Contains(":"))
                    {
                        int ColonCount = 0;
                        foreach (char character in UpdateHourTextbox.Text)
                        {
                            if (character is ':')
                            {
                                ColonCount += 1;
                            }
                        }
                        if (ColonCount is not 1)
                        {
                            UpdateHourTextbox.Text = "15:00";
                        }
                        else
                        {
                            if (UpdateHourTextbox.Text[2] is ':')
                            {
                                string[] HourComponents = UpdateHourTextbox.Text.Split(':');
                                bool ValidHour = uint.TryParse(HourComponents[0], out uint Hour) && (Hour is >= 0 and <= 23);
                                bool ValidMinute = uint.TryParse(HourComponents[1], out uint Minute) && (Minute is >= 0 and <= 59);
                                if (!ValidHour || !ValidMinute)
                                {
                                    UpdateHourTextbox.Text = "15:00";
                                }
                            }
                            else
                            {
                                UpdateHourTextbox.Text = "15:00";
                            }
                        }
                    }
                    else
                    {
                        UpdateHourTextbox.Text = "15:00";
                    }
                }
                else
                {
                    UpdateHourTextbox.Text = "15:00";
                }
            }
        }

        /// <summary>
        /// Costruisce il contenuto del file XML che contiene le impostazioni.
        /// </summary>
        private void BuildSettingsXML()
        {
            string DocumentText =
                "<Settings>" + Environment.NewLine +
                "   <General>" + Environment.NewLine +
                "       <ProcessDataUpdateRate>" + DataUpdateRateTextbox.Text + "</ProcessDataUpdateRate>" + Environment.NewLine +
                "       <SafeMode>" + SafeModeCheckbox.IsChecked.Value.ToString() + "</SafeMode>" + Environment.NewLine +
                "       <AllowProcessMemoryManipulation>" + MemoryManipulationCheckbox.IsChecked.Value.ToString() + "</AllowProcessMemoryManipulation>" + Environment.NewLine +
                "       <ServiceMonitoringEnabled>" + ServiceMonitoringCheckbox.IsChecked.Value.ToString() + "</ServiceMonitoringEnabled>" + Environment.NewLine +
                "       <ForceLogOffIfHung>" + ForceLogoffIfHungCheckbox.IsChecked.Value.ToString() + "</ForceLogOffIfHung>" + Environment.NewLine +
                "       <ForceOtherSessionsLogOffOnShutdown>" + ForceOtherSessionsLogOffOnShutdownCheckbox.IsChecked.Value.ToString() + "</ForceOtherSessionsLogOffOnShutdown>" + Environment.NewLine +
                "       <ForceCurrentSessionLogOffOnShutdown>" + ForceCurrentSessionsLogOffOnShutdownCheckbox.IsChecked.Value.ToString() + "</ForceCurrentSessionLogOffOnShutdown>" + Environment.NewLine +
                "       <InstallUpdatesBeforeShutdown>" + InstallUpdatesBeforeShutdownCheckbox.IsChecked.Value.ToString() + "</InstallUpdatesBeforeShutdown>" + Environment.NewLine +
                "       <ManualPowerDown>" + ManualPowerDownCheckbox.IsChecked.Value.ToString() + "</ManualPowerDown>" + Environment.NewLine +
                "       <DataSource>" + DataSourceCombobox.SelectedIndex.ToString(CultureInfo.CurrentCulture) + "</DataSource>" + Environment.NewLine +
                "   </General>" + Environment.NewLine +
                "   <Logging>" + Environment.NewLine +
                "       <LogProgramActivity>" + LoggingEnabledCheckbox.IsChecked.Value.ToString() + "</LogProgramActivity>" + Environment.NewLine +
                "       <LogsPath>" + LogsPathTextbox.Text + "</LogsPath>" + Environment.NewLine +
                "       <KeepOldLogs>" + KeepOldLogsCheckbox.IsChecked.Value.ToString() + "</KeepOldLogs>" + Environment.NewLine +
                "       <MaxLogSize>" + MaxLogSizeTextbox.Text + "</MaxLogSize>" + Environment.NewLine +
                "       <LogLevel>" + Enum.GetName(typeof(LogLevel), LogLevelCombobox.SelectedIndex) + "</LogLevel>" + Environment.NewLine +
                "   </Logging>" + Environment.NewLine +
                "   <Update>" + Environment.NewLine +
                "       <AutomaticUpdatesCheck>" + AutomaticUpdateCheckCheckbox.IsChecked.Value.ToString() + "</AutomaticUpdatesCheck>" + Environment.NewLine +
                "       <CheckRate>" + UpdateCheckRateCombobox.SelectedIndex.ToString(CultureInfo.CurrentCulture) + "</CheckRate>" + Environment.NewLine +
                "       <Hour>" + UpdateHourTextbox.Text + "</Hour>" + Environment.NewLine +
                "       <DayOfWeek>" + UpdateWeekDayCombobox.SelectedIndex.ToString(CultureInfo.CurrentCulture) + "</DayOfWeek>" + Environment.NewLine +
                "       <DownloadOnly>" + DownloadOnlyCheckbox.IsChecked.Value.ToString() + "</DownloadOnly>" + Environment.NewLine +
                "       <DownloadCompletedNotifications>" + UpdateCompletedDownloadNotificationsCheckbox.IsChecked.Value.ToString() + "</DownloadCompletedNotifications>" + Environment.NewLine +
                "       <DownloadAfterConfirmation>" + UpdateDownloadAfterConfirmationCheckbox.IsChecked.Value.ToString() + "</DownloadAfterConfirmation>" + Environment.NewLine +
                "       <InstallStartedNotifications>" + UpdateInstallStartedNotificationsCheckbox.IsChecked.Value.ToString() + "</InstallStartedNotifications>" + Environment.NewLine +
                "       <InstallAfterConfirmation>" + UpdateInstallAfterConfirmationCheckbox.IsChecked.Value.ToString() + "</InstallAfterConfirmation>" + Environment.NewLine +
                "       <RestartAfterUpdate>" + RestartAfterUpdateCheckbox.IsChecked.Value.ToString() + "</RestartAfterUpdate>" + Environment.NewLine +
                "   </Update>" + Environment.NewLine +
                "</Settings>";
            XDocument Doc = XDocument.Parse(DocumentText);
            Doc.Save(AppDomain.CurrentDomain.BaseDirectory + "\\Settings.xml");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LogLevelCombobox.ItemsSource = Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>();
        }

        private void SafeModeCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            MessageBoxResult Result = MessageBox.Show(Properties.Resources.SafeModeDisablementMessage, Properties.Resources.SafeModeDisablementTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (Result == MessageBoxResult.No)
            {
                SafeModeCheckbox.IsChecked = true;
            }
        }

        private void MemoryManipulationCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            MessageBoxResult Result = MessageBox.Show(Properties.Resources.MemoryInfoManipulationEnablementMessage, Properties.Resources.MemoryInfoManipulationEnablementTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (Result == MessageBoxResult.No)
            {
                MemoryManipulationCheckbox.IsChecked = false;
            }
        }
    }
}