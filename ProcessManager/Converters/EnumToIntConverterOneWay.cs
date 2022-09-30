using System;
using System.Globalization;
using System.Windows.Data;

namespace ProcessManager.Converters
{
    public class EnumToIntConverterOneWay : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ChangeType(value, typeof(int), culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}