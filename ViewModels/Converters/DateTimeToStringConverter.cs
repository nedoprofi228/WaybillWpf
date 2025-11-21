using System;
using System.Globalization;
using System.Windows.Data;

namespace WaybillWpf.ViewModels.Converters
{
    [ValueConversion(typeof(DateTime), typeof(string))]
    public class DateTimeToStringConverter : IValueConverter
    {
        private const string Format = "dd.MM.yyyy HH:mm";

        // Из ViewModel во View (DateTime -> String)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                return date.ToString(Format);
            }
            return string.Empty;
        }

        // Из View во ViewModel (String -> DateTime)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                // Пытаемся распарсить именно в том формате, который мы ожидаем
                if (DateTime.TryParseExact(str, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                {
                    return result;
                }
            }
            // Если пользователь ввел ерунду, возвращаем "ничего", чтобы не ломать программу
            return System.Windows.DependencyProperty.UnsetValue;
        }
    }
}