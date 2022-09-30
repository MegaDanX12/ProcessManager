using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace ProcessManager.Converters
{
    public class TimeSpanToStringMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
{
            StringBuilder TimeSpanString = new();
            TimeSpan Value = (TimeSpan)values[0];
            if (Value == TimeSpan.Zero)
            {
                _ = TimeSpanString.Append(Properties.Resources.UnavailableText);
            }
            else
            {
                if (Value.Days is 1)
                {
                    _ = TimeSpanString.Append(Value.Days).Append(" " + Properties.Resources.DayText + ", ");
                }
                else if (Value.Days is > 1)
                {
                    _ = TimeSpanString.Append(Value.Days).Append(" " + Properties.Resources.DaysText + ", ");
                }
                if (Value.Hours is 1)
                {
                    _ = TimeSpanString.Append(Value.Hours).Append(" " + Properties.Resources.HourText + ", ");
                }
                else if (Value.Hours is > 1)
                {
                    _ = TimeSpanString.Append(Value.Hours).Append(" " + Properties.Resources.HoursText + ", ");
                }
                if (Value.Minutes is 1)
                {
                    _ = TimeSpanString.Append(Value.Minutes).Append(" " + Properties.Resources.MinuteText);
                }
                else if (Value.Minutes is > 1)
                {
                    _ = TimeSpanString.Append(Value.Minutes).Append(" " + Properties.Resources.MinutesText);
                }
                _ = TimeSpanString.Append(' ').Append(((byte)values[1]).ToString("D0", culture)).Append('%');
            }
            return TimeSpanString.ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}