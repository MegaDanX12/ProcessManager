using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace ProcessManager.Converters
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            StringBuilder TimeSpanString = new();
            TimeSpan Value = (TimeSpan)value;
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
            }
            return TimeSpanString.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}