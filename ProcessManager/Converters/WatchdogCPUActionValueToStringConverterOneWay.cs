using ProcessManager.Watchdog;
using ProcessManager.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ProcessManager.Converters
{
    public class WatchdogCPUActionValueToStringConverterOneWay : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((WatchdogAction)values[0] is WatchdogAction.ChangeAffinity)
            {
                return UtilityMethods.BuildAffinityString((ulong)values[1]);
            }
            else
            {
                return (ProcessInfo.ProcessPriority)values[1] switch
                {
                    ProcessInfo.ProcessPriority.RealTime => Properties.Resources.ProcessPriorityRealTimeText,
                    ProcessInfo.ProcessPriority.AboveNormal => Properties.Resources.ProcessPriorityAboveNormalText,
                    ProcessInfo.ProcessPriority.BelowNormal => Properties.Resources.ProcessPriorityBelowNormalText,
                    ProcessInfo.ProcessPriority.High => Properties.Resources.ProcessPriorityHighText,
                    ProcessInfo.ProcessPriority.Idle => Properties.Resources.ProcessPriorityIdleText,
                    ProcessInfo.ProcessPriority.Normal => Properties.Resources.ProcessPriorityNormalText,
                    ProcessInfo.ProcessPriority.Unknown => Properties.Resources.UnknownText,
                    _ => "Invalid"
                };
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}