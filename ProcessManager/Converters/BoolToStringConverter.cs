using System;
using System.Globalization;
using System.Windows.Data;

namespace ProcessManager.Converters
{
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool Value = (bool)value;
            return Value ? Properties.Resources.YesText : "No";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string Value = value as string;
            return Value == Properties.Resources.YesText;
        }
    }
}