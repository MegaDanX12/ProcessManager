using ProcessManager.Watchdog;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ProcessManager.Converters
{
    public class WatchdogActionToStringConverterOneWay : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (WatchdogAction)value switch
            {
                WatchdogAction.ChangeAffinity => Properties.Resources.WatchdogActionsChangeAffinityText,
                WatchdogAction.ChangePriority => Properties.Resources.WatchdogActionsChangePriorityText,
                WatchdogAction.EmptyWorkingSet => Properties.Resources.WatchdogActionsEmptyWorkingSetText,
                WatchdogAction.TerminateProcess => Properties.Resources.WatchdogActionsTerminateProcessText,
                _ => "Invalid"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}