using ProcessManager.ViewModels;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per LogEntriesFilterSettingsWindow.xaml
    /// </summary>
    public partial class LogEntriesFilterSettingsWindow : Window
    {
        public LogEntriesFilterSettingsWindow(LogManagerDataVM VM)
        {
            DataContext = VM;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DateFilterValueDatePicker.FirstDayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            SeverityFilterValueComboBox.ItemsSource = Enum.GetValues(typeof(EventSeverity)).Cast<EventSeverity>();
            SourceFilterValueComboBox.ItemsSource = Enum.GetValues(typeof(EventSource)).Cast<EventSource>();
            ActionFilterValueComboBox.ItemsSource = Enum.GetValues(typeof(EventAction)).Cast<EventAction>();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (CheckFilterData())
            {
                LogManagerDataVM VM = DataContext as LogManagerDataVM;
                VM.FilterSettings.TextFilter = TextCheckBox.IsChecked.Value;
                if (VM.FilterSettings.TextFilter)
                {
                    VM.FilterSettings.TextFilterValue = TextFilterValueTextBox.Text;
                }
                VM.FilterSettings.DateFilter = DateCheckBox.IsChecked.Value;
                if (VM.FilterSettings.DateFilter)
                {
                    VM.FilterSettings.DateFilterValue = DateFilterValueDatePicker.SelectedDate.Value;
                }
                VM.FilterSettings.HourFilter = HourCheckBox.IsChecked.Value;
                if (VM.FilterSettings.HourFilter)
                {
                    string[] StartTimeSections = StartHourFilterValueTextBox.Text.Split(':');
                    string[] EndTimeSections = EndHourFilterValueTextBox.Text.Split(':');
                    int StartHourComponent = Convert.ToInt32(StartTimeSections[0]);
                    int StartMinuteComponent = Convert.ToInt32(StartTimeSections[1]);
                    VM.FilterSettings.StartHourFilterValue = new(1, 1, 1, StartHourComponent, StartMinuteComponent, 0);
                    int EndHourComponent = Convert.ToInt32(EndTimeSections[0]);
                    int EndMinuteComponent = Convert.ToInt32(EndTimeSections[1]);
                    VM.FilterSettings.EndHourFilterValue = new(1, 1, 1, EndHourComponent, EndMinuteComponent, 0);
                }
                VM.FilterSettings.SeverityFilter = SeverityCheckBox.IsChecked.Value;
                if (VM.FilterSettings.SeverityFilter)
                {
                    VM.FilterSettings.SeverityFilterValue = (EventSeverity)SeverityFilterValueComboBox.SelectedIndex;
                }
                VM.FilterSettings.SourceFilter = SourceCheckBox.IsChecked.Value;
                if (VM.FilterSettings.SourceFilter)
                {
                    VM.FilterSettings.EventSourceValue = (EventSource)SourceFilterValueComboBox.SelectedIndex;
                }
                VM.FilterSettings.ActionFilter = ActionCheckBox.IsChecked.Value;
                if (VM.FilterSettings.ActionFilter)
                {
                    VM.FilterSettings.ActionFilterValue = (EventAction)ActionFilterValueComboBox.SelectedIndex;
                }
                Close();
            }
            else
            {
                MessageBox.Show(Properties.Resources.InvalidFilterDataErrorMessage, Properties.Resources.InvalidFilterDataErrorTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Controlla che i dati forniti per i filtri siano validi.
        /// </summary>
        /// <returns>true se i dati sono validi, false altrimenti.</returns>
        private bool CheckFilterData()
        {
            DateTime CurrentTime = DateTime.Now;
            if (TextCheckBox.IsChecked.Value)
            {
                if (string.IsNullOrWhiteSpace(TextFilterValueTextBox.Text))
                {
                    return false;
                }
            }
            if (DateCheckBox.IsChecked.Value)
            {
                if (DateFilterValueDatePicker.SelectedDate == null || DateFilterValueDatePicker.SelectedDate.Value > CurrentTime.Date)
                {
                    return false;
                }
            }
            if (HourCheckBox.IsChecked.Value)
            {
                if (!string.IsNullOrWhiteSpace(StartHourFilterValueTextBox.Text) && !string.IsNullOrWhiteSpace(EndHourFilterValueTextBox.Text))
                {

                    if (!CheckHourFilter(StartHourFilterValueTextBox.Text, CurrentTime))
                    {
                        return false;
                    }
                    if (EndHourFilterValueTextBox.Text != StartHourFilterValueTextBox.Text)
                    {
                        if (!CheckHourFilter(EndHourFilterValueTextBox.Text, CurrentTime))
                        {
                            return false;
                        }
                    }
                    if (!CheckHourFilterOrder(StartHourFilterValueTextBox.Text, EndHourFilterValueTextBox.Text))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
                
            }
            if (SeverityCheckBox.IsChecked.Value)
            {
                if (SeverityFilterValueComboBox.SelectedIndex == -1)
                {
                    return false;
                }
            }
            if (SourceCheckBox.IsChecked.Value)
            {
                if (SourceFilterValueComboBox.SelectedIndex == -1)
                {
                    return false;
                }
            }
            if (ActionCheckBox.IsChecked.Value)
            {
                if (ActionFilterValueComboBox.SelectedIndex == -1)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Controlla che l'ora di inizio sia precedente all'ora di fine del filtro dell'ora.
        /// </summary>
        /// <param name="StartHourFilterValue">Ora di inizio come stringa.</param>
        /// <param name="EndHourFilterValue">Ora di fine come stringa.</param>
        /// <returns>true se l'ora di inizio è precedente all'ora di fine, false altrimenti.</returns>
        private static bool CheckHourFilterOrder(string StartHourFilterValue, string EndHourFilterValue)
        {
            string[] StartTimeSections = StartHourFilterValue.Split(':');
            string[] EndTimeSections = EndHourFilterValue.Split(':');
            if (StartTimeSections.Length != 2 || EndTimeSections.Length != 2)
            {
                return false;
            }
            else
            {
                DateTime StartHour;
                DateTime EndHour;
                int StartHourComponent = Convert.ToInt32(StartTimeSections[0]);
                int StartMinuteComponent = Convert.ToInt32(StartTimeSections[1]);
                StartHour = new(1, 1, 1, StartHourComponent, StartMinuteComponent, 0);
                int EndHourComponent = Convert.ToInt32(EndTimeSections[0]);
                int EndMinuteComponent = Convert.ToInt32(EndTimeSections[1]);
                EndHour = new(1, 1, 1, EndHourComponent, EndMinuteComponent, 0);
                if (EndHour < StartHour)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Controlla che l'ora indicata nel filtro sia valida.
        /// </summary>
        /// <param name="FilterValue">Stringa da controllare.</param>
        /// <param name="CurrentTime">Ora corrente.</param>
        /// <returns>true se l'ora è valida, false altrimenti.</returns>
        private static bool CheckHourFilter(string FilterValue, DateTime CurrentTime)
        {
            string[] TimeSections = FilterValue.Split(':');
            if (TimeSections.Length != 2)
            {
                return false;
            }
            else
            {
                int HourComponent;
                int MinuteComponent = 0;
                if (int.TryParse(TimeSections[0], out HourComponent) || int.TryParse(TimeSections[1], out MinuteComponent))
                {
                    if (HourComponent < 0 || HourComponent > 23)
                    {
                        return false;
                    }
                    if (MinuteComponent < 0 || MinuteComponent > 59)
                    {
                        return false;
                    }
                    if (HourComponent > CurrentTime.Hour)
                    {
                        return false;
                    }
                    else if (HourComponent == CurrentTime.Hour && MinuteComponent > CurrentTime.Minute)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            TextCheckBox.IsChecked = false;
            DateCheckBox.IsChecked = false;
            HourCheckBox.IsChecked = false;
            SeverityCheckBox.IsChecked = false;
            SourceCheckBox.IsChecked = false;
            ActionCheckBox.IsChecked = false;
            LogManagerDataVM VM = DataContext as LogManagerDataVM;
            VM.Entries = VM.FilterSettings.ResetFilters();
        }
    }
}