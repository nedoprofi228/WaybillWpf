using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WaybillWpf.Converters // Убедитесь, что namespace совпадает с вашей папкой
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class CustomBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 1. Проверяем параметр. Если там написано "Inverted", включаем режим инверсии
            bool invert = parameter is string paramString && paramString.Equals("Inverted", StringComparison.OrdinalIgnoreCase);

            // 2. Проверяем само значение
            if (value is bool boolValue)
            {
                if (invert)
                {
                    // Режим Inverted: Если True -> Скрываем, Если False -> Показываем
                    return boolValue ? Visibility.Collapsed : Visibility.Visible;
                }
                else
                {
                    // Обычный режим: Если True -> Показываем, Если False -> Скрываем
                    return boolValue ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            return Visibility.Collapsed; // Значение по умолчанию, если пришло null
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool invert = parameter is string paramString && paramString.Equals("Inverted", StringComparison.OrdinalIgnoreCase);

                if (invert)
                    return visibility != Visibility.Visible;
                else
                    return visibility == Visibility.Visible;
            }
            return false;
        }
    }
}