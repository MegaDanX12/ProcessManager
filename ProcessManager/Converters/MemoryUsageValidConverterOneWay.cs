using System;
using System.Globalization;
using System.Windows.Data;

namespace ProcessManager.Converters
{
    public class MemoryUsageValidConverterOneWay : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string Text = value as string;
            return !string.IsNullOrWhiteSpace(Text) && Text != "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}