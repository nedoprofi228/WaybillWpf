// Создайте папку Converters/
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WaybillWpf.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Проверяем, есть ли параметр
            bool invert = false;
            if (parameter != null && bool.TryParse(parameter.ToString(), out bool p))
            {
                invert = p;
            }

            bool boolValue = (bool)value;

            if (invert)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}